using IMGR.LegacyData.Tenant.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMGR.LegacyData.Tenant;

public partial class LegacyTenantDbContext : DbContext
{
    public LegacyTenantDbContext()
    {
    }

    public LegacyTenantDbContext(DbContextOptions<LegacyTenantDbContext> options)
        : base(options)
    {
    }

    public DbSet<EmpPersonalInfo> Employees => Set<EmpPersonalInfo>();
    public DbSet<EmpPositionInfo> Positions => Set<EmpPositionInfo>();
    public DbSet<EmpHierarchy> EmployeeHierarchies => Set<EmpHierarchy>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Position> PositionLookups => Set<Position>();
    public DbSet<Rank> Ranks => Set<Rank>();
    public DbSet<EmploymentType> EmploymentTypes => Set<EmploymentType>();
    public DbSet<HierarchyLevel> HierarchyLevels => Set<HierarchyLevel>();
    public DbSet<HierarchyElement> HierarchyElements => Set<HierarchyElement>();
    public DbSet<EmpContractTerms> Contracts => Set<EmpContractTerms>();
    public DbSet<EmpBankAccount> BankAccounts => Set<EmpBankAccount>();
    public DbSet<BankList> Banks => Set<BankList>();
    public DbSet<EmpSpouse> Spouses => Set<EmpSpouse>();
    public DbSet<EmpDependant> Dependants => Set<EmpDependant>();
    public DbSet<EmpEmergencyContact> EmergencyContacts => Set<EmpEmergencyContact>();
    public DbSet<EmpSkill> EmployeeSkills => Set<EmpSkill>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<SkillLevel> SkillLevels => Set<SkillLevel>();
    public DbSet<EmpQualification> EmployeeQualifications => Set<EmpQualification>();
    public DbSet<Qualification> Qualifications => Set<Qualification>();
    public DbSet<EmpWorkExp> WorkExperiences => Set<EmpWorkExp>();
    public DbSet<EmpDocument> EmployeeDocuments => Set<EmpDocument>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<Users> Users => Set<Users>();
    public DbSet<SystemParameter> SystemParameters => Set<SystemParameter>();
    public DbSet<LoginAudit> LoginAudits => Set<LoginAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureEmployee(modelBuilder);
        ConfigureEmployment(modelBuilder);
        ConfigureOrganization(modelBuilder);
        ConfigureProfileSections(modelBuilder);
        ConfigureIdentity(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer();
        }
    }

    private static void ConfigureEmployee(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<EmpPersonalInfo>();
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
        var position = modelBuilder.Entity<EmpPositionInfo>();
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

        var contract = modelBuilder.Entity<EmpContractTerms>();
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
        var hierarchy = modelBuilder.Entity<EmpHierarchy>();
        hierarchy.ToTable("EmpHierarchy", "dbo");
        hierarchy.HasKey(record => record.EmployeeHierarchyId);
        hierarchy.Property(record => record.EmployeeHierarchyId).HasColumnName("EmpHierarchyID");
        hierarchy.Property(record => record.EmployeeId).HasColumnName("EmpID");
        hierarchy.Property(record => record.AssignmentId).HasColumnName("EmpPosID");
        hierarchy.Property(record => record.ElementId).HasColumnName("HElementID");
        hierarchy.Property(record => record.LevelId).HasColumnName("HLevelID");

        MapCodeDescription<Company>(modelBuilder, "Company", record => record.CompanyId,
            "CompanyID", record => record.Code, "CompanyCode", record => record.Name, "CompanyName");
        MapCodeDescription<Position>(modelBuilder, "Position", record => record.PositionId,
            "PositionID", record => record.Code, "PositionCode", record => record.Description, "PositionDesc");
        MapCodeDescription<Rank>(modelBuilder, "Rank", record => record.RankId,
            "RankID", record => record.Code, "RankCode", record => record.Description, "RankDesc");
        MapCodeDescription<EmploymentType>(modelBuilder, "EmploymentType", record => record.EmploymentTypeId,
            "EmploymentTypeID", record => record.Code, "EmploymentTypeCode", record => record.Description, "EmploymentTypeDesc");

        var level = modelBuilder.Entity<HierarchyLevel>();
        level.ToTable("HierarchyLevel", "dbo");
        level.HasKey(record => record.LevelId);
        level.Property(record => record.LevelId).HasColumnName("HLevelID");
        level.Property(record => record.Code).HasColumnName("HLevelCode");
        level.Property(record => record.Description).HasColumnName("HLevelDesc");
        level.Property(record => record.Sequence).HasColumnName("HLevelSeqNo");

        var element = modelBuilder.Entity<HierarchyElement>();
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
        var bankAccount = modelBuilder.Entity<EmpBankAccount>();
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

        var bank = modelBuilder.Entity<BankList>();
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
        var spouse = modelBuilder.Entity<EmpSpouse>();
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

        var dependant = modelBuilder.Entity<EmpDependant>();
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

        var emergency = modelBuilder.Entity<EmpEmergencyContact>();
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
        var employeeSkill = modelBuilder.Entity<EmpSkill>();
        employeeSkill.ToTable("EmpSkill", "dbo");
        employeeSkill.HasKey(record => record.EmployeeSkillId);
        employeeSkill.Property(record => record.EmployeeSkillId).HasColumnName("EmpSkillID");
        employeeSkill.Property(record => record.EmployeeId).HasColumnName("EmpID");
        employeeSkill.Property(record => record.SkillId).HasColumnName("SkillID");
        employeeSkill.Property(record => record.SkillLevelId).HasColumnName("SkillLevelID");
        MapCodeDescription<Skill>(modelBuilder, "Skill", record => record.SkillId,
            "SkillID", record => record.Code, "SkillCode", record => record.Description, "SkillDesc");
        MapCodeDescription<SkillLevel>(modelBuilder, "SkillLevel", record => record.SkillLevelId,
            "SkillLevelID", record => record.Code, "SkillLevelCode", record => record.Description, "SkillLevelDesc");

        var employeeQualification = modelBuilder.Entity<EmpQualification>();
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
        MapCodeDescription<Qualification>(modelBuilder, "Qualification", record => record.QualificationId,
            "QualificationID", record => record.Code, "QualificationCode", record => record.Description, "QualificationDesc");

        var work = modelBuilder.Entity<EmpWorkExp>();
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
        var document = modelBuilder.Entity<EmpDocument>();
        document.ToTable("EmpDocument", "dbo");
        document.HasKey(record => record.DocumentId);
        document.Property(record => record.DocumentId).HasColumnName("EmpDocumentID");
        document.Property(record => record.EmployeeId).HasColumnName("EmpID");
        document.Property(record => record.DocumentTypeId).HasColumnName("DocumentTypeID");
        document.Property(record => record.OriginalFileName).HasColumnName("EmpDocumentOriginalFileName");
        document.Property(record => record.Description).HasColumnName("EmpDocumentDesc");
        document.Property(record => record.IsCompressed).HasColumnName("EmpDocumentIsCompressed");
        document.Property(record => record.IsProfilePhoto).HasColumnName("EmpDocumentIsProfilePhoto");
        MapCodeDescription<DocumentType>(modelBuilder, "DocumentType", record => record.DocumentTypeId,
            "DocumentTypeID", record => record.Code, "DocumentTypeCode", record => record.Description, "DocumentTypeDesc");
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<Users>();
        user.ToTable("Users", "dbo");
        user.HasKey(record => record.UserId).HasName("PK_Users");
        user.Property(record => record.UserId).HasColumnName("UserID").ValueGeneratedOnAdd();
        user.Property(record => record.LoginId).HasColumnName("LoginID").HasMaxLength(20);
        user.Property(record => record.UserName).HasColumnName("UserName").HasMaxLength(100);
        user.Property(record => record.UserEmail).HasColumnName("UserEmail").HasMaxLength(50);
        user.Property(record => record.UserMobileNo).HasColumnName("UserMobileNo").HasMaxLength(20);
        user.Property(record => record.PasswordHash).HasColumnName("UserPassword").HasMaxLength(255);
        user.Property(record => record.AccountStatus).HasColumnName("UserAccountStatus").HasMaxLength(1);
        user.Property(record => record.ExpiryDate).HasColumnName("ExpiryDate").HasColumnType("datetime");
        user.Property(record => record.UserChangePassword).HasColumnName("UserChangePassword");
        user.Property(record => record.UserChangePasswordPeriod).HasColumnName("UserChangePasswordPeriod");
        user.Property(record => record.UserChangePasswordUnit).HasColumnName("UserChangePasswordUnit").HasMaxLength(1);
        user.Property(record => record.UserChangePasswordDate).HasColumnName("UserChangePasswordDate").HasColumnType("datetime");
        user.Property(record => record.FailCount).HasColumnName("FailCount");
        user.Property(record => record.UserLanguage).HasColumnName("UserLanguage").HasMaxLength(10);
        user.Property(record => record.UserIsKeepConnected).HasColumnName("UserIsKeepConnected");
        user.Property(record => record.UsersCannotCreateUsersWithMorePermission)
            .HasColumnName("UsersCannotCreateUsersWithMorePermission");

        var parameter = modelBuilder.Entity<SystemParameter>();
        parameter.ToTable("SystemParameter", "dbo");
        parameter.HasKey(record => record.ParameterCode).HasName("PK_SystemParameter");
        parameter.Property(record => record.ParameterCode).HasColumnName("ParameterCode").HasMaxLength(100);
        parameter.Property(record => record.ParameterDescription).HasColumnName("ParameterDesc").HasMaxLength(200);
        parameter.Property(record => record.ParameterValue).HasColumnName("ParameterValue").HasColumnType("ntext");

        var audit = modelBuilder.Entity<LoginAudit>();
        audit.ToTable("LoginAudit", "dbo");
        audit.HasKey(record => record.LoginAuditId).HasName("PK_LoginAudit");
        audit.Property(record => record.LoginAuditId).HasColumnName("LoginAuditID").ValueGeneratedOnAdd();
        audit.Property(record => record.UserId).HasColumnName("UserID");
        audit.Property(record => record.LoginId).HasColumnName("LoginAuditLoginID").HasMaxLength(255);
        audit.Property(record => record.LoginMachine).HasColumnName("LoginAuditLoginMachine").HasMaxLength(255);
        audit.Property(record => record.LoginIpAddress).HasColumnName("LoginAuditLoginIPAddress").HasMaxLength(255);
        audit.Property(record => record.LoginAgent).HasColumnName("LoginAuditLoginAgent").HasColumnType("ntext");
        audit.Property(record => record.LoginDateTime).HasColumnName("LoginAuditLoginDateTime").HasColumnType("datetime");
        audit.Property(record => record.IsLoginFail).HasColumnName("LoginAuditIsLoginFail");
        audit.Property(record => record.LoginErrorMessage).HasColumnName("LoginAuditLoginErrorMesage").HasMaxLength(255);
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
