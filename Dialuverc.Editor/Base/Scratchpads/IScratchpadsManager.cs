using Dialuverc.Editor.Base.Observability;

namespace Dialuverc.Editor.Base.Scratchpads
{
    /// <summary>
    /// Represents an object which manages 1 or more scratchpad objects of type <typeparamref name="TScratchpad"/>.<br/>
    /// Each is identified with a value from <typeparamref name="TEnum"/>.
    /// <para>Exposes the current scratchpad selection and a mechanism to change it.</para>
    /// </summary>
    /// <typeparam name="TEnum">The <see cref="Enum"/> defining valid values to identify each scratchpad.</typeparam>
    /// <typeparam name="TScratchpad">The scratchpad's type.</typeparam>
    public interface IScratchpadsManager<TEnum, TScratchpad> where TEnum : struct, Enum
    {
        public EnumSelection<TEnum> Selection { get; }

        public TScratchpad? ActiveScratchpad { get; }
    }
}
