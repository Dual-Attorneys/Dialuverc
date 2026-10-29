using Dialuverc.Editor.Base.Localization;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;
using System.Collections.Immutable;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    /// <summary>
    /// Groups all of <see cref="ThoughtsEditorArea"/>'s state for history management.
    /// </summary>
    public class ThoughtsEditorState
    {
        public readonly ImmutableList<EditorThought> Thoughts;

        public readonly EditorThought? AddBuilder;
        public readonly EditorThought? EditBuilder;

        public readonly ImmutableDictionary<string, ImmutableDictionary<string, string>> LocalizationLookup;

        public readonly AddEditEnum Mode;

        /// <summary>
        /// Which thought was selected at the time this state was saved.
        /// </summary>
        public readonly ThoughtGuid? ThoughtSelection;

        public ThoughtsEditorState(ImmutableList<EditorThought> thoughts,
            AddEditScratchpads<EditorThought> scratchpadsManager,
            LocalizationStore localizationManager,
            AddEditEnum mode,
            ThoughtGuid? thoughtSelection)
        {
            Thoughts = thoughts;

            AddBuilder = scratchpadsManager.AddScratchpad;
            EditBuilder = scratchpadsManager.EditScratchpad;

            LocalizationLookup = localizationManager.Lookup;

            Mode = mode;

            ThoughtSelection = thoughtSelection;
        }
    }
}
