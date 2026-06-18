using Dialuverc.Editor.Base;
using System.Text;

namespace Dialuverc.Editor.Tests.Base
{
    internal class EditorAreaTests
    {
        const string _baseState = "BaseState";

        TestArea _testArea;

        [SetUp]
        public void SetUp()
        {
            _testArea = new TestArea(_baseState);
        }

        [Test]
        public void BaseStateCreated()
        {
            _testArea.ChangeState("UserState");

            Assert.That(_testArea.SavedStates.Count, Is.EqualTo(2));

            Assert.That(_testArea.SavedStates[0], Is.EqualTo(_baseState));
            Assert.That(_testArea.SavedStates[1], Is.EqualTo("UserState"));
        }

        [Test]
        public void MaxStateAmountEnforced()
        {
            for (int i = 0; i < _testArea.MaxStates + 1; i++)
            {
                _testArea.ChangeState($"State{i}");
            }

            Assert.That(_testArea.SavedStates.Count, Is.EqualTo(_testArea.MaxStates));
        }

        [Test]
        public void RestoreStateBothDirections()
        {
            string state1 = "StateOne";
            string state2 = "StateTwo";

            _testArea.ChangeState(state1);
            _testArea.ChangeState(state2);

            Assert.That(_testArea.CurrentState, Is.EqualTo(state2));

            _testArea.RestorePreviousState(EditorArea.RestoreDirection.Previous);

            Assert.That(_testArea.CurrentState, Is.EqualTo(state1));

            _testArea.RestorePreviousState(EditorArea.RestoreDirection.Previous);

            Assert.That(_testArea.CurrentState, Is.EqualTo(_baseState));

            _testArea.RestorePreviousState(EditorArea.RestoreDirection.Next);
            _testArea.RestorePreviousState(EditorArea.RestoreDirection.Next);

            Assert.That(_testArea.CurrentState, Is.EqualTo(state2));
        }

        [Test]
        public void InsertingNewStateDeletesAfter()
        {
            string[] states =
            [
                "State1",
                "State2",
                "State3",
                "State4",
                "State5"
            ];

            _testArea.ChangeState(states[0]);
            _testArea.ChangeState(states[1]);
            _testArea.ChangeState(states[2]);
            _testArea.ChangeState(states[3]);

            Assert.That(_testArea.SavedStates.Count, Is.EqualTo(5));

            Assert.That(_testArea.CurrentState, Is.EqualTo(states[3]));

            _testArea.RestorePreviousState(EditorArea.RestoreDirection.Previous);
            _testArea.RestorePreviousState(EditorArea.RestoreDirection.Previous);

            _testArea.ChangeState(states[4]);

            Assert.That(_testArea.SavedStates.Count, Is.EqualTo(4));

            Assert.That(_testArea.SavedStates[_testArea.SavedStates.Count - 1], Is.EqualTo(states[4]));

            _testArea.RestorePreviousState(EditorArea.RestoreDirection.Previous);

            Assert.That(_testArea.CurrentState, Is.EqualTo(states[1]));

            _testArea.RestorePreviousState(EditorArea.RestoreDirection.Previous);

            Assert.That(_testArea.CurrentState, Is.EqualTo(states[0]));
        }

        private class TestArea : EditorArea
        {
            public string CurrentState { get; private set; }

            new public int MaxStates => base.MaxStates;

            new public IReadOnlyList<byte[]> SavedStates => base.SavedStates;

            public TestArea(string baseState)
            {
                CurrentState = baseState;
            }

            public void ChangeState(string newState)
            {
                BeginChange();

                CurrentState = newState;

                EndChange();
            }

            protected override void ApplyEditorState(byte[] previousState, byte[] newState)
            {
                CurrentState = Encoding.UTF8.GetString(newState);
            }

            protected override byte[] SerializeCurrentEditorState()
            {
                return Encoding.UTF8.GetBytes(CurrentState);
            }

            public override string SerializeForExport()
            {
                throw new NotImplementedException();
            }
        }
    }
}
