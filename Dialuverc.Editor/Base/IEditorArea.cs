using Dialuverc.Editor.Base.Verifier;

namespace Dialuverc.Editor.Base
{
    /// <summary>
    /// Represents one of multiple editor areas, each allowing to work on a specific part of the narrative system.
    /// <para>Implementations provide basic undo/redo, verification and exporting functionality.</para>
    /// </summary>
    public interface IEditorArea : IVerifiable
    {
        public bool CanUndo { get; }
        public bool CanRedo { get; }

        public event Action? OnStateChanged;

        public void RestorePreviousState(RestoreDirection direction);

        // TODO: This should be split in a way that allows the "editor project" to be saved
        // separately from the output that will be used by the game.
        public string SerializeForExport(); 
    }
}
