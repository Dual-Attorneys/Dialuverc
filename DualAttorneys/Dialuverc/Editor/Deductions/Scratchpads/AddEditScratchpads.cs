using Dialuverc.Editor.Base;
using Dialuverc.Editor.Base.Observability;
using Dialuverc.Editor.Base.Scratchpads;

namespace DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads
{
    /// <summary>
    /// A component which stores temporary scratchpad objects of type <typeparamref name="T"/> for an <see cref="EditorArea{T}"/> to work on.
    /// </summary>
    /// <typeparam name="T">The type the associated <see cref="EditorArea{T}"/> works on.</typeparam>
    public class AddEditScratchpads<T> : IScratchpadsManager<AddEditEnum, T>
    {
        public EnumSelection<AddEditEnum> Selection { get; private set; }

        public AddEditScratchpads(AddEditEnum defaultValue)
        {
            // In similar spirit to exhaustive switches.
            if (defaultValue != AddEditEnum.Add && defaultValue != AddEditEnum.Edit)
                throw new InvalidOperationException($"{defaultValue} does not map to a valid scratchpad");

            Selection = new EnumSelection<AddEditEnum>(defaultValue);
        }

        public T? AddScratchpad { get; set; }
        public T? EditScratchpad { get; set; }

        /// <summary>
        /// The appropriate scratchpad based on the <see cref="EnumSelection{E}.CurrentSelection"/>.
        /// <para>Shorthand for <code>this[<see cref="EnumSelection{E}.CurrentSelection"/>]</code></para>
        /// </summary>
        public T? ActiveScratchpad
        {
            get
            {
                return Selection.CurrentSelection switch
                {
                    AddEditEnum.Add => AddScratchpad,
                    AddEditEnum.Edit => EditScratchpad,

                    _ => throw new InvalidOperationException($"{Selection.CurrentSelection} does not map to a valid scratchpad")
                };
            }

            set
            {
                switch (Selection.CurrentSelection)
                {
                    case AddEditEnum.Add:

                        AddScratchpad = value;
                        break;

                    case AddEditEnum.Edit:

                        EditScratchpad = value;
                        break;

                    default:
                        throw new InvalidOperationException($"{Selection.CurrentSelection} does not map to a valid scratchpad");
                }
                ;
            }
        }
    }
}
