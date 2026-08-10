namespace IMGR.Employee.Domain.Profiles;

public sealed record EmployeeBankAccount(
    int BankAccountId,
    string? BankCode,
    string? BankName,
    string? BranchCode,
    string? MaskedAccountNumber,
    string? AccountHolderName,
    bool IsDefault,
    string? Remark);

public sealed record EmployeeSpouse(
    int SpouseId,
    string Surname,
    string OtherName,
    string ChineseName,
    string? MaskedIdentityNumber,
    string? MaskedPassportNumber,
    DateOnly? DateOfBirth);

public sealed record EmployeeDependant(
    int DependantId,
    string Surname,
    string OtherName,
    string ChineseName,
    string? Gender,
    string? Relationship,
    string? MaskedIdentityNumber,
    string? MaskedPassportNumber,
    DateOnly? DateOfBirth);

public sealed record EmergencyContact(
    int EmergencyContactId,
    string Name,
    string? Gender,
    string? Relationship,
    string? DaytimeContactNumber,
    string? NightContactNumber);

public sealed record EmployeeSkill(
    int EmployeeSkillId,
    int? SkillId,
    string? SkillCode,
    string? SkillDescription,
    int? SkillLevelId,
    string? SkillLevelCode,
    string? SkillLevelDescription);

public sealed record EmployeeQualification(
    int EmployeeQualificationId,
    int? QualificationId,
    string? QualificationCode,
    string? QualificationDescription,
    DateOnly? From,
    DateOnly? To,
    string? Institution,
    string? LearningMethod,
    string? Remark);

public sealed record WorkExperience(
    int WorkExperienceId,
    int? FromYear,
    int? FromMonth,
    int? ToYear,
    int? ToMonth,
    string? CompanyName,
    string? Position,
    int? EmploymentTypeId,
    bool IsRelevantExperience,
    string? Remark);

public sealed record EmployeeDocument(
    int DocumentId,
    int? DocumentTypeId,
    string? DocumentTypeCode,
    string? DocumentTypeDescription,
    string? OriginalFileName,
    string? Description,
    bool IsCompressed,
    bool IsProfilePhoto);

public sealed record OrganizationUnit(
    int ElementId,
    string? ElementCode,
    string? ElementDescription,
    int? CompanyId,
    string? CompanyCode,
    string? CompanyName,
    int? LevelId,
    string? LevelCode,
    string? LevelDescription,
    int? LevelSequence);
