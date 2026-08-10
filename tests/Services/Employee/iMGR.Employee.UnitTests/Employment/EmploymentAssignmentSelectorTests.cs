using IMGR.Employee.Domain.Employment;

namespace IMGR.Employee.UnitTests.Employment;

public sealed class EmploymentAssignmentSelectorTests
{
    [Fact]
    public void SelectAsOf_ReturnsLatestAssignmentEffectiveOnOrBeforeDate()
    {
        var assignments = new[]
        {
            Create(1, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31)),
            Create(2, new DateOnly(2025, 1, 1), null),
            Create(3, new DateOnly(2026, 1, 1), null)
        };

        var selected = EmploymentAssignmentSelector.SelectAsOf(assignments, new DateOnly(2025, 6, 1));

        Assert.NotNull(selected);
        Assert.Equal(2, selected.AssignmentId);
    }

    [Fact]
    public void SelectAsOf_WhenNoPastAssignment_ReturnsLatestOpenFutureAssignment()
    {
        var assignments = new[]
        {
            Create(1, new DateOnly(2027, 1, 1), null),
            Create(2, new DateOnly(2028, 1, 1), new DateOnly(2028, 12, 31))
        };

        var selected = EmploymentAssignmentSelector.SelectAsOf(assignments, new DateOnly(2026, 1, 1));

        Assert.NotNull(selected);
        Assert.Equal(1, selected.AssignmentId);
    }

    [Fact]
    public void SelectAsOf_WithNoApplicableAssignment_ReturnsNull()
    {
        var assignments = new[] { Create(1, new DateOnly(2027, 1, 1), new DateOnly(2027, 12, 31)) };

        Assert.Null(EmploymentAssignmentSelector.SelectAsOf(assignments, new DateOnly(2026, 1, 1)));
    }

    private static EmploymentAssignment Create(int id, DateOnly from, DateOnly? to) => new(
        id,
        from,
        to,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        []);
}
