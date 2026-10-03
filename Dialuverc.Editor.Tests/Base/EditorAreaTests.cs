using Dialuverc.Editor.Base;
using Dialuverc.Editor.Base.StateTracking;
using System.Text;

namespace Dialuverc.Editor.Tests.Base
{
    internal class EditorAreaTests
    {
        const string _baseState = "BaseState";

        TestEditorArea _testArea;

        [SetUp]
        public void SetUp()
        {
            _testArea = new TestEditorArea(_baseState);
        }

        [Test]
        public void UnsavedChangesUpdatesCorrectly()
        {
            Assert.That(_testArea.HasUnsavedChanges, Is.False);

            _testArea.ChangeState("A");

            Assert.That(_testArea.HasUnsavedChanges, Is.True);

            _testArea.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_testArea.HasUnsavedChanges, Is.False);

            _testArea.ChangeState("B");

            Assert.That(_testArea.HasUnsavedChanges, Is.True);

            _testArea.SetCurrentStateAsSaved();

            Assert.That(_testArea.HasUnsavedChanges, Is.False);
        }

        [Test]
        public void ClearStatesHistory()
        {
            _testArea.ChangeState("A");
            _testArea.ChangeState("B");

            _testArea.ClearStatesHistory();

            Assert.That(_testArea.CanUndo, Is.False);
            Assert.That(_testArea.CanRedo, Is.False);
            Assert.That(_testArea.HasUnsavedChanges, Is.False);

            _testArea.ChangeState("C");
            _testArea.ChangeState("D");

            Assert.That(_testArea.CurrentLocalState, Is.EqualTo("D"));

            _testArea.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_testArea.CurrentLocalState, Is.EqualTo("C"));
        }

        class TestEditorArea : EditorArea<byte[]>
        {
            public string CurrentLocalState { get; private set; }

            new public int MaxStates => base.MaxStates;

            new public IReadOnlyList<byte[]> SavedStates => base.SavedStates;

            public TestEditorArea(string baseState)
            {
                CurrentLocalState = baseState;
            }

            public void ChangeState(string newState)
            {
                BeginChange();

                CurrentLocalState = newState;

                EndChange();
            }

            protected override void ApplyRestoredState(byte[] newState)
            {
                CurrentLocalState = Encoding.UTF8.GetString(newState);
            }

            protected override bool CheckStateEquality(byte[] a, byte[] b)
            {
                if (a is null && b is not null ||
                    a is not null & b is null)
                    return false;

                if (a is null & b is null)
                    return true;

                return a.SequenceEqual(b);
            }

            protected override byte[] GetStateToSave()
            {
                return Encoding.UTF8.GetBytes(CurrentLocalState);
            }
        }
    }
}
