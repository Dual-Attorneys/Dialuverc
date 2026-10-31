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

        public readonly ThoughtsEditorFields? AddScratchpad;
        public readonly ThoughtsEditorFields? EditScratchpad;

        public readonly AddEditEnum Mode;

        /// <summary>
        /// Which thought was selected at the time this state was saved.
        /// </summary>
        public readonly ThoughtGuid? ThoughtSelection;

        public ThoughtsEditorState(ImmutableList<EditorThought> thoughts, AddEditScratchpads<ThoughtsEditorFields> scratchpads, ThoughtGuid? thoughtSelection)
        {
            Thoughts = thoughts;

            AddScratchpad = scratchpads.AddScratchpad;
            EditScratchpad = scratchpads.EditScratchpad;

            Mode = scratchpads.Selection.CurrentSelection;

            ThoughtSelection = thoughtSelection;
        }
    }
}
