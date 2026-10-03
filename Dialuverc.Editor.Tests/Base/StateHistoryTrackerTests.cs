using Dialuverc.Editor.Base.StateTracking;
using System.Text;

namespace Dialuverc.Editor.Tests.Base
{
    internal class StateHistoryTrackerTests
    {
        const string _baseState = "BaseState";

        TestStateHistoryTracker _testStateHistoryTracker;

        [SetUp]
        public void SetUp()
        {
            _testStateHistoryTracker = new TestStateHistoryTracker(_baseState);
        }

        [Test]
        public void BaseStateCreated()
        {
            _testStateHistoryTracker.ChangeState("UserState");

            Assert.That(_testStateHistoryTracker.SavedStates.Count, Is.EqualTo(2));

            Assert.That(_testStateHistoryTracker.SavedStates[0], Is.EqualTo(_baseState));
            Assert.That(_testStateHistoryTracker.SavedStates[1], Is.EqualTo("UserState"));
        }

        [Test]
        public void MaxStateAmountEnforced()
        {
            for (int i = 0; i < _testStateHistoryTracker.MaxStates + 1; i++)
            {
                _testStateHistoryTracker.ChangeState($"State{i}");
            }

            Assert.That(_testStateHistoryTracker.SavedStates.Count, Is.EqualTo(_testStateHistoryTracker.MaxStates));
        }

        [Test]
        public void NoCommitIfStateIsIdentical()
        {
            Assert.That(_testStateHistoryTracker.SavedStates, Has.Count.EqualTo(0));

            _testStateHistoryTracker.ChangeState(_baseState);

            Assert.That(_testStateHistoryTracker.SavedStates, Has.Count.EqualTo(1));

            _testStateHistoryTracker.ChangeState("UserState");

            Assert.That(_testStateHistoryTracker.SavedStates, Has.Count.EqualTo(2));
        }

        [Test]
        public void RestoreStateBothDirections()
        {
            string state1 = "StateOne";
            string state2 = "StateTwo";

            _testStateHistoryTracker.ChangeState(state1);
            _testStateHistoryTracker.ChangeState(state2);

            Assert.That(_testStateHistoryTracker.CurrentLocalState, Is.EqualTo(state2));

            _testStateHistoryTracker.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_testStateHistoryTracker.CurrentLocalState, Is.EqualTo(state1));

            _testStateHistoryTracker.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_testStateHistoryTracker.CurrentLocalState, Is.EqualTo(_baseState));

            _testStateHistoryTracker.RestorePreviousState(RestoreDirection.Next);
            _testStateHistoryTracker.RestorePreviousState(RestoreDirection.Next);

            Assert.That(_testStateHistoryTracker.CurrentLocalState, Is.EqualTo(state2));
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

            _testStateHistoryTracker.ChangeState(states[0]);
            _testStateHistoryTracker.ChangeState(states[1]);
            _testStateHistoryTracker.ChangeState(states[2]);
            _testStateHistoryTracker.ChangeState(states[3]);

            Assert.That(_testStateHistoryTracker.SavedStates.Count, Is.EqualTo(5));

            Assert.That(_testStateHistoryTracker.CurrentLocalState, Is.EqualTo(states[3]));

            _testStateHistoryTracker.RestorePreviousState(RestoreDirection.Previous);
            _testStateHistoryTracker.RestorePreviousState(RestoreDirection.Previous);

            _testStateHistoryTracker.ChangeState(states[4]);

            Assert.That(_testStateHistoryTracker.SavedStates.Count, Is.EqualTo(4));

            Assert.That(_testStateHistoryTracker.SavedStates[_testStateHistoryTracker.SavedStates.Count - 1], Is.EqualTo(states[4]));

            _testStateHistoryTracker.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_testStateHistoryTracker.CurrentLocalState, Is.EqualTo(states[1]));

            _testStateHistoryTracker.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_testStateHistoryTracker.CurrentLocalState, Is.EqualTo(states[0]));
        }

        [Test]
        public void CommitAndRestoreInvokeEvent()
        {
            int count = 0;

            _testStateHistoryTracker.OnStateChanged += () => { count++; };

            _testStateHistoryTracker.ChangeState("A");
            _testStateHistoryTracker.ChangeState("B");

            Assert.That(count, Is.EqualTo(3));
        }

        private class TestStateHistoryTracker : StateHistoryTracker<byte[]>
        {
            public string CurrentLocalState { get; private set; }

            new public int MaxStates => base.MaxStates;

            new public IReadOnlyList<byte[]> SavedStates => base.SavedStates;

            public TestStateHistoryTracker(string baseState)
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
