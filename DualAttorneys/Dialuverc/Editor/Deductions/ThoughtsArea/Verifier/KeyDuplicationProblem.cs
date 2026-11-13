using Dialuverc.Editor.Base.Verifier;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;

namespace DualAttorneys.Dialuverc.Editor.Deductions.ThoughtsArea.Verifier
{
    /// <summary>
    /// Pairs a <see cref="Problem"/> related to keys defined from multiple sources with the scratchpad causing it.
    /// </summary>
    internal class KeyDuplicationProblem : Problem
    {
        public readonly AddEditEnum AffectedScratchpad;

        public KeyDuplicationProblem(ProblemSeverity severity, string description, AddEditEnum scratchpad)
            : base(severity, description)
        {
            AffectedScratchpad = scratchpad;
        }
    }
}
