using DualAttorneys.Dialuverc.Deductions;
using System.Collections.Immutable;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    /// <summary>
    /// Wraps a <see cref="Thought"/> with additional editor-only information.
    /// </summary>
    public record class EditorThought
    {
        public const string NameLocalizationFieldId = "name";
        public const string DescriptionLocalizationFieldId = "description";

        public Thought RuntimeThought { get; init; }

        /// <summary>
        /// A user-defined <see langword="string"/> that contains information about the <see cref="RuntimeThought"/>.
        /// </summary>
        public string EditorNote { get; init; } = string.Empty;

        /// <summary>
        /// Localized names keyed by locale identifier.
        /// </summary>
        public ImmutableDictionary<string, string> NameLocalizations { get; init; }
            = ImmutableDictionary<string, string>.Empty;

        /// <summary>
        /// Localized descriptions keyed by locale identifier.
        /// </summary>
        public ImmutableDictionary<string, string> DescriptionLocalizations { get; init; }
            = ImmutableDictionary<string, string>.Empty;

        public EditorThought(Thought runtimeThought)
        {
            RuntimeThought = runtimeThought;
        }
    }
}
