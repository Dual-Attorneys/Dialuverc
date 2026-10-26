using Dialuverc.Editor.Base.Observability;

namespace Dialuverc.Editor.Base.Scratchpads
{
    /// <summary>
    /// Represents an object which manages 1 or more scratchpad objects of type <typeparamref name="T"/>.<br/>
    /// Each is identified with a value from <typeparamref name="E"/>.
    /// <para>Exposes the current scratchpad selection and a mechanism to change it.</para>
    /// </summary>
    /// <typeparam name="E">The <see cref="Enum"/> defining valid values to identify each scratchpad.</typeparam>
    /// <typeparam name="T">The scratchpad's type.</typeparam>
    public interface IScratchpadsManager<E, T> where E : struct, Enum
    {
        public EnumSelection<E> Selection { get; }

        public T? ActiveScratchpad { get; }
    }
}
