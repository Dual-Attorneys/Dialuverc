namespace Dialuverc.Editor.Base.StateTracking
{
    public interface IStateHistoryTracker
    {
        public bool CanUndo { get; }
        public bool CanRedo { get; }

        public event Action? OnStateChanged;

        public void RestorePreviousState(RestoreDirection direction);
    }
}
