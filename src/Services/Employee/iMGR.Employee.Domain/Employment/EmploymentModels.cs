namespace IMGR.Employee.Domain.Employment;

public sealed record EmploymentAssignment(
    int AssignmentId,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    int? CompanyId,
    string? CompanyCode,
    string? CompanyName,
    int? PositionId,
    string? PositionCode,
    string? PositionDescription,
    int? RankId,
    string? RankCode,
    string? RankDescription,
    int? EmploymentTypeId,
    string? EmploymentTypeCode,
    string? EmploymentTypeDescription,
    IReadOnlyList<OrganizationAssignment> Organizations);

public sealed record OrganizationAssignment(
    int ElementId,
    string? ElementCode,
    string? ElementDescription,
    int? LevelId,
    string? LevelCode,
    string? LevelDescription,
    int? LevelSequence);

public sealed record EmploymentContract(
    int ContractId,
    string? CompanyName,
    string? CompanyContactNumber,
    string? CompanyAddress,
    DateOnly? EmployedFrom,
    DateOnly? EmployedTo,
    decimal? Gratuity,
    string? CurrencyCode,
    string? GratuityMethod);

public static class EmploymentAssignmentSelector
{
    public static EmploymentAssignment? SelectAsOf(
        IEnumerable<EmploymentAssignment> assignments,
        DateOnly asOfDate)
    {
        var materialized = assignments as IReadOnlyCollection<EmploymentAssignment> ?? assignments.ToArray();
        return materialized
            .Where(assignment => assignment.EffectiveFrom is not null && assignment.EffectiveFrom <= asOfDate)
            .OrderByDescending(assignment => assignment.EffectiveFrom)
            .ThenByDescending(assignment => assignment.AssignmentId)
            .FirstOrDefault()
            ?? materialized
                .Where(assignment => assignment.EffectiveTo is null)
                .OrderByDescending(assignment => assignment.EffectiveFrom)
                .ThenByDescending(assignment => assignment.AssignmentId)
                .FirstOrDefault();
    }
}
