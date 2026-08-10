using Microsoft.EntityFrameworkCore;

namespace IMGR.Employee.Infrastructure.Persistence;

internal sealed class EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : DbContext(options)
{
    public DbSet<EmployeeRecord> Employees => Set<EmployeeRecord>();
    public DbSet<PositionRecord> Positions => Set<PositionRecord>();
    public DbSet<EmployeeHierarchyRecord> EmployeeHierarchies => Set<EmployeeHierarchyRecord>();
    public DbSet<CompanyRecord> Companies => Set<CompanyRecord>();
    public DbSet<PositionLookupRecord> PositionLookups => Set<PositionLookupRecord>();
    public DbSet<RankRecord> Ranks => Set<RankRecord>();
    public DbSet<EmploymentTypeRecord> EmploymentTypes => Set<EmploymentTypeRecord>();
    public DbSet<HierarchyLevelRecord> HierarchyLevels => Set<HierarchyLevelRecord>();
    public DbSet<HierarchyElementRecord> HierarchyElements => Set<HierarchyElementRecord>();
    public DbSet<ContractRecord> Contracts => Set<ContractRecord>();
    public DbSet<BankAccountRecord> BankAccounts => Set<BankAccountRecord>();
    public DbSet<BankRecord> Banks => Set<BankRecord>();
    public DbSet<SpouseRecord> Spouses => Set<SpouseRecord>();
    public DbSet<DependantRecord> Dependants => Set<DependantRecord>();
    public DbSet<EmergencyContactRecord> EmergencyContacts => Set<EmergencyContactRecord>();
    public DbSet<EmployeeSkillRecord> EmployeeSkills => Set<EmployeeSkillRecord>();
    public DbSet<SkillRecord> Skills => Set<SkillRecord>();
    public DbSet<SkillLevelRecord> SkillLevels => Set<SkillLevelRecord>();
    public DbSet<EmployeeQualificationRecord> EmployeeQualifications => Set<EmployeeQualificationRecord>();
    public DbSet<QualificationRecord> Qualifications => Set<QualificationRecord>();
    public DbSet<WorkExperienceRecord> WorkExperiences => Set<WorkExperienceRecord>();
    public DbSet<EmployeeDocumentRecord> EmployeeDocuments => Set<EmployeeDocumentRecord>();
    public DbSet<DocumentTypeRecord> DocumentTypes => Set<DocumentTypeRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureEmployee(modelBuilder);
        ConfigureEmployment(modelBuilder);
        ConfigureOrganization(modelBuilder);
        ConfigureProfileSections(modelBuilder);
    }

    private static void ConfigureEmployee(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<EmployeeRecord>();
        entity.ToTable("EmpPersonalInfo", "dbo");
        entity.HasKey(record => record.EmployeeId);
        entity.Property(record => record.EmployeeId).HasColumnName("EmpID");
        entity.Property(record => record.EmployeeNumber).HasColumnName("EmpNo");
        entity.Property(record => record.Status).HasColumnName("EmpStatus");
        entity.Property(record => record.EnglishSurname).HasColumnName("EmpEngSurname");
        entity.Property(record => record.EnglishOtherName).HasColumnName("EmpEngOtherName");
        entity.Property(record => record.ChineseName).HasColumnName("EmpChiFullName");
        entity.Property(record => record.Alias).HasColumnName("EmpAlias");
        entity.Property(record => record.IdentityNumber).HasColumnName("EmpHKID");
        entity.Property(record => record.Gender).HasColumnName("EmpGender");
        entity.Property(record => record.MaritalStatus).HasColumnName("EmpMaritalStatus");
        entity.Property(record => record.DateOfBirth).HasColumnName("EmpDateOfBirth");
        entity.Property(record => record.Nationality).HasColumnName("EmpNationality");
        entity.Property(record => record.PassportNumber).HasColumnName("EmpPassportNo");
        entity.Property(record => record.PassportExpiryDate).HasColumnName("EmpPassportExpiryDate");
        entity.Property(record => record.ResidentialAddress).HasColumnName("EmpResAddr");
        entity.Property(record => record.CorrespondenceAddress).HasColumnName("EmpCorAddr");
        entity.Property(record => record.DateOfJoin).HasColumnName("EmpDateOfJoin");
        entity.Property(record => record.ServiceDate).HasColumnName("EmpServiceDate");
        entity.Property(record => record.ProbationLastDate).HasColumnName("EmpProbaLastDate");
        entity.Property(record => record.Email).HasColumnName("EmpEmail");
        entity.Property(record => record.InternalEmail).HasColumnName("EmpInternalEmail");
        entity.Property(record => record.MobileNumber).HasColumnName("EmpMobileNo");
        entity.Property(record => record.HomePhoneNumber).HasColumnName("EmpHomePhoneNo");
        entity.Property(record => record.OfficePhoneNumber).HasColumnName("EmpOfficePhoneNo");
        entity.Property(record => record.Remark).HasColumnName("Remark");
    }

    private static void ConfigureEmployment(ModelBuilder modelBuilder)
    {
        var position = modelBuilder.Entity<PositionRecord>();
        position.ToTable("EmpPositionInfo", "dbo");
        position.HasKey(record => record.AssignmentId);
        position.Property(record => record.AssignmentId).HasColumnName("EmpPosID");
        position.Property(record => record.EmployeeId).HasColumnName("EmpID");
        position.Property(record => record.EffectiveFrom).HasColumnName("EmpPosEffFr");
        position.Property(record => record.EffectiveTo).HasColumnName("EmpPosEffTo");
        position.Property(record => record.CompanyId).HasColumnName("CompanyID");
        position.Property(record => record.PositionId).HasColumnName("PositionID");
        position.Property(record => record.RankId).HasColumnName("RankID");
        position.Property(record => record.EmploymentTypeId).HasColumnName("EmploymentTypeID");

        var contract = modelBuilder.Entity<ContractRecord>();
        contract.ToTable("EmpContractTerms", "dbo");
        contract.HasKey(record => record.ContractId);
        contract.Property(record => record.ContractId).HasColumnName("EmpContractID");
        contract.Property(record => record.EmployeeId).HasColumnName("EmpID");
        contract.Property(record => record.CompanyName).HasColumnName("EmpContractCompanyName");
        contract.Property(record => record.CompanyContactNumber).HasColumnName("EmpContractCompanyContactNo");
        contract.Property(record => record.CompanyAddress).HasColumnName("EmpContractCompanyAddr");
        contract.Property(record => record.EmployedFrom).HasColumnName("EmpContractEmployedFrom");
        contract.Property(record => record.EmployedTo).HasColumnName("EmpContractEmployedTo");
        contract.Property(record => record.Gratuity).HasColumnName("EmpContractGratuity");
        contract.Property(record => record.CurrencyCode).HasColumnName("CurrencyID");
        contract.Property(record => record.GratuityMethod).HasColumnName("EmpContractGratuityMethod");
    }

    private static void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        var hierarchy = modelBuilder.Entity<EmployeeHierarchyRecord>();
        hierarchy.ToTable("EmpHierarchy", "dbo");
        hierarchy.HasKey(record => record.EmployeeHierarchyId);
        hierarchy.Property(record => record.EmployeeHierarchyId).HasColumnName("EmpHierarchyID");
        hierarchy.Property(record => record.EmployeeId).HasColumnName("EmpID");
        hierarchy.Property(record => record.AssignmentId).HasColumnName("EmpPosID");
        hierarchy.Property(record => record.ElementId).HasColumnName("HElementID");
        hierarchy.Property(record => record.LevelId).HasColumnName("HLevelID");

        MapCodeDescription<CompanyRecord>(modelBuilder, "Company", record => record.CompanyId,
            "CompanyID", record => record.Code, "CompanyCode", record => record.Name, "CompanyName");
        MapCodeDescription<PositionLookupRecord>(modelBuilder, "Position", record => record.PositionId,
            "PositionID", record => record.Code, "PositionCode", record => record.Description, "PositionDesc");
        MapCodeDescription<RankRecord>(modelBuilder, "Rank", record => record.RankId,
            "RankID", record => record.Code, "RankCode", record => record.Description, "RankDesc");
        MapCodeDescription<EmploymentTypeRecord>(modelBuilder, "EmploymentType", record => record.EmploymentTypeId,
            "EmploymentTypeID", record => record.Code, "EmploymentTypeCode", record => record.Description, "EmploymentTypeDesc");

        var level = modelBuilder.Entity<HierarchyLevelRecord>();
        level.ToTable("HierarchyLevel", "dbo");
        level.HasKey(record => record.LevelId);
        level.Property(record => record.LevelId).HasColumnName("HLevelID");
        level.Property(record => record.Code).HasColumnName("HLevelCode");
        level.Property(record => record.Description).HasColumnName("HLevelDesc");
        level.Property(record => record.Sequence).HasColumnName("HLevelSeqNo");

        var element = modelBuilder.Entity<HierarchyElementRecord>();
        element.ToTable("HierarchyElement", "dbo");
        element.HasKey(record => record.ElementId);
        element.Property(record => record.ElementId).HasColumnName("HElementID");
        element.Property(record => record.CompanyId).HasColumnName("CompanyID");
        element.Property(record => record.Code).HasColumnName("HElementCode");
        element.Property(record => record.Description).HasColumnName("HElementDesc");
        element.Property(record => record.LevelId).HasColumnName("HLevelID");
    }

    private static void ConfigureProfileSections(ModelBuilder modelBuilder)
    {
        var bankAccount = modelBuilder.Entity<BankAccountRecord>();
        bankAccount.ToTable("EmpBankAccount", "dbo");
        bankAccount.HasKey(record => record.BankAccountId);
        bankAccount.Property(record => record.BankAccountId).HasColumnName("EmpBankAccountID");
        bankAccount.Property(record => record.EmployeeId).HasColumnName("EmpID");
        bankAccount.Property(record => record.BankCode).HasColumnName("EmpBankCode");
        bankAccount.Property(record => record.BranchCode).HasColumnName("EmpBranchCode");
        bankAccount.Property(record => record.AccountNumber).HasColumnName("EmpAccountNo");
        bankAccount.Property(record => record.AccountHolderName).HasColumnName("EmpBankAccountHolderName");
        bankAccount.Property(record => record.IsDefault).HasColumnName("EmpAccDefault");
        bankAccount.Property(record => record.Remark).HasColumnName("EmpBankAccountRemark");

        var bank = modelBuilder.Entity<BankRecord>();
        bank.ToTable("BankList", "dbo");
        bank.HasKey(record => record.BankCode);
        bank.Property(record => record.BankCode).HasColumnName("BankCode");
        bank.Property(record => record.Name).HasColumnName("BankName");

        ConfigureFamily(modelBuilder);
        ConfigureCapability(modelBuilder);
        ConfigureDocuments(modelBuilder);
    }

    private static void ConfigureFamily(ModelBuilder modelBuilder)
    {
        var spouse = modelBuilder.Entity<SpouseRecord>();
        spouse.ToTable("EmpSpouse", "dbo");
        spouse.HasKey(record => record.SpouseId);
        spouse.Property(record => record.SpouseId).HasColumnName("EmpSpouseID");
        spouse.Property(record => record.EmployeeId).HasColumnName("EmpID");
        spouse.Property(record => record.Surname).HasColumnName("EmpSpouseSurname");
        spouse.Property(record => record.OtherName).HasColumnName("EmpSpouseOtherName");
        spouse.Property(record => record.ChineseName).HasColumnName("EmpSpouseChineseName");
        spouse.Property(record => record.IdentityNumber).HasColumnName("EmpSpouseHKID");
        spouse.Property(record => record.PassportNumber).HasColumnName("EmpSpousePassportNo");
        spouse.Property(record => record.DateOfBirth).HasColumnName("EmpSpouseDateOfBirth");

        var dependant = modelBuilder.Entity<DependantRecord>();
        dependant.ToTable("EmpDependant", "dbo");
        dependant.HasKey(record => record.DependantId);
        dependant.Property(record => record.DependantId).HasColumnName("EmpDependantID");
        dependant.Property(record => record.EmployeeId).HasColumnName("EmpID");
        dependant.Property(record => record.Surname).HasColumnName("EmpDependantSurname");
        dependant.Property(record => record.OtherName).HasColumnName("EmpDependantOtherName");
        dependant.Property(record => record.ChineseName).HasColumnName("EmpDependantChineseName");
        dependant.Property(record => record.Gender).HasColumnName("EmpDependantGender");
        dependant.Property(record => record.IdentityNumber).HasColumnName("EmpDependantHKID");
        dependant.Property(record => record.PassportNumber).HasColumnName("EmpDependantPassportNo");
        dependant.Property(record => record.Relationship).HasColumnName("EmpDependantRelationship");
        dependant.Property(record => record.DateOfBirth).HasColumnName("EmpDependantDateOfBirth");

        var emergency = modelBuilder.Entity<EmergencyContactRecord>();
        emergency.ToTable("EmpEmergencyContact", "dbo");
        emergency.HasKey(record => record.EmergencyContactId);
        emergency.Property(record => record.EmergencyContactId).HasColumnName("EmpEmergencyContactID");
        emergency.Property(record => record.EmployeeId).HasColumnName("EmpID");
        emergency.Property(record => record.Name).HasColumnName("EmpEmergencyContactName");
        emergency.Property(record => record.Gender).HasColumnName("EmpEmergencyContactGender");
        emergency.Property(record => record.Relationship).HasColumnName("EmpEmergencyContactRelationship");
        emergency.Property(record => record.DaytimeContactNumber).HasColumnName("EmpEmergencyContactContactNoDay");
        emergency.Property(record => record.NightContactNumber).HasColumnName("EmpEmergencyContactContactNoNight");
    }

    private static void ConfigureCapability(ModelBuilder modelBuilder)
    {
        var employeeSkill = modelBuilder.Entity<EmployeeSkillRecord>();
        employeeSkill.ToTable("EmpSkill", "dbo");
        employeeSkill.HasKey(record => record.EmployeeSkillId);
        employeeSkill.Property(record => record.EmployeeSkillId).HasColumnName("EmpSkillID");
        employeeSkill.Property(record => record.EmployeeId).HasColumnName("EmpID");
        employeeSkill.Property(record => record.SkillId).HasColumnName("SkillID");
        employeeSkill.Property(record => record.SkillLevelId).HasColumnName("SkillLevelID");
        MapCodeDescription<SkillRecord>(modelBuilder, "Skill", record => record.SkillId,
            "SkillID", record => record.Code, "SkillCode", record => record.Description, "SkillDesc");
        MapCodeDescription<SkillLevelRecord>(modelBuilder, "SkillLevel", record => record.SkillLevelId,
            "SkillLevelID", record => record.Code, "SkillLevelCode", record => record.Description, "SkillLevelDesc");

        var employeeQualification = modelBuilder.Entity<EmployeeQualificationRecord>();
        employeeQualification.ToTable("EmpQualification", "dbo");
        employeeQualification.HasKey(record => record.EmployeeQualificationId);
        employeeQualification.Property(record => record.EmployeeQualificationId).HasColumnName("EmpQualificationID");
        employeeQualification.Property(record => record.EmployeeId).HasColumnName("EmpID");
        employeeQualification.Property(record => record.QualificationId).HasColumnName("QualificationID");
        employeeQualification.Property(record => record.From).HasColumnName("EmpQualificationFrom");
        employeeQualification.Property(record => record.To).HasColumnName("EmpQualificationTo");
        employeeQualification.Property(record => record.Institution).HasColumnName("EmpQualificationInstitution");
        employeeQualification.Property(record => record.Remark).HasColumnName("EmpQualificationRemark");
        employeeQualification.Property(record => record.LearningMethod).HasColumnName("EmpQualificationLearningMethod");
        MapCodeDescription<QualificationRecord>(modelBuilder, "Qualification", record => record.QualificationId,
            "QualificationID", record => record.Code, "QualificationCode", record => record.Description, "QualificationDesc");

        var work = modelBuilder.Entity<WorkExperienceRecord>();
        work.ToTable("EmpWorkExp", "dbo");
        work.HasKey(record => record.WorkExperienceId);
        work.Property(record => record.WorkExperienceId).HasColumnName("EmpWorkExpID");
        work.Property(record => record.EmployeeId).HasColumnName("EmpID");
        work.Property(record => record.FromYear).HasColumnName("EmpWorkExpFromYear");
        work.Property(record => record.FromMonth).HasColumnName("EmpWorkExpFromMonth");
        work.Property(record => record.ToYear).HasColumnName("EmpWorkExpToYear");
        work.Property(record => record.ToMonth).HasColumnName("EmpWorkExpToMonth");
        work.Property(record => record.CompanyName).HasColumnName("EmpWorkExpCompanyName");
        work.Property(record => record.Position).HasColumnName("EmpWorkExpPosition");
        work.Property(record => record.Remark).HasColumnName("EmpWorkExpRemark");
        work.Property(record => record.EmploymentTypeId).HasColumnName("EmpWorkExpEmploymentTypeID");
        work.Property(record => record.IsRelevantExperience).HasColumnName("EmpWorkExpIsRelevantExperience");
    }

    private static void ConfigureDocuments(ModelBuilder modelBuilder)
    {
        var document = modelBuilder.Entity<EmployeeDocumentRecord>();
        document.ToTable("EmpDocument", "dbo");
        document.HasKey(record => record.DocumentId);
        document.Property(record => record.DocumentId).HasColumnName("EmpDocumentID");
        document.Property(record => record.EmployeeId).HasColumnName("EmpID");
        document.Property(record => record.DocumentTypeId).HasColumnName("DocumentTypeID");
        document.Property(record => record.OriginalFileName).HasColumnName("EmpDocumentOriginalFileName");
        document.Property(record => record.Description).HasColumnName("EmpDocumentDesc");
        document.Property(record => record.IsCompressed).HasColumnName("EmpDocumentIsCompressed");
        document.Property(record => record.IsProfilePhoto).HasColumnName("EmpDocumentIsProfilePhoto");
        MapCodeDescription<DocumentTypeRecord>(modelBuilder, "DocumentType", record => record.DocumentTypeId,
            "DocumentTypeID", record => record.Code, "DocumentTypeCode", record => record.Description, "DocumentTypeDesc");
    }

    private static void MapCodeDescription<TEntity>(
        ModelBuilder modelBuilder,
        string table,
        System.Linq.Expressions.Expression<Func<TEntity, object?>> key,
        string keyColumn,
        System.Linq.Expressions.Expression<Func<TEntity, string?>> code,
        string codeColumn,
        System.Linq.Expressions.Expression<Func<TEntity, string?>> description,
        string descriptionColumn)
        where TEntity : class
    {
        var entity = modelBuilder.Entity<TEntity>();
        entity.ToTable(table, "dbo");
        entity.HasKey(key);
        entity.Property(key).HasColumnName(keyColumn);
        entity.Property(code).HasColumnName(codeColumn);
        entity.Property(description).HasColumnName(descriptionColumn);
    }
}
