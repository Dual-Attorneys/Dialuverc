using Dialuverc.Editor.Base;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions;

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

        // TODO: This also checks whether FinishBuilding in Add mode actually appends to the list.
        // Move out to its own test if needed.
        [Test]
        public void AddAlias()
        {
            _area.ChangeMode(DeductionsEditorArea.Mode.Add);

            _area.SetAlias("alias");
            _area.FinishBuilding();

            Assert.That(_area.Deductions, Has.Count.EqualTo(1));
            Assert.That(_area.Deductions[0].Alias, Is.EqualTo("alias"));
        }

        [Test]
        public void AddEditorNote()
        {
            _area.ChangeMode(DeductionsEditorArea.Mode.Add);

            _area.SetEditorNote("editor note");
            _area.FinishBuilding();

            Assert.That(_area.Deductions[0].EditorNote, Is.EqualTo("editor note"));
        }

        [Test]
        public void FinishAddResetsBuilder()
        {
            _area.ChangeMode(DeductionsEditorArea.Mode.Add);

            _area.SetAlias("first");
            Guid first = _area.FinishBuilding();

            Guid second = _area.FinishBuilding();

            Assert.That(string.IsNullOrWhiteSpace(_area.Deductions[1].Alias), Is.True);
            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void EditThrowsWithNoSelection()
        {
            _area.ChangeMode(DeductionsEditorArea.Mode.Edit);

            Assert.That(_area.FinishBuilding, Throws.InvalidOperationException);
        }

        // This along with AddAlias should be enough to test _activeBuilder works,
        // which should in turn allow us to run some of the remaining tests on 1 mode only.
        [Test]
        public void EditAlias()
        {
            _area.ChangeMode(DeductionsEditorArea.Mode.Add);

            _area.SetAlias("alias");
            Guid toEdit = _area.FinishBuilding();

            _area.ChangeMode(DeductionsEditorArea.Mode.Edit);

            _area.SelectDeduction(toEdit);
            _area.SetAlias("changedAlias");
            Guid edited = _area.FinishBuilding();

            Assert.That(_area.Deductions, Has.Count.EqualTo(1));
            Assert.That(_area.Deductions[0].Alias, Is.EqualTo("changedAlias"));
            Assert.That(toEdit, Is.EqualTo(edited));
        }

        [Test]
        public void AddOutputThought()
        {
            _area.ChangeMode(DeductionsEditorArea.Mode.Add);

            ThoughtGuid first = new ThoughtGuid();
            ThoughtGuid second = new ThoughtGuid();

            _area.AddOutputThought(first);
            _area.AddOutputThought(second);
            _area.FinishBuilding();

            Assert.That(_area.Deductions[0].Outputs.Thoughts, Has.Length.EqualTo(2));
            Assert.That(_area.Deductions[0].Outputs.Thoughts[0], Is.EqualTo(first));
            Assert.That(_area.Deductions[0].Outputs.Thoughts[1], Is.EqualTo(second));
        }

        [Test]
        public void RemoveOutputThought()
        {
            _area.ChangeMode(DeductionsEditorArea.Mode.Add);

            ThoughtGuid first = new ThoughtGuid();
            ThoughtGuid second = new ThoughtGuid();

            _area.AddOutputThought(first);
            _area.AddOutputThought(second);

            _area.RemoveOutputThought(first);

            _area.FinishBuilding();

            Assert.That(_area.Deductions[0].Outputs.Thoughts, Has.Length.EqualTo(1));
            Assert.That(_area.Deductions[0].Outputs.Thoughts[0], Is.EqualTo(second));
        }

        [Test]
        public void RemoveNonExistingOutputThoughtIsNoOp()
        {
            _area.ChangeMode(DeductionsEditorArea.Mode.Add);

            ThoughtGuid thought = new ThoughtGuid();

            _area.AddOutputThought(thought);
            _area.RemoveOutputThought(new ThoughtGuid());

            Assert.That(_area.CurrentStateIndex, Is.EqualTo(1));
        }

        [Test]
        public void ChangeMode()
        {
            bool eventInvoked = false;

            _area.OnModeChanged += (_) => eventInvoked = true;

            _area.ChangeMode(DeductionsEditorArea.Mode.Add);

            Assert.That(_area.CurrentMode, Is.EqualTo(DeductionsEditorArea.Mode.Add));

            _area.ChangeMode(DeductionsEditorArea.Mode.Edit);

            Assert.That(_area.CurrentMode, Is.EqualTo(DeductionsEditorArea.Mode.Edit));

            Assert.That(eventInvoked, Is.True);
        }

        [Test]
        public void ChangeModeToSameIsNoOp()
        {
            _area.ChangeMode(DeductionsEditorArea.Mode.Add);

            bool eventInvoked = false;

            _area.OnModeChanged += (_) => eventInvoked = true;

            _area.ChangeMode(DeductionsEditorArea.Mode.Add);

            Assert.That(eventInvoked, Is.False);
        }

        [Test]
        public void UndoRestoresMode()
        {
            _area.ChangeMode(DeductionsEditorArea.Mode.Add);

            _area.SetEditorNote("something");

            _area.ChangeMode(DeductionsEditorArea.Mode.Edit);

            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_area.CurrentMode, Is.EqualTo(DeductionsEditorArea.Mode.Add));
        }

        [Test]
        public void SelectDeduction()
        {
            EditorDeduction? selectedDeduction = null;

            _area.OnDeductionSelectionChanged += (d) => selectedDeduction = d;

            _area.ChangeMode(DeductionsEditorArea.Mode.Add);
            Guid toSelect = _area.FinishBuilding();

            _area.SelectDeduction(toSelect);

            Assert.That(selectedDeduction!.Guid, Is.EqualTo(toSelect));
        }

        [Test]
        public void SelectChangesModeToEdit()
        {
            _area.ChangeMode(DeductionsEditorArea.Mode.Add);

            Guid toSelect = _area.FinishBuilding();

            _area.SelectDeduction(toSelect);

            Assert.That(_area.CurrentMode, Is.EqualTo(DeductionsEditorArea.Mode.Edit));
        }

        [Test]
        public void RemoveDeselects()
        {
            EditorDeduction? selectedDeduction = null;

            _area.OnDeductionSelectionChanged += (d) => selectedDeduction = d;

            _area.ChangeMode(DeductionsEditorArea.Mode.Add);
            Guid toSelect = _area.FinishBuilding();

            _area.SelectDeduction(toSelect);
            _area.RemoveDeduction(toSelect);

            Assert.That(selectedDeduction, Is.Null);
        }

        private class TestDeductionsEditorArea : DeductionsEditorArea
        {
            public new int CurrentStateIndex => base.CurrentStateIndex;
        }
    }
}
