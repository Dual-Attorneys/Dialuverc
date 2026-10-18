namespace Dialuverc.Editor.Base.Modes
{
    /// <summary>
    /// A component which provides temporary scratchpad objects of type <typeparamref name="T"/> for an <see cref="EditorArea{T}"/> to work on.
    /// <para>
    /// To safely expose this to other systems, go through the base <see cref="EditorModeManager{E}"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type the associated <see cref="EditorArea{T}"/> works on.</typeparam>
    /// <typeparam name="E">The enum representing available modes.</typeparam>
    public class EditorScratchpadManager<T, E> : EditorModeManager<E> where E : struct, Enum
    {
        readonly Dictionary<E, T?> _availableScratchpads = new Dictionary<E, T?>();

        /// <summary>
        /// The appropriate scratchpad based on the <see cref="EditorModeManager{E}.CurrentMode"/>.
        /// <para>Shorthand for <code>this[<see cref="EditorModeManager{E}.CurrentMode"/>]</code></para>
        /// </summary>
        public T? ActiveScratchpad
        {
            // Go through this[] to benefit from null checks.
            get => this[CurrentMode];

            set => this[CurrentMode] = value;
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
