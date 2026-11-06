using Dialuverc.Editor.Base.Observability;
using Dialuverc.Editor.Base.Scratchpads;

namespace DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads
{
    public class AddEditScratchpads<T> : IReadOnlyScratchpadsManager<T>
    {
        public IReadOnlyObservableValue<AddEditEnum> Selection { get; private set; }

        public AddEditScratchpads(IReadOnlyObservableValue<AddEditEnum> selection)
        {
            // In similar spirit to exhaustive switches.
            if (selection.Value != AddEditEnum.Add && selection.Value != AddEditEnum.Edit)
                throw new InvalidOperationException($"{selection} does not map to a valid scratchpad");

            Selection = selection;
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
