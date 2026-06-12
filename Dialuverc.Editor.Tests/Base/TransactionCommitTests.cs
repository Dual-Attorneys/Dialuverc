using Dialuverc.Editor.Base;

namespace Dialuverc.Editor.Tests.Base
{
    internal class TransactionCommitTests
    {
        TestTransactionalObject _testObject;

        [SetUp]
        public void SetUp()
        {
            _testObject = new TestTransactionalObject();
        }

        [Test]
        public void ChangesAreCommitted()
        {
            _testObject.BeginChange();

            Assert.That(_testObject.CommitsCount, Is.EqualTo(0));
            Assert.That(_testObject.TransactionPending, Is.True);

            _testObject.EndChange();

            Assert.That(_testObject.CommitsCount, Is.EqualTo(1));
            Assert.That(_testObject.TransactionPending, Is.False);
        }

        [Test]
        public void ForceCommitWithNoTransaction()
        {
            _testObject.ForceCommit();

            Assert.That(_testObject.CommitsCount, Is.EqualTo(1));
        }

        [Test]
        public void ForceCommitWithPendingTransaction()
        {
            _testObject.BeginChange();

            _testObject.ForceCommit();

            Assert.That(_testObject.CommitsCount, Is.EqualTo(0));
        }

        [Test]
        public void EndWithoutBeginThrows()
        {
            _testObject.BeginChange();
            _testObject.EndChange();

            Assert.That(_testObject.EndChange, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void OnlyLastEndTriggersCommit()
        {
            _testObject.BeginChange();
            _testObject.BeginChange();

            _testObject.EndChange();

            Assert.That(_testObject.CommitsCount, Is.EqualTo(0));

            _testObject.EndChange();

            Assert.That(_testObject.CommitsCount, Is.EqualTo(1));
        }

        private class TestTransactionalObject : TransactionalObject
        {
            public int CommitsCount { get; private set; }

            protected override void Commit()
            {
                CommitsCount++;
            }
        }
    }
}
