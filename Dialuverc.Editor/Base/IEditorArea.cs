using Dialuverc.Editor.Base.IO;
using Dialuverc.Editor.Base.Verifier;

namespace Dialuverc.Editor.Base
{
    /// <summary>
    /// Represents one of multiple editor areas, each allowing to work on a specific part of the narrative system.
    /// <para>Implementations provide basic undo/redo, verification and exporting/importing functionality.</para>
    /// </summary>
    public interface IEditorArea : IVerifiable, IExportable
    {
        public bool CanUndo { get; }
        public bool CanRedo { get; }

        public event Action? OnStateChanged;

        public void RestorePreviousState(RestoreDirection direction);
    }
}
