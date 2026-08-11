using Dialuverc.Editor.Base;
using System.Collections.Immutable;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    public class DeductionsEditorState
    {
        public readonly ImmutableList<EditorDeduction> Deductions;

        public readonly EditorDeduction AddBuilder;
        public readonly EditorDeduction? EditBuilder;

        public readonly EditorScratchpadManager.Mode Mode;

        public readonly Guid? DeductionSelection;

        public DeductionsEditorState(ImmutableList<EditorDeduction> deductions, EditorDeduction addBuilder, EditorDeduction? editBuilder, EditorScratchpadManager.Mode mode, Guid? deductionSelection)
        {
            Deductions = deductions;
            AddBuilder = addBuilder;
            EditBuilder = editBuilder;
            Mode = mode;
            DeductionSelection = deductionSelection;
        }
    }
}
