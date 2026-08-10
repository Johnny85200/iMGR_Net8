using IMGR.Employee.Domain.Employment;
using IMGR.Employee.Domain.Profiles;

namespace IMGR.Employee.Domain.Employees;

public enum EmployeeStatus
{
    Unknown,
    Active,
    Terminated,
    Pending
}

public static class EmployeeStatusCodes
{
    public static EmployeeStatus Parse(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "A" => EmployeeStatus.Active,
        "T" => EmployeeStatus.Terminated,
        "P" => EmployeeStatus.Pending,
        _ => EmployeeStatus.Unknown
    };

    public static string? ToLegacyCode(EmployeeStatus? value) => value switch
    {
        EmployeeStatus.Active => "A",
        EmployeeStatus.Terminated => "T",
        EmployeeStatus.Pending => "P",
        EmployeeStatus.Unknown => null,
        null => null,
        _ => null
    };
}

public sealed record EmployeeSummary(
    int EmployeeId,
    string EmployeeNumber,
    string DisplayName,
    EmployeeStatus Status,
    DateOnly? DateOfJoin,
    EmploymentAssignment? CurrentAssignment);

public sealed record EmployeeProfile(
    int EmployeeId,
    string EmployeeNumber,
    EmployeeStatus Status,
    string EnglishSurname,
    string EnglishOtherName,
    string ChineseName,
    string Alias,
    string? MaskedIdentityNumber,
    string? Gender,
    string? MaritalStatus,
    DateOnly? DateOfBirth,
    string? Nationality,
    string? MaskedPassportNumber,
    DateOnly? PassportExpiryDate,
    string? ResidentialAddress,
    string? CorrespondenceAddress,
    DateOnly? DateOfJoin,
    DateOnly? ServiceDate,
    DateOnly? ProbationLastDate,
    string? Email,
    string? InternalEmail,
    string? MobileNumber,
    string? HomePhoneNumber,
    string? OfficePhoneNumber,
    string? Remark,
    EmploymentAssignment? CurrentAssignment,
    IReadOnlyList<EmploymentAssignment> PositionHistory,
    IReadOnlyList<EmploymentContract> Contracts,
    IReadOnlyList<EmployeeBankAccount> BankAccounts,
    IReadOnlyList<EmployeeSpouse> Spouses,
    IReadOnlyList<EmployeeDependant> Dependants,
    IReadOnlyList<EmergencyContact> EmergencyContacts,
    IReadOnlyList<EmployeeSkill> Skills,
    IReadOnlyList<EmployeeQualification> Qualifications,
    IReadOnlyList<WorkExperience> WorkExperiences,
    IReadOnlyList<EmployeeDocument> Documents);

public static class SensitiveValueMasker
{
    public static string? Mask(string? value, int visibleSuffixLength = 4)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length <= visibleSuffixLength)
        {
            return new string('*', normalized.Length);
        }

        return new string('*', normalized.Length - visibleSuffixLength) + normalized[^visibleSuffixLength..];
    }
}
