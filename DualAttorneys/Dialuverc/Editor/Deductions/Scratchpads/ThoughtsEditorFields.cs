using DualAttorneys.Dialuverc.Deductions;

namespace DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads
{
    /// <summary>
    /// Groups all fields exposed through the UI for state management.
    /// </summary>
    public record class ThoughtsEditorFields
    {
        public string NameKey { get; init; } = string.Empty;
        public string DescriptionKey { get; init; } = string.Empty;

        public CharacterSides Side { get; init; } = CharacterSides.Any;

        public string EditorNote { get; init; } = string.Empty;

        /// <summary>
        /// Checks if any changes were made by comparing current values with the given <see cref="EditorThought"/>.
        /// </summary>
        public bool HasChanges(EditorThought thought)
        {
            return thought.RuntimeThought.NameKey != NameKey ||
                thought.RuntimeThought.DescriptionKey != DescriptionKey ||
                thought.RuntimeThought.Side != Side ||
                thought.EditorNote != EditorNote;
        }

        /// <summary>
        /// Creates a new <see cref="EditorThought"/> using the current field values.
        /// </summary>
        public EditorThought CreateFromCurrentValues(ThoughtGuid guid)
        {
            return new EditorThought(new Thought(guid, NameKey, DescriptionKey, Side))
            {
                EditorNote = EditorNote,
            };
        }
    }
}
