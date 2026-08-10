namespace Dialuverc.Editor.Base
{
    /// <summary>
    /// An <see cref="EditorArea{T}"/> which allows progressive editing of 2 <typeparamref name="TBuilder"/>s (possibly at the same time).
    /// <para>
    /// Creation and editing happen respectively in <see cref="Mode.Add"/> and <see cref="Mode.Edit"/> mode.<br/>
    /// Each mode uses its own builder (with undo/redo support).
    /// </para>
    /// </summary>
    /// <remarks>The UI is responsible for appropriately initializing and updating based on this.</remarks>
    /// <typeparam name="TBuilder">The type this area edits.</typeparam>
    /// <typeparam name="TState">The type encapsulating state for this area's undo/redo.</typeparam>
    public abstract class ModeEditorArea<TBuilder, TState> : EditorArea<TState>
    {
        // Add is currently assumed to be the default mode.
        // Implementations can override this.
        public virtual Mode CurrentMode { get; private set; } = Mode.Add;

        /// <summary>
        /// Builder to be used for Add/Insert operations.
        /// </summary>
        protected TBuilder _addingBuilder = default!;

        /// <summary>
        /// Builder to be used for any operation that modifies items that already exist in the list.
        /// </summary>
        protected TBuilder? _editingBuilder;

        /// <summary>
        /// The appropriate builder based on the <see cref="CurrentMode"/>.<br/>
        /// Guaranteed to never be <see langword="null"/>.
        /// </summary>
        protected TBuilder ActiveBuilder
        {
            get
            {
                switch (CurrentMode)
                {
                    case Mode.Add:
                        return _addingBuilder;

                    case Mode.Edit:

                        if (_editingBuilder is null)
                            throw new InvalidOperationException($"Can't get {nameof(ActiveBuilder)} in {Mode.Edit} mode with null {nameof(_editingBuilder)}");

                        return _editingBuilder;

                    default: throw new NotImplementedException();
                }
            }

            set
            {
                switch (CurrentMode)
                {
                    case Mode.Add:

                        _addingBuilder = value!;
                        break;

                    case Mode.Edit:

                        _editingBuilder = value;
                        break;

                    default: throw new NotImplementedException();
                }
            }
        }

        public event Action<Mode>? OnModeChanged;

        public ModeEditorArea()
        {
            _addingBuilder = CreateDefaultBuilder();
        }

        /// <summary>
        /// Sets which builder to use based on the passed <paramref name="newMode"/>.<br/>
        /// All changes happen on the currently active builder.
        /// <para></para>
        /// </summary>
        public void ChangeMode(Mode newMode)
        {
            if (newMode == CurrentMode)
                return;

            CurrentMode = newMode;

            OnModeChanged?.Invoke(CurrentMode);
        }

        protected abstract TBuilder CreateDefaultBuilder();

        public enum Mode
        {
            Add,
            Edit
        }
    }
}
