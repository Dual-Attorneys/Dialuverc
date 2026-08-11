namespace Dialuverc.Editor.Base
{
    /// <summary>
    /// A component which manages temporary scratchpad objects of type <typeparamref name="T"/> for an <see cref="EditorArea{T}"/> to work on.
    /// <para>
    /// To safely expose this to other systems, go through the base <see cref="EditorScratchpadManager"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type the associated <see cref="EditorArea{T}"/> works on.</typeparam>
    public class EditorScratchpadManager<T> : EditorScratchpadManager
    {
        /// <summary>
        /// Builder to be used for Add/Insert operations.
        /// </summary>
        public T AddBuilder = default!;

        /// <summary>
        /// Builder to be used for any operation that modifies items that already exist in the list.
        /// </summary>
        public T? EditBuilder;

        /// <summary>
        /// The appropriate builder based on the <see cref="EditorScratchpadManager.CurrentMode"/>.<br/>
        /// Guaranteed to never be <see langword="null"/>.
        /// </summary>
        public T ActiveBuilder
        {
            get
            {
                switch (CurrentMode)
                {
                    case Mode.Add:
                        return AddBuilder;

                    case Mode.Edit:

                        if (EditBuilder is null)
                            throw new InvalidOperationException($"Can't get {nameof(ActiveBuilder)} in {Mode.Edit} mode with null {nameof(EditBuilder)}");

                        return EditBuilder;

                    default: throw new NotImplementedException();
                }
            }

            set
            {
                switch (CurrentMode)
                {
                    case Mode.Add:

                        AddBuilder = value!;
                        break;

                    case Mode.Edit:

                        EditBuilder = value;
                        break;

                    default: throw new NotImplementedException();
                }
            }
        }
    }

    /// <summary>
    /// An abstract component which manages temporary scratchpad objects for an <see cref="EditorArea{T}"/> to work on.
    /// <para>Can be used to safely expose a concrete implementation.</para>
    /// </summary>
    // Note: This can later be refactored to take in an arbitrary enum as <T>.
    // Implementations would then inherit from this<ArbitraryEnum>.
    public abstract class EditorScratchpadManager
    {
        // Add is currently assumed to be the default mode.
        public Mode CurrentMode { get; private set; } = Mode.Add;

        public event Action<Mode>? OnModeChanged;

        /// <summary>
        /// Sets which builder to use based on the passed <paramref name="newMode"/>.<br/>
        /// All changes happen on the currently active builder.
        /// </summary>
        public void ChangeMode(Mode newMode)
        {
            if (newMode == CurrentMode)
                return;

            CurrentMode = newMode;

            OnModeChanged?.Invoke(CurrentMode);
        }

        public enum Mode
        {
            Add,
            Edit
        }
    }
}
