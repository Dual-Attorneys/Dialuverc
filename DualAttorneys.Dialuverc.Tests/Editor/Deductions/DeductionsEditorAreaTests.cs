using Dialuverc.Editor.Base;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions;

using static Dialuverc.Editor.Base.Modes.EditorModeManager;

namespace DualAttorneys.Dialuverc.Tests.Editor.Deductions
{
    internal class DeductionsEditorAreaTests
    {
        TestDeductionsEditorArea _area;

        [SetUp]
        public void SetUp()
        {
            _area = new TestDeductionsEditorArea();
        }

        private class TestDeductionsEditorArea : DeductionsEditorArea
        {
            public new int CurrentStateIndex => base.CurrentStateIndex;
        }
    }
}
