namespace Dialuverc.Editor.Base
{
    /// <summary>
    /// Represents one of multiple editor areas, each allowing to work on a specific part of the narrative system.
    /// <para>Implementations provides basic undo/redo functionality.</para>
    /// </summary>
    public interface IEditorArea
    {
        public void RestorePreviousState(RestoreDirection direction);
    }
}
