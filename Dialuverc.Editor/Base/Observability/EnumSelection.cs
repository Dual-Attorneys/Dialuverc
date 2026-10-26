namespace Dialuverc.Editor.Base.Observability
{
    /// <typeparam name="T">The <see cref="Enum"/> defining valid values.</typeparam>
    public class EnumSelection<T> where T : struct, Enum
    {
        public T CurrentSelection { get; private set; }

        public event Action<T>? SelectionChanged;

        public EnumSelection() : this(default) { }

        public EnumSelection(T defaultValue)
        {
            CurrentSelection = defaultValue;
        }

        /// <summary>
        /// Attempts to change the <see cref="CurrentSelection"/>.<br/>
        /// No-op if <paramref name="newSelection"/> is equal to <see cref="CurrentSelection"/>.
        /// </summary>
        /// <param name="newSelection"></param>
        /// <param name="invokeEvent">Whether to invoke <see cref="SelectionChanged"/> on success.</param>
        /// <returns><see langword="true"/> if selection was changed, <see langword="false"/> otherwise.</returns>
        public bool ChangeSelection(T newSelection, bool invokeEvent = true)
        {
            if (EqualityComparer<T>.Default.Equals(newSelection, CurrentSelection))
                return false;

            CurrentSelection = newSelection;

            if (invokeEvent)
                SelectionChanged?.Invoke(CurrentSelection);

            return true;
        }
    }
}
