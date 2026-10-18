namespace Dialuverc.Editor.Base.Modes
{
    /// <summary>
    /// An abstract component which manages in which mode an <see cref="EditorArea{T}"/> works.
    /// <para>Can be used to safely expose a concrete implementation.</para>
    /// </summary>
    public abstract class EditorModeManager<T> where T : struct, Enum
    {
        public T CurrentMode { get; private set; } = default;

        public event Action<T>? OnModeChanged;

        /// <summary>
        /// Attempts to change the <see cref="CurrentMode"/>.<br/>
        /// No-op if <paramref name="newMode"/> is equal to <see cref="CurrentMode"/>.
        /// </summary>
        /// <param name="newMode"></param>
        /// <param name="invokeEvent">Whether to invoke <see cref="OnModeChanged"/> on success.</param>
        /// <returns><see langword="true"/> if mode was changed, <see langword="false"/> otherwise.</returns>
        public bool ChangeMode(T newMode, bool invokeEvent = true)
        {
            if (EqualityComparer<T>.Default.Equals(newMode, CurrentMode))
                return false;

            CurrentMode = newMode;

            if (invokeEvent)
                OnModeChanged?.Invoke(CurrentMode);

            return true;
        }
    }
}
