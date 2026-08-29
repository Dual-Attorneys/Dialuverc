using System.Collections.Immutable;

namespace DualAttorneys.Dialuverc.Deductions
{
    public record class DeductionOutputs
    {
        // TODO: Add dialogue and evidence outputs once their respective Guid types exist.

        public ImmutableArray<ThoughtGuid> Thoughts { get; init; } = ImmutableArray<ThoughtGuid>.Empty;
    }
}