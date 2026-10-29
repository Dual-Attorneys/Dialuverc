namespace Dialuverc.Editor.Base.Observability
{
    /// <summary>
    /// Provides a read-only view of an <see cref="ObservableValue{T}"/>, only allowing to react to changes.
    /// </summary>
    public interface IReadOnlyObservableValue<T>
    {
        public T? Value { get; }

        public event Action<T?> ValueChanged;
    }
}
