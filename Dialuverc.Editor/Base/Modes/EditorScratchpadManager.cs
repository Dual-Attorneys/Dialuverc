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
        /// Used for Add/Insert operations.
        /// </summary>
        public T? AddScratchpad;

        /// <summary>
        /// Used for any operation that modifies items that already exist in the list.
        /// </summary>
        public T? EditScratchpad;

        /// <summary>
        /// The appropriate scratchpad based on the <see cref="EditorModeManager.CurrentMode"/>.
        /// </summary>
        public T ActiveScratchpad
        {
            get
            {
                switch (CurrentMode)
                {
                    case Mode.Add:

                        if (AddScratchpad is null)
                            throw new InvalidOperationException($"Can't get {nameof(ActiveScratchpad)} in {Mode.Add} mode with null {nameof(AddScratchpad)}");

                        return AddScratchpad;

                    case Mode.Edit:

                        if (EditScratchpad is null)
                            throw new InvalidOperationException($"Can't get {nameof(ActiveScratchpad)} in {Mode.Edit} mode with null {nameof(EditScratchpad)}");

                        return EditScratchpad;

                    default: throw new NotImplementedException();
                }
            }

            set
            {
                switch (CurrentMode)
                {
                    case Mode.Add:

                        AddScratchpad = value!;
                        break;

                    case Mode.Edit:

                        EditScratchpad = value;
                        break;

                    default: throw new NotImplementedException();
                }
            }
        }
    }
}
