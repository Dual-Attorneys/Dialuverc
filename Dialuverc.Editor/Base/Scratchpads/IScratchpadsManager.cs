using Dialuverc.Editor.Base.Observability;

namespace Dialuverc.Editor.Base.Scratchpads
{
    /// <summary>
    /// Represents an object which manages 1 or more scratchpad objects of type <typeparamref name="TScratchpad"/>.<br/>
    /// <para>Exposes the currently active scratchpad.</para>
    /// </summary>
    /// <typeparam name="TScratchpad">The scratchpad's type.</typeparam>
    public interface IScratchpadsManager<TScratchpad>
    {
        public TScratchpad? ActiveScratchpad { get; }
    }
}
