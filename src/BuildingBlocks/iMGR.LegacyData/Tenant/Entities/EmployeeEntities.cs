namespace IMGR.LegacyData.Tenant.Entities;

public partial class EmpPersonalInfo
{
    public int EmployeeId { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? Status { get; set; }
    public string? EnglishSurname { get; set; }
    public string? EnglishOtherName { get; set; }
    public string? ChineseName { get; set; }
    public string? Alias { get; set; }
    public string? IdentityNumber { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime? PassportExpiryDate { get; set; }
    public string? ResidentialAddress { get; set; }
    public string? CorrespondenceAddress { get; set; }
    public DateTime? DateOfJoin { get; set; }
    public DateTime? ServiceDate { get; set; }
    public DateTime? ProbationLastDate { get; set; }
    public string? Email { get; set; }
    public string? InternalEmail { get; set; }
    public string? MobileNumber { get; set; }
    public string? HomePhoneNumber { get; set; }
    public string? OfficePhoneNumber { get; set; }
    public string? Remark { get; set; }
}

public partial class EmpPositionInfo
{
    public int AssignmentId { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int? CompanyId { get; set; }
    public int? PositionId { get; set; }
    public int? RankId { get; set; }
    public int? EmploymentTypeId { get; set; }
}

public partial class EmpHierarchy
{
    public int EmployeeHierarchyId { get; set; }
    public int? EmployeeId { get; set; }
    public int? AssignmentId { get; set; }
    public int? ElementId { get; set; }
    public int? LevelId { get; set; }
}

public partial class Company
{
    public int CompanyId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
}

public partial class Position
{
    public int PositionId { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
}

public partial class Rank
{
    public int RankId { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
}

public partial class EmploymentType
{
    public int EmploymentTypeId { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
}

public partial class HierarchyLevel
{
    public int LevelId { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int? Sequence { get; set; }
}

public partial class HierarchyElement
{
    public int ElementId { get; set; }
    public int? CompanyId { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int? LevelId { get; set; }
}

public partial class EmpContractTerms
{
    public int ContractId { get; set; }
    public int? EmployeeId { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyContactNumber { get; set; }
    public string? CompanyAddress { get; set; }
    public DateTime? EmployedFrom { get; set; }
    public DateTime? EmployedTo { get; set; }
    public decimal? Gratuity { get; set; }
    public string? CurrencyCode { get; set; }
    public string? GratuityMethod { get; set; }
}

public partial class EmpBankAccount
{
    public int BankAccountId { get; set; }
    public int? EmployeeId { get; set; }
    public string? BankCode { get; set; }
    public string? BranchCode { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountHolderName { get; set; }
    public int? IsDefault { get; set; }
    public string? Remark { get; set; }
}

public partial class BankList
{
    public string BankCode { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public partial class EmpSpouse
{
    public int SpouseId { get; set; }
    public int? EmployeeId { get; set; }
    public string? Surname { get; set; }
    public string? OtherName { get; set; }
    public string? ChineseName { get; set; }
    public string? IdentityNumber { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public partial class EmpDependant
{
    public int DependantId { get; set; }
    public int? EmployeeId { get; set; }
    public string? Surname { get; set; }
    public string? OtherName { get; set; }
    public string? ChineseName { get; set; }
    public string? Gender { get; set; }
    public string? IdentityNumber { get; set; }
    public string? PassportNumber { get; set; }
    public string? Relationship { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public partial class EmpEmergencyContact
{
    public int EmergencyContactId { get; set; }
    public int? EmployeeId { get; set; }
    public string? Name { get; set; }
    public string? Gender { get; set; }
    public string? Relationship { get; set; }
    public string? DaytimeContactNumber { get; set; }
    public string? NightContactNumber { get; set; }
}

public partial class EmpSkill
{
    public int EmployeeSkillId { get; set; }
    public int? EmployeeId { get; set; }
    public int? SkillId { get; set; }
    public int? SkillLevelId { get; set; }
}

public partial class Skill
{
    public int SkillId { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
}

public partial class SkillLevel
{
    public int SkillLevelId { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
}

public partial class EmpQualification
{
    public int EmployeeQualificationId { get; set; }
    public int? EmployeeId { get; set; }
    public int? QualificationId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Institution { get; set; }
    public string? Remark { get; set; }
    public string? LearningMethod { get; set; }
}

public partial class Qualification
{
    public int QualificationId { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
}

public partial class EmpWorkExp
{
    public int WorkExperienceId { get; set; }
    public int? EmployeeId { get; set; }
    public int? FromYear { get; set; }
    public int? FromMonth { get; set; }
    public int? ToYear { get; set; }
    public int? ToMonth { get; set; }
    public string? CompanyName { get; set; }
    public string? Position { get; set; }
    public string? Remark { get; set; }
    public int? EmploymentTypeId { get; set; }
    public int? IsRelevantExperience { get; set; }
}

public partial class EmpDocument
{
    public int DocumentId { get; set; }
    public int? EmployeeId { get; set; }
    public int? DocumentTypeId { get; set; }
    public string? OriginalFileName { get; set; }
    public string? Description { get; set; }
    public int? IsCompressed { get; set; }
    public int? IsProfilePhoto { get; set; }
}

public partial class DocumentType
{
    public int DocumentTypeId { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
}
