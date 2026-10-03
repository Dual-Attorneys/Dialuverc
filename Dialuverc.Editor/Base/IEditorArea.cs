using Dialuverc.Editor.Base.Project;
using Dialuverc.Editor.Base.StateTracking;
using Dialuverc.Editor.Base.Verifier;

namespace Dialuverc.Editor.Base
{
    /// <summary>
    /// Represents one of multiple editor areas, each allowing to work on a specific part of the narrative system.
    /// <para>Implementations provide basic undo/redo, verification and exporting/importing functionality.</para>
    /// </summary>
    public interface IEditorArea : IProjectTrackable, IVerifiable
    {
        // TODO: Ideally, these should also be moved to their own interface.
        // Doing so, however, requires reworking how EditorArea keeps track of states (as a general-purpose object would be needed instead).
        public bool CanUndo { get; }
        public bool CanRedo { get; }

        public event Action? OnStateChanged;

        public void RestorePreviousState(RestoreDirection direction);
    }
}
