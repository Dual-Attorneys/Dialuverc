using Dialuverc.Editor.Base.Localization;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;
using System.Collections.Immutable;

namespace DualAttorneys.Dialuverc.Editor.Deductions.ThoughtsArea
{
    /// <summary>
    /// Groups all of <see cref="ThoughtsEditorArea"/>'s state for history management.
    /// </summary>
    public record class ThoughtsEditorState
    {
        public required ImmutableList<EditorThought> Thoughts { get; init; }

        public required ThoughtsEditorFields? AddScratchpad { get; init; }
        public required ThoughtsEditorFields? EditScratchpad { get; init; }

        public required ImmutableDictionary<string, ImmutableDictionary<string, string>> LocalizationLookup { get; init; }

        public required AddEditEnum ScratchpadSelection { get; init; }

        /// <summary>
        /// Which thought was selected at the time this state was saved.
        /// </summary>
        public required ThoughtGuid? ThoughtSelection { get; init; }
    }
}
