namespace Dialuverc.Editor.Base.Modes
{
    /// <summary>
    /// A component which manages temporary scratchpad objects of type <typeparamref name="T"/> for an <see cref="EditorArea{T}"/> to work on.
    /// <para>
    /// To safely expose this to other systems, go through the base <see cref="EditorModeManager"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type the associated <see cref="EditorArea{T}"/> works on.</typeparam>
    public class EditorScratchpadManager<T> : EditorModeManager
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
        /// The appropriate builder based on the <see cref="EditorModeManager.CurrentMode"/>.<br/>
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
}
