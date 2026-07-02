using DualAttorneys.Dialuverc.Deductions;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    /// <summary>
    /// Wraps a <see cref="Thought"/> with additional editor-only information.
    /// </summary>
    public record class EditorThought
    {
        public Thought RuntimeThought { get; init; }

        /// <summary>
        /// A user-defined <see langword="string"/> that contains information about the <see cref="RuntimeThought"/>.
        /// </summary>
        public string EditorNote { get; init; } = string.Empty;

        public EditorThought(Thought runtimeThought)
        {
            RuntimeThought = runtimeThought;
        }
    }
}
