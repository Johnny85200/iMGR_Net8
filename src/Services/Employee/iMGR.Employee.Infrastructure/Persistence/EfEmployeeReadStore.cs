using IMGR.Employee.Application.Abstractions;
using IMGR.Employee.Application.Employees;
using IMGR.Employee.Domain.Employees;
using IMGR.Employee.Domain.Employment;
using IMGR.Employee.Domain.Profiles;
using IMGR.Employee.Infrastructure.Tenancy;
using IMGR.Database.Tenant;
using IMGR.Database.Tenant.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMGR.Employee.Infrastructure.Persistence;

internal sealed class EfEmployeeReadStore(
    LegacyTenantDbContext dbContext,
    LegacyFieldProtector protector) : IEmployeeReadStore
{
    public async Task<EmployeePage> SearchAsync(
        EmployeeSearchQuery query,
        CancellationToken cancellationToken)
    {
        var employees = dbContext.EmpPersonalInfos.AsNoTracking();
        var legacyStatus = EmployeeStatusCodes.ToLegacyCode(query.Status);
        if (legacyStatus is not null)
        {
            employees = employees.Where(record => record.EmpStatus == legacyStatus);
        }

        if (query.Search is not null)
        {
            var plaintext = query.Search;
            var encrypted = protector.Protect(plaintext);
            employees = employees.Where(record =>
                record.EmpNo == plaintext || record.EmpNo == encrypted
                || record.EmpEngSurname == plaintext || record.EmpEngSurname == encrypted
                || record.EmpEngOtherName == plaintext || record.EmpEngOtherName == encrypted
                || record.EmpChiFullName == plaintext || record.EmpChiFullName == encrypted
                || record.EmpAlias == plaintext || record.EmpAlias == encrypted);
        }

        var totalCount = await employees.CountAsync(cancellationToken);
        var pageRecords = await employees
            .OrderBy(record => record.EmpID)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
        var employeeIds = pageRecords.Select(record => record.EmpID).ToArray();
        var assignments = await LoadAssignmentsAsync(employeeIds, cancellationToken);

        var items = pageRecords.Select(record =>
        {
            var employeeAssignments = assignments.GetValueOrDefault(record.EmpID) ?? [];
            return new EmployeeSummary(
                record.EmpID,
                U(record.EmpNo) ?? string.Empty,
                BuildDisplayName(record),
                EmployeeStatusCodes.Parse(record.EmpStatus),
                D(record.EmpDateOfJoin),
                EmploymentAssignmentSelector.SelectAsOf(employeeAssignments, query.AsOfDate));
        }).ToArray();

        return new EmployeePage(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<EmployeeProfile?> GetAsync(
        int employeeId,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        var employee = await dbContext.EmpPersonalInfos.AsNoTracking()
            .SingleOrDefaultAsync(record => record.EmpID == employeeId, cancellationToken);
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
            employee.EmpID,
            U(employee.EmpNo) ?? string.Empty,
            EmployeeStatusCodes.Parse(employee.EmpStatus),
            U(employee.EmpEngSurname) ?? string.Empty,
            U(employee.EmpEngOtherName) ?? string.Empty,
            U(employee.EmpChiFullName) ?? string.Empty,
            U(employee.EmpAlias) ?? string.Empty,
            SensitiveValueMasker.Mask(U(employee.EmpHKID)),
            U(employee.EmpGender),
            U(employee.EmpMaritalStatus),
            D(employee.EmpDateOfBirth),
            U(employee.EmpNationality),
            SensitiveValueMasker.Mask(U(employee.EmpPassportNo)),
            D(employee.EmpPassportExpiryDate),
            U(employee.EmpResAddr),
            U(employee.EmpCorAddr),
            D(employee.EmpDateOfJoin),
            D(employee.EmpServiceDate),
            D(employee.EmpProbaLastDate),
            U(employee.EmpEmail),
            U(employee.EmpInternalEmail),
            U(employee.EmpMobileNo),
            U(employee.EmpHomePhoneNo),
            U(employee.EmpOfficePhoneNo),
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
            query = query.Where(record => record.CompanyID == companyId);
        }

        if (levelId is not null)
        {
            query = query.Where(record => record.HLevelID == levelId);
        }

        var elements = await query.OrderBy(record => record.HElementID).ToListAsync(cancellationToken);
        var companies = await dbContext.Companies.AsNoTracking()
            .Where(record => elements.Select(element => element.CompanyID).Contains(record.CompanyID))
            .ToDictionaryAsync(record => record.CompanyID, cancellationToken);
        var levels = await dbContext.HierarchyLevels.AsNoTracking()
            .Where(record => elements.Select(element => element.HLevelID).Contains(record.HLevelID))
            .ToDictionaryAsync(record => record.HLevelID, cancellationToken);

        return elements.Select(element =>
        {
            companies.TryGetValue(element.CompanyID ?? 0, out var company);
            levels.TryGetValue(element.HLevelID ?? 0, out var level);
            return new OrganizationUnit(
                element.HElementID,
                U(element.HElementCode),
                U(element.HElementDesc),
                element.CompanyID,
                U(company?.CompanyCode),
                U(company?.CompanyName),
                element.HLevelID,
                U(level?.HLevelCode),
                U(level?.HLevelDesc),
                level?.HLevelSeqNo);
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

        var positions = await dbContext.EmpPositionInfos.AsNoTracking()
            .Where(record => record.EmpID != null && employeeIds.Contains(record.EmpID.Value))
            .OrderByDescending(record => record.EmpPosEffFr)
            .ThenByDescending(record => record.EmpPosID)
            .ToListAsync(cancellationToken);
        var companies = await LoadDictionaryAsync(
            dbContext.Companies,
            positions.Select(record => record.CompanyID),
            record => record.CompanyID,
            cancellationToken);
        var positionLookups = await LoadDictionaryAsync(
            dbContext.Positions,
            positions.Select(record => record.PositionID),
            record => record.PositionID,
            cancellationToken);
        var ranks = await LoadDictionaryAsync(
            dbContext.Ranks,
            positions.Select(record => record.RankID),
            record => record.RankID,
            cancellationToken);
        var employmentTypes = await LoadDictionaryAsync(
            dbContext.EmploymentTypes,
            positions.Select(record => record.EmploymentTypeID),
            record => record.EmploymentTypeID,
            cancellationToken);

        var assignmentIds = positions.Select(record => record.EmpPosID).ToArray();
        var hierarchies = await dbContext.EmpHierarchies.AsNoTracking()
            .Where(record => record.EmpPosID != null && assignmentIds.Contains(record.EmpPosID.Value))
            .ToListAsync(cancellationToken);
        var elements = await LoadDictionaryAsync(
            dbContext.HierarchyElements,
            hierarchies.Select(record => record.HElementID),
            record => record.HElementID,
            cancellationToken);
        var levels = await LoadDictionaryAsync(
            dbContext.HierarchyLevels,
            hierarchies.Select(record => record.HLevelID),
            record => record.HLevelID,
            cancellationToken);
        var organizationsByAssignment = hierarchies
            .Where(record => record.EmpPosID is not null)
            .GroupBy(record => record.EmpPosID!.Value)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<OrganizationAssignment>)group.Select(record =>
                {
                    elements.TryGetValue(record.HElementID ?? 0, out var element);
                    levels.TryGetValue(record.HLevelID ?? element?.HLevelID ?? 0, out var level);
                    return new OrganizationAssignment(
                        record.HElementID ?? 0,
                        U(element?.HElementCode),
                        U(element?.HElementDesc),
                        record.HLevelID ?? element?.HLevelID,
                        U(level?.HLevelCode),
                        U(level?.HLevelDesc),
                        level?.HLevelSeqNo);
                }).OrderBy(item => item.LevelSequence).ToArray());

        return positions
            .Where(record => record.EmpID is not null)
            .GroupBy(record => record.EmpID!.Value)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<EmploymentAssignment>)group.Select(record =>
                {
                    companies.TryGetValue(record.CompanyID ?? 0, out var company);
                    positionLookups.TryGetValue(record.PositionID ?? 0, out var position);
                    ranks.TryGetValue(record.RankID ?? 0, out var rank);
                    employmentTypes.TryGetValue(record.EmploymentTypeID ?? 0, out var employmentType);
                    return new EmploymentAssignment(
                        record.EmpPosID,
                        D(record.EmpPosEffFr),
                        D(record.EmpPosEffTo),
                        record.CompanyID,
                        U(company?.CompanyCode),
                        U(company?.CompanyName),
                        record.PositionID,
                        U(position?.PositionCode),
                        U(position?.PositionDesc),
                        record.RankID,
                        U(rank?.RankCode),
                        U(rank?.RankDesc),
                        record.EmploymentTypeID,
                        U(employmentType?.EmploymentTypeCode),
                        U(employmentType?.EmploymentTypeDesc),
                        organizationsByAssignment.GetValueOrDefault(record.EmpPosID) ?? []);
                }).ToArray());
    }

    private async Task<IReadOnlyList<EmploymentContract>> LoadContractsAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        var records = await dbContext.EmpContractTerms.AsNoTracking()
            .Where(record => record.EmpID == employeeId)
            .OrderByDescending(record => record.EmpContractEmployedFrom)
            .ToListAsync(cancellationToken);
        return records.Select(record => new EmploymentContract(
            record.EmpContractID,
            U(record.EmpContractCompanyName),
            U(record.EmpContractCompanyContactNo),
            U(record.EmpContractCompanyAddr),
            D(record.EmpContractEmployedFrom),
            D(record.EmpContractEmployedTo),
            record.EmpContractGratuity,
            record.CurrencyID,
            record.EmpContractGratuityMethod)).ToArray();
    }

    private async Task<IReadOnlyList<EmployeeBankAccount>> LoadBankAccountsAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        var records = await dbContext.EmpBankAccounts.AsNoTracking()
            .Where(record => record.EmpID == employeeId)
            .OrderByDescending(record => record.EmpAccDefault)
            .ThenBy(record => record.EmpBankAccountID)
            .ToListAsync(cancellationToken);
        var bankCodes = records.Select(record => U(record.EmpBankCode)).Where(code => code is not null).ToArray();
        var banks = await dbContext.BankLists.AsNoTracking()
            .Where(record => bankCodes.Contains(record.BankCode))
            .ToDictionaryAsync(record => record.BankCode, cancellationToken);
        return records.Select(record =>
        {
            var bankCode = U(record.EmpBankCode);
            banks.TryGetValue(bankCode ?? string.Empty, out var bank);
            return new EmployeeBankAccount(
                record.EmpBankAccountID,
                bankCode,
                bank?.BankName,
                U(record.EmpBranchCode),
                SensitiveValueMasker.Mask(U(record.EmpAccountNo)),
                U(record.EmpBankAccountHolderName),
                record.EmpAccDefault is true,
                U(record.EmpBankAccountRemark));
        }).ToArray();
    }

    private async Task<IReadOnlyList<EmployeeSpouse>> LoadSpousesAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.EmpSpouses.AsNoTracking()
            .Where(record => record.EmpID == employeeId).ToListAsync(token);
        return records.Select(record => new EmployeeSpouse(
            record.EmpSpouseID, U(record.EmpSpouseSurname) ?? string.Empty, U(record.EmpSpouseOtherName) ?? string.Empty,
            U(record.EmpSpouseChineseName) ?? string.Empty, SensitiveValueMasker.Mask(U(record.EmpSpouseHKID)),
            SensitiveValueMasker.Mask(U(record.EmpSpousePassportNo)), D(record.EmpSpouseDateOfBirth))).ToArray();
    }

    private async Task<IReadOnlyList<EmployeeDependant>> LoadDependantsAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.EmpDependants.AsNoTracking()
            .Where(record => record.EmpID == employeeId).ToListAsync(token);
        return records.Select(record => new EmployeeDependant(
            record.EmpDependantID, U(record.EmpDependantSurname) ?? string.Empty, U(record.EmpDependantOtherName) ?? string.Empty,
            U(record.EmpDependantChineseName) ?? string.Empty, U(record.EmpDependantGender), U(record.EmpDependantRelationship),
            SensitiveValueMasker.Mask(U(record.EmpDependantHKID)), SensitiveValueMasker.Mask(U(record.EmpDependantPassportNo)),
            D(record.EmpDependantDateOfBirth))).ToArray();
    }

    private async Task<IReadOnlyList<EmergencyContact>> LoadEmergencyContactsAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.EmpEmergencyContacts.AsNoTracking()
            .Where(record => record.EmpID == employeeId).ToListAsync(token);
        return records.Select(record => new EmergencyContact(
            record.EmpEmergencyContactID, U(record.EmpEmergencyContactName) ?? string.Empty,
            U(record.EmpEmergencyContactGender), U(record.EmpEmergencyContactRelationship),
            U(record.EmpEmergencyContactContactNo), null)).ToArray();
    }

    private async Task<IReadOnlyList<EmployeeSkill>> LoadSkillsAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.EmpSkills.AsNoTracking()
            .Where(record => record.EmpID == employeeId).ToListAsync(token);
        var skills = await LoadDictionaryAsync(dbContext.Skills, records.Select(record => record.SkillID),
            record => record.SkillID, token);
        var levels = await LoadDictionaryAsync(dbContext.SkillLevels, records.Select(record => record.SkillLevelID),
            record => record.SkillLevelID, token);
        return records.Select(record =>
        {
            skills.TryGetValue(record.SkillID ?? 0, out var skill);
            levels.TryGetValue(record.SkillLevelID ?? 0, out var level);
            return new EmployeeSkill(record.EmpSkillID, record.SkillID, U(skill?.SkillCode), U(skill?.SkillDesc),
                record.SkillLevelID, U(level?.SkillLevelCode), U(level?.SkillLevelDesc));
        }).ToArray();
    }

    private async Task<IReadOnlyList<EmployeeQualification>> LoadQualificationsAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.EmpQualifications.AsNoTracking()
            .Where(record => record.EmpID == employeeId).ToListAsync(token);
        var lookups = await LoadDictionaryAsync(dbContext.Qualifications,
            records.Select(record => record.QualificationID), record => record.QualificationID, token);
        return records.Select(record =>
        {
            lookups.TryGetValue(record.QualificationID ?? 0, out var lookup);
            return new EmployeeQualification(record.EmpQualificationID, record.QualificationID,
                U(lookup?.QualificationCode), U(lookup?.QualificationDesc), D(record.EmpQualificationFrom),
                D(record.EmpQualificationTo), U(record.EmpQualificationInstitution),
                U(record.EmpQualificationLearningMethod), U(record.EmpQualificationRemark));
        }).ToArray();
    }

    private async Task<IReadOnlyList<WorkExperience>> LoadWorkExperiencesAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.EmpWorkExps.AsNoTracking()
            .Where(record => record.EmpID == employeeId)
            .OrderByDescending(record => record.EmpWorkExpFromYear).ThenByDescending(record => record.EmpWorkExpFromMonth)
            .ToListAsync(token);
        return records.Select(record => new WorkExperience(
            record.EmpWorkExpID, record.EmpWorkExpFromYear, record.EmpWorkExpFromMonth,
            record.EmpWorkExpToYear, record.EmpWorkExpToMonth, U(record.EmpWorkExpCompanyName),
            U(record.EmpWorkExpPosition), record.EmpWorkExpEmploymentTypeID,
            record.EmpWorkExpIsRelevantExperience is true, U(record.EmpWorkExpRemark))).ToArray();
    }

    private async Task<IReadOnlyList<EmployeeDocument>> LoadDocumentsAsync(int employeeId, CancellationToken token)
    {
        var records = await dbContext.EmpDocuments.AsNoTracking()
            .Where(record => record.EmpID == employeeId).ToListAsync(token);
        var types = await LoadDictionaryAsync(dbContext.DocumentTypes,
            records.Select(record => record.DocumentTypeID), record => record.DocumentTypeID, token);
        return records.Select(record =>
        {
            types.TryGetValue(record.DocumentTypeID ?? 0, out var type);
            return new EmployeeDocument(record.EmpDocumentID, record.DocumentTypeID,
                U(type?.DocumentTypeCode), U(type?.DocumentTypeDesc), U(record.EmpDocumentOriginalFileName),
                U(record.EmpDocumentDesc), record.EmpDocumentIsCompressed is true,
                record.EmpDocumentIsProfilePhoto is true);
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

    private string BuildDisplayName(EmpPersonalInfo record)
    {
        var englishName = string.Join(' ', new[] { U(record.EmpEngSurname), U(record.EmpEngOtherName) }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        return !string.IsNullOrWhiteSpace(englishName)
            ? englishName
            : U(record.EmpChiFullName) ?? U(record.EmpAlias) ?? U(record.EmpNo) ?? record.EmpID.ToString();
    }

    private string? U(string? value) => protector.Unprotect(value);

    private static DateOnly? D(DateTime? value) => value is null ? null : DateOnly.FromDateTime(value.Value);
}
