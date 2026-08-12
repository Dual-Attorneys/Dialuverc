using DualAttorneys.Dialuverc.Editor.Deductions;

namespace DualAttorneys.Dialuverc.Tests.Editor.Deductions
{
    internal class ThoughtsEditorAreaTests
    {
        TestThoughtsEditorArea _area;

        [SetUp]
        public void SetUp()
        {
            _area = new TestThoughtsEditorArea();
        }

        private class TestThoughtsEditorArea : ThoughtsEditorArea
        {
            public new int CurrentStateIndex => base.CurrentStateIndex;
        }
    }
}
