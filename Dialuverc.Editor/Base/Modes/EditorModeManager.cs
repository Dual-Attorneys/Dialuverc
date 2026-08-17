namespace Dialuverc.Editor.Base.Modes
{
    /// <summary>
    /// An abstract component which manages in which mode an <see cref="EditorArea{T}"/> works.
    /// <para>Can be used to safely expose a concrete implementation.</para>
    /// </summary>
    // Note: This can later be refactored to take in an arbitrary enum as <T>.
    // Implementations would then inherit from this<ArbitraryEnum>.
    public abstract class EditorModeManager
    {
        // Add is currently assumed to be the default mode.
        public Mode CurrentMode { get; private set; } = Mode.Add;

        public event Action<Mode>? OnModeChanged;

        public void ChangeMode(Mode newMode)
        {
            if (newMode == CurrentMode)
                return;

            CurrentMode = newMode;

            OnModeChanged?.Invoke(CurrentMode);
        }

        public enum Mode
        {
            Add,
            Edit
        }
    }
}
