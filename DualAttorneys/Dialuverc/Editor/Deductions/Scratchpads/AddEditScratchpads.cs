using Dialuverc.Editor.Base.Observability;
using Dialuverc.Editor.Base.Scratchpads;

namespace DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads
{
    public class AddEditScratchpads<T> : IScratchpadsManager<AddEditEnum, T>
    {
        public ObservableValue<AddEditEnum> Selection { get; private set; }

        public AddEditScratchpads(AddEditEnum defaultValue)
        {
            // In similar spirit to exhaustive switches.
            if (defaultValue != AddEditEnum.Add && defaultValue != AddEditEnum.Edit)
                throw new InvalidOperationException($"{defaultValue} does not map to a valid scratchpad");

            Selection = new ObservableValue<AddEditEnum>(defaultValue);
        }

        public T? AddScratchpad { get; set; }
        public T? EditScratchpad { get; set; }

        /// <summary>
        /// The appropriate scratchpad based on the <see cref="ObservableValue{E}.Value"/>.
        /// <para>Shorthand for <code>this[<see cref="ObservableValue{E}.Value"/>]</code></para>
        /// </summary>
        public T? ActiveScratchpad
        {
            get
            {
                return Selection.Value switch
                {
                    AddEditEnum.Add => AddScratchpad,
                    AddEditEnum.Edit => EditScratchpad,

                    _ => throw new InvalidOperationException($"{Selection.Value} does not map to a valid scratchpad")
                };
            }

            set
            {
                switch (Selection.Value)
                {
                    case AddEditEnum.Add:

                        AddScratchpad = value;
                        break;

                    case AddEditEnum.Edit:

                        EditScratchpad = value;
                        break;

                    default:
                        throw new InvalidOperationException($"{Selection.Value} does not map to a valid scratchpad");
                }
            }
        }
    }
}
