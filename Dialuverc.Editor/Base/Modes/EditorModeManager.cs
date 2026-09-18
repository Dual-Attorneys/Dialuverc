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

        /// <summary>
        /// Attempts to change the <see cref="CurrentMode"/>.<br/>
        /// No-op if <paramref name="newMode"/> is equal to <see cref="CurrentMode"/>.
        /// </summary>
        /// <param name="newMode"></param>
        /// <param name="invokeEvent">Whether to invoke <see cref="OnModeChanged"/> on success.</param>
        /// <returns><see langword="true"/> if mode was changed, <see langword="false"/> otherwise.</returns>
        public bool ChangeMode(Mode newMode, bool invokeEvent = true)
        {
            if (newMode == CurrentMode)
                return false;

            CurrentMode = newMode;

            if (invokeEvent)
                OnModeChanged?.Invoke(CurrentMode);

            return true;
        }

        public enum Mode
        {
            Add,
            Edit
        }
    }
}
