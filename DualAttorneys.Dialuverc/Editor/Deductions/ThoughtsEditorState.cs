using DualAttorneys.Dialuverc.Deductions;
using System.Collections.Immutable;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    /// <summary>
    /// Groups all of <see cref="ThoughtsEditorArea"/>'s state for history management.
    /// </summary>
    public class ThoughtsEditorState
    {
        public readonly ImmutableList<Thought> Thoughts;

        /// <summary>
        /// Which thought was selected at the time this state was saved.
        /// </summary>
        public readonly Guid ThoughtSelection;

        public ThoughtsEditorState(ImmutableList<Thought> thoughts, Guid thoughtSelection)
        {
            Thoughts = thoughts;
            ThoughtSelection = thoughtSelection;
        }
    }
}
