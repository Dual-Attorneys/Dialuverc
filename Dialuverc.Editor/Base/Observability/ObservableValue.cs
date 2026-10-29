namespace Dialuverc.Editor.Base.Observability
{
    public class ObservableValue<T> : IReadOnlyObservableValue<T>
    {
        public T? Value { get; private set; }

        public event Action<T>? ValueChanged;

        public ObservableValue() : this(default) { }

        public ObservableValue(T? defaultValue)
        {
            Value = defaultValue;
        }

        /// <summary>
        /// Attempts to change the <see cref="Value"/>.<br/>
        /// No-op if <paramref name="newValue"/> is equal to <see cref="Value"/>.
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="invokeEvent">Whether to invoke <see cref="ValueChanged"/> on success.</param>
        /// <returns><see langword="true"/> if <see cref="Value"/> was changed, <see langword="false"/> otherwise.</returns>
        public bool ChangeValue(T newValue, bool invokeEvent = true)
        {
            if (EqualityComparer<T>.Default.Equals(newValue, Value))
                return false;

            Value = newValue;

            if (invokeEvent)
                ValueChanged?.Invoke(Value);

            return true;
        }
    }
}
