using IMGR.Employee.Application.Abstractions;
using IMGR.Employee.Application.Employees;
using IMGR.Employee.Domain.Employees;
using IMGR.Employee.Domain.Employment;
using IMGR.Employee.Domain.Profiles;
using IMGR.Employee.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace IMGR.Employee.Infrastructure.Persistence;

internal sealed class EfEmployeeReadStore(
    EmployeeDbContext dbContext,
    LegacyFieldProtector protector) : IEmployeeReadStore
{
    public async Task<EmployeePage> SearchAsync(
        EmployeeSearchQuery query,
        CancellationToken cancellationToken)
    {
        var employees = dbContext.Employees.AsNoTracking();
        var legacyStatus = EmployeeStatusCodes.ToLegacyCode(query.Status);
        if (legacyStatus is not null)
        {
            employees = employees.Where(record => record.Status == legacyStatus);
        }

        if (query.Search is not null)
        {
            var plaintext = query.Search;
            var encrypted = protector.Protect(plaintext);
            employees = employees.Where(record =>
                record.EmployeeNumber == plaintext || record.EmployeeNumber == encrypted
                || record.EnglishSurname == plaintext || record.EnglishSurname == encrypted
                || record.EnglishOtherName == plaintext || record.EnglishOtherName == encrypted
                || record.ChineseName == plaintext || record.ChineseName == encrypted
                || record.Alias == plaintext || record.Alias == encrypted);
        }

        var totalCount = await employees.CountAsync(cancellationToken);
        var pageRecords = await employees
            .OrderBy(record => record.EmployeeId)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
        var employeeIds = pageRecords.Select(record => record.EmployeeId).ToArray();
        var assignments = await LoadAssignmentsAsync(employeeIds, cancellationToken);

        var items = pageRecords.Select(record =>
        {
            var employeeAssignments = assignments.GetValueOrDefault(record.EmployeeId) ?? [];
            return new EmployeeSummary(
                record.EmployeeId,
                U(record.EmployeeNumber) ?? string.Empty,
                BuildDisplayName(record),
                EmployeeStatusCodes.Parse(record.Status),
                D(record.DateOfJoin),
                EmploymentAssignmentSelector.SelectAsOf(employeeAssignments, query.AsOfDate));
        }).ToArray();

        return new EmployeePage(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<EmployeeProfile?> GetAsync(
        int employeeId,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees.AsNoTracking()
            .SingleOrDefaultAsync(record => record.EmployeeId == employeeId, cancellationToken);
        if (employee is null)
        {
            return null;
        }

        var assignmentsByEmployee = await LoadAssignmentsAsync([employeeId], cancellationToken);
        var assignments = assignmentsByEmployee.GetValueOrDefault(employeeId) ?? [];
        var contracts = await LoadContractsAsync(employeeId, cancellationToken);
        var bankAccounts = await LoadBankAccountsAsync(employeeId, cancellationToken);
        var spouses = await LoadSpousesAsync(employeeId, cancellationToken);
        var dependants = await LoadDependantsAsync(employeeId, cancellationToken);
        var emergencyContacts = await LoadEmergencyContactsAsync(employeeId, cancellationToken);
        var skills = await LoadSkillsAsync(employeeId, cancellationToken);
        var qualifications = await LoadQualificationsAsync(employeeId, cancellationToken);
        var workExperiences = await LoadWorkExperiencesAsync(employeeId, cancellationToken);
        var documents = await LoadDocumentsAsync(employeeId, cancellationToken);

        return new EmployeeProfile(
            employee.EmployeeId,
            U(employee.EmployeeNumber) ?? string.Empty,
            EmployeeStatusCodes.Parse(employee.Status),
            U(employee.EnglishSurname) ?? string.Empty,
            U(employee.EnglishOtherName) ?? string.Empty,
            U(employee.ChineseName) ?? string.Empty,
            U(employee.Alias) ?? string.Empty,
            SensitiveValueMasker.Mask(U(employee.IdentityNumber)),
            U(employee.Gender),
            U(employee.MaritalStatus),
            D(employee.DateOfBirth),
            U(employee.Nationality),
            SensitiveValueMasker.Mask(U(employee.PassportNumber)),
            D(employee.PassportExpiryDate),
            U(employee.ResidentialAddress),
            U(employee.CorrespondenceAddress),
            D(employee.DateOfJoin),
            D(employee.ServiceDate),
            D(employee.ProbationLastDate),
            U(employee.Email),
            U(employee.InternalEmail),
            U(employee.MobileNumber),
            U(employee.HomePhoneNumber),
            U(employee.OfficePhoneNumber),
            U(employee.Remark),
            EmploymentAssignmentSelector.SelectAsOf(assignments, asOfDate),
            assignments,
            contracts,
            bankAccounts,
            spouses,
            dependants,
            emergencyContacts,
            skills,
            qualifications,
            workExperiences,
            documents);
    }

    public async Task<IReadOnlyList<OrganizationUnit>> GetOrganizationsAsync(
        int? companyId,
        int? levelId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.HierarchyElements.AsNoTracking();
        if (companyId is not null)
        {
            query = query.Where(record => record.CompanyId == companyId);
        }

        if (levelId is not null)
        {
            query = query.Where(record => record.LevelId == levelId);
        }

        var elements = await query.OrderBy(record => record.ElementId).ToListAsync(cancellationToken);
        var companies = await dbContext.Companies.AsNoTracking()
            .Where(record => elements.Select(element => element.CompanyId).Contains(record.CompanyId))
            .ToDictionaryAsync(record => record.CompanyId, cancellationToken);
        var levels = await dbContext.HierarchyLevels.AsNoTracking()
            .Where(record => elements.Select(element => element.LevelId).Contains(record.LevelId))
            .ToDictionaryAsync(record => record.LevelId, cancellationToken);

        return elements.Select(element =>
        {
            companies.TryGetValue(element.CompanyId ?? 0, out var company);
            levels.TryGetValue(element.LevelId ?? 0, out var level);
            return new OrganizationUnit(
                element.ElementId,
                U(element.Code),
                U(element.Description),
                element.CompanyId,
                U(company?.Code),
                U(company?.Name),
                element.LevelId,
                U(level?.Code),
                U(level?.Description),
                level?.Sequence);
        }).ToArray();
    }

    public ValueTask DisposeAsync() => dbContext.DisposeAsync();

    private async Task<Dictionary<int, IReadOnlyList<EmploymentAssignment>>> LoadAssignmentsAsync(
        IReadOnlyCollection<int> employeeIds,
        CancellationToken cancellationToken)
    {
        if (employeeIds.Count == 0)
        {
            return [];
        }

        var positions = await dbContext.Positions.AsNoTracking()
            .Where(record => record.EmployeeId != null && employeeIds.Contains(record.EmployeeId.Value))
            .OrderByDescending(record => record.EffectiveFrom)
            .ThenByDescending(record => record.AssignmentId)
            .ToListAsync(cancellationToken);
        var companies = await LoadDictionaryAsync(
            dbContext.Companies,
            positions.Select(record => record.CompanyId),
            record => record.CompanyId,
            cancellationToken);
        var positionLookups = await LoadDictionaryAsync(
            dbContext.PositionLookups,
            positions.Select(record => record.PositionId),
            record => record.PositionId,
            cancellationToken);
        var ranks = await LoadDictionaryAsync(
            dbContext.Ranks,
            positions.Select(record => record.RankId),
            record => record.RankId,
            cancellationToken);
        var employmentTypes = await LoadDictionaryAsync(
            dbContext.EmploymentTypes,
            positions.Select(record => record.EmploymentTypeId),
            record => record.EmploymentTypeId,
            cancellationToken);

        var assignmentIds = positions.Select(record => record.AssignmentId).ToArray();
        var hierarchies = await dbContext.EmployeeHierarchies.AsNoTracking()
            .Where(record => record.AssignmentId != null && assignmentIds.Contains(record.AssignmentId.Value))
            .ToListAsync(cancellationToken);
        var elements = await LoadDictionaryAsync(
            dbContext.HierarchyElements,
            hierarchies.Select(record => record.ElementId),
            record => record.ElementId,
            cancellationToken);
        var levels = await LoadDictionaryAsync(
            dbContext.HierarchyLevels,
            hierarchies.Select(record => record.LevelId),
            record => record.LevelId,
            cancellationToken);
        var organizationsByAssignment = hierarchies
            .Where(record => record.AssignmentId is not null)
            .GroupBy(record => record.AssignmentId!.Value)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<OrganizationAssignment>)group.Select(record =>
                {
                    elements.TryGetValue(record.ElementId ?? 0, out var element);
                    levels.TryGetValue(record.LevelId ?? element?.LevelId ?? 0, out var level);
                    return new OrganizationAssignment(
                        record.ElementId ?? 0,
                        U(element?.Code),
                        U(element?.Description),
                        record.LevelId ?? element?.LevelId,
                        U(level?.Code),
                        U(level?.Description),
                        level?.Sequence);
                }).OrderBy(item => item.LevelSequence).ToArray());

        return positions
            .Where(record => record.EmployeeId is not null)
            .GroupBy(record => record.EmployeeId!.Value)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<EmploymentAssignment>)group.Select(record =>
                {
                    companies.TryGetValue(record.CompanyId ?? 0, out var company);
                    positionLookups.TryGetValue(record.PositionId ?? 0, out var position);
                    ranks.TryGetValue(record.RankId ?? 0, out var rank);
                    employmentTypes.TryGetValue(record.EmploymentTypeId ?? 0, out var employmentType);
                    return new EmploymentAssignment(
                        record.AssignmentId,
                        D(record.EffectiveFrom),
                        D(record.EffectiveTo),
                        record.CompanyId,
                        U(company?.Code),
                        U(company?.Name),
                        record.PositionId,
                        U(position?.Code),
                        U(position?.Description),
                        record.RankId,
                        U(rank?.Code),
                        U(rank?.Description),
                        record.EmploymentTypeId,
                        U(employmentType?.Code),
                        U(employmentType?.Description),
                        organizationsByAssignment.GetValueOrDefault(record.AssignmentId) ?? []);
                }).ToArray());
    }

    private async Task<IReadOnlyList<EmploymentContract>> LoadContractsAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        var records = await dbContext.Contracts.AsNoTracking()
            .Where(record => record.EmployeeId == employeeId)
            .OrderByDescending(record => record.EmployedFrom)
            .ToListAsync(cancellationToken);
        return records.Select(record => new EmploymentContract(
            record.ContractId,
            U(record.CompanyName),
            U(record.CompanyContactNumber),
            U(record.CompanyAddress),
            D(record.EmployedFrom),
            D(record.EmployedTo),
            record.Gratuity,
            record.CurrencyCode,
            record.GratuityMethod)).ToArray();
    }

    private async Task<IReadOnlyList<EmployeeBankAccount>> LoadBankAccountsAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        var records = await dbContext.BankAccounts.AsNoTracking()
            .Where(record => record.EmployeeId == employeeId)
            .OrderByDescending(record => record.IsDefault)
            .ThenBy(record => record.BankAccountId)
            .ToListAsync(cancellationToken);
        var bankCodes = records.Select(record => U(record.BankCode)).Where(code => code is not null).ToArray();
        var banks = await dbContext.Banks.AsNoTracking()
            .Where(record => bankCodes.Contains(record.BankCode))
            .ToDictionaryAsync(record => record.BankCode, cancellationToken);
        return records.Select(record =>
        {
            var bankCode = U(record.BankCode);
            banks.TryGetValue(bankCode ?? string.Empty, out var bank);
            return new EmployeeBankAccount(
                record.BankAccountId,
                bankCode,
                bank?.Name,
                U(record.BranchCode),
                SensitiveValueMasker.Mask(U(record.AccountNumber)),
                U(record.AccountHolderName),
                record.IsDefault == 1,
                U(record.Remark));
        }).ToArray();
    }

    private async Task<IReadOnlyList<EmployeeSpouse>> LoadSpousesAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.Spouses.AsNoTracking()
            .Where(record => record.EmployeeId == employeeId).ToListAsync(token);
        return records.Select(record => new EmployeeSpouse(
            record.SpouseId, U(record.Surname) ?? string.Empty, U(record.OtherName) ?? string.Empty,
            U(record.ChineseName) ?? string.Empty, SensitiveValueMasker.Mask(U(record.IdentityNumber)),
            SensitiveValueMasker.Mask(U(record.PassportNumber)), D(record.DateOfBirth))).ToArray();
    }

    private async Task<IReadOnlyList<EmployeeDependant>> LoadDependantsAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.Dependants.AsNoTracking()
            .Where(record => record.EmployeeId == employeeId).ToListAsync(token);
        return records.Select(record => new EmployeeDependant(
            record.DependantId, U(record.Surname) ?? string.Empty, U(record.OtherName) ?? string.Empty,
            U(record.ChineseName) ?? string.Empty, U(record.Gender), U(record.Relationship),
            SensitiveValueMasker.Mask(U(record.IdentityNumber)), SensitiveValueMasker.Mask(U(record.PassportNumber)),
            D(record.DateOfBirth))).ToArray();
    }

    private async Task<IReadOnlyList<EmergencyContact>> LoadEmergencyContactsAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.EmergencyContacts.AsNoTracking()
            .Where(record => record.EmployeeId == employeeId).ToListAsync(token);
        return records.Select(record => new EmergencyContact(
            record.EmergencyContactId, U(record.Name) ?? string.Empty, U(record.Gender), U(record.Relationship),
            U(record.DaytimeContactNumber), U(record.NightContactNumber))).ToArray();
    }

    private async Task<IReadOnlyList<EmployeeSkill>> LoadSkillsAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.EmployeeSkills.AsNoTracking()
            .Where(record => record.EmployeeId == employeeId).ToListAsync(token);
        var skills = await LoadDictionaryAsync(dbContext.Skills, records.Select(record => record.SkillId),
            record => record.SkillId, token);
        var levels = await LoadDictionaryAsync(dbContext.SkillLevels, records.Select(record => record.SkillLevelId),
            record => record.SkillLevelId, token);
        return records.Select(record =>
        {
            skills.TryGetValue(record.SkillId ?? 0, out var skill);
            levels.TryGetValue(record.SkillLevelId ?? 0, out var level);
            return new EmployeeSkill(record.EmployeeSkillId, record.SkillId, U(skill?.Code), U(skill?.Description),
                record.SkillLevelId, U(level?.Code), U(level?.Description));
        }).ToArray();
    }

    private async Task<IReadOnlyList<EmployeeQualification>> LoadQualificationsAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.EmployeeQualifications.AsNoTracking()
            .Where(record => record.EmployeeId == employeeId).ToListAsync(token);
        var lookups = await LoadDictionaryAsync(dbContext.Qualifications,
            records.Select(record => record.QualificationId), record => record.QualificationId, token);
        return records.Select(record =>
        {
            lookups.TryGetValue(record.QualificationId ?? 0, out var lookup);
            return new EmployeeQualification(record.EmployeeQualificationId, record.QualificationId,
                U(lookup?.Code), U(lookup?.Description), D(record.From), D(record.To), U(record.Institution),
                U(record.LearningMethod), U(record.Remark));
        }).ToArray();
    }

    private async Task<IReadOnlyList<WorkExperience>> LoadWorkExperiencesAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.WorkExperiences.AsNoTracking()
            .Where(record => record.EmployeeId == employeeId)
            .OrderByDescending(record => record.FromYear).ThenByDescending(record => record.FromMonth)
            .ToListAsync(token);
        return records.Select(record => new WorkExperience(
            record.WorkExperienceId, record.FromYear, record.FromMonth, record.ToYear, record.ToMonth,
            U(record.CompanyName), U(record.Position), record.EmploymentTypeId,
            record.IsRelevantExperience == 1, U(record.Remark))).ToArray();
    }

    private async Task<IReadOnlyList<EmployeeDocument>> LoadDocumentsAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.EmployeeDocuments.AsNoTracking()
            .Where(record => record.EmployeeId == employeeId).ToListAsync(token);
        var types = await LoadDictionaryAsync(dbContext.DocumentTypes,
            records.Select(record => record.DocumentTypeId), record => record.DocumentTypeId, token);
        return records.Select(record =>
        {
            types.TryGetValue(record.DocumentTypeId ?? 0, out var type);
            return new EmployeeDocument(record.DocumentId, record.DocumentTypeId, U(type?.Code), U(type?.Description),
                U(record.OriginalFileName), U(record.Description), record.IsCompressed == 1,
                record.IsProfilePhoto == 1);
        }).ToArray();
    }

    private async Task<Dictionary<int, TRecord>> LoadDictionaryAsync<TRecord>(
        DbSet<TRecord> source,
        IEnumerable<int?> ids,
        Func<TRecord, int> keySelector,
        CancellationToken cancellationToken)
        where TRecord : class
    {
        var keys = ids.Where(id => id is not null).Select(id => id!.Value).Distinct().ToArray();
        if (keys.Length == 0)
        {
            return [];
        }

        var records = await source.AsNoTracking().ToListAsync(cancellationToken);
        return records.Where(record => keys.Contains(keySelector(record))).ToDictionary(keySelector);
    }

    private string BuildDisplayName(EmployeeRecord record)
    {
        var englishName = string.Join(' ', new[] { U(record.EnglishSurname), U(record.EnglishOtherName) }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        return !string.IsNullOrWhiteSpace(englishName)
            ? englishName
            : U(record.ChineseName) ?? U(record.Alias) ?? U(record.EmployeeNumber) ?? record.EmployeeId.ToString();
    }

    private string? U(string? value) => protector.Unprotect(value);

    private static DateOnly? D(DateTime? value) => value is null ? null : DateOnly.FromDateTime(value.Value);
}
