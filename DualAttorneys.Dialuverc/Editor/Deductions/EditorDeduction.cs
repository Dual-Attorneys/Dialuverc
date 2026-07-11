using DualAttorneys.Dialuverc.Deductions;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    public record class EditorDeduction
    {
        /// <summary>
        /// Used to strongly identify this deduction even if the container it lives in changes.
        /// </summary>
        public readonly Guid Guid;

        public ThoughtCombination Combination { get; init; }
        public DeductionOutputs Outputs { get; init; }

        /// <summary>
        /// A user-defined human-readable <see langword="string"/> that weakly identifies this deduction.
        /// </summary>
        public string Alias { get; init; } = string.Empty;

        /// <summary>
        /// A user-defined <see langword="string"/> that contains information about this deduction.
        /// </summary>
        public string EditorNote { get; init; } = string.Empty;

        public EditorDeduction(Guid guid, ThoughtCombination combination, DeductionOutputs outputs)
        {
            Guid = guid;

            Combination = combination;
            Outputs = outputs;
        }
    }
}
