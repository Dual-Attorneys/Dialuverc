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

        /// <summary>
        /// Which thought was selected at the time this state was saved.
        /// </summary>
        public readonly ThoughtGuid? ThoughtSelection;

        public ThoughtsEditorState(ImmutableList<EditorThought> thoughts, ThoughtGuid? thoughtSelection)
        {
            Thoughts = thoughts;
            ThoughtSelection = thoughtSelection;
        }
    }
}
