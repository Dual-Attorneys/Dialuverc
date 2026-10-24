using Dialuverc.Editor.Base.Observability;

namespace Dialuverc.Editor.Base
{
    /// <summary>
    /// A component which stores temporary scratchpad objects of type <typeparamref name="T"/> for an <see cref="EditorArea{T}"/> to work on.
    /// </summary>
    /// <typeparam name="T">The type the associated <see cref="EditorArea{T}"/> works on.</typeparam>
    /// <typeparam name="E">The enum representing available modes.</typeparam>
    public class EditorScratchpadManager<T, E> where E : struct, Enum
    {
        public readonly EnumSelection<E> Selection = new EnumSelection<E>();

        readonly Dictionary<E, T?> _availableScratchpads = new Dictionary<E, T?>();

        /// <summary>
        /// The appropriate scratchpad based on the <see cref="EnumSelection{E}.CurrentSelection"/>.
        /// <para>Shorthand for <code>this[<see cref="EnumSelection{E}.CurrentSelection"/>]</code></para>
        /// </summary>
        public T? ActiveScratchpad
        {
            // Go through this[] to benefit from null checks.
            get => this[Selection.CurrentSelection];

            set => this[Selection.CurrentSelection] = value;
        }

        public T? this[E mode]
        {
            get
            {
                if (_availableScratchpads.TryGetValue(mode, out T? value))
                    return value;

                return default;
            }

            set => _availableScratchpads[mode] = value;
        }
    }
}
