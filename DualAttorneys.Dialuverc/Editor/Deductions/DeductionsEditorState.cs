using Dialuverc.Editor.Base.Modes;
using System.Collections.Immutable;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    public class DeductionsEditorState
    {
        public readonly ImmutableList<EditorDeduction> Deductions;

        public readonly EditorDeduction AddBuilder;
        public readonly EditorDeduction? EditBuilder;

        public readonly EditorModeManager.Mode Mode;

        public readonly Guid? DeductionSelection;

        public DeductionsEditorState(ImmutableList<EditorDeduction> deductions, EditorDeduction addBuilder, EditorDeduction? editBuilder, EditorModeManager.Mode mode, Guid? deductionSelection)
        {
            Deductions = deductions;
            AddBuilder = addBuilder;
            EditBuilder = editBuilder;
            Mode = mode;
            DeductionSelection = deductionSelection;
        }
    }
}
