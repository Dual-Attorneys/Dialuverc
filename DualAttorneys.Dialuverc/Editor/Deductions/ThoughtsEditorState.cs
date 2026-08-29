using Dialuverc.Editor.Base.Modes;
using DualAttorneys.Dialuverc.Deductions;
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

        public readonly EditorModeManager.Mode Mode;

        /// <summary>
        /// Which thought was selected at the time this state was saved.
        /// </summary>
        public readonly ThoughtGuid? ThoughtSelection;

        public ThoughtsEditorState(ImmutableList<EditorThought> thoughts, EditorThought? addBuilder, EditorThought? editBuilder, EditorModeManager.Mode mode, ThoughtGuid? thoughtSelection)
        {
            Thoughts = thoughts;
            AddBuilder = addBuilder;
            EditBuilder = editBuilder;
            Mode = mode;
            ThoughtSelection = thoughtSelection;
        }
    }
}
