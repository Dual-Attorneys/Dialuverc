namespace Dialuverc.Editor.Base.StateTracking
{
    /// <summary>
    /// Represents an object which can keep track of a single timeline of state changes.
    /// <para>Provides undo/redo capabilities.</para>
    /// </summary>
    public interface IStateHistoryTracker
    {
        public bool CanUndo { get; }
        public bool CanRedo { get; }

        public event Action? OnStateChanged;

        public void RestorePreviousState(RestoreDirection direction);
    }
}
