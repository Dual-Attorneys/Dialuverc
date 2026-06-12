namespace Dialuverc.Editor.Base
{
    /// <summary>
    /// Allows tracking of one or multiple changes which can all be applied together.
    /// </summary>
    // Heavily inspired by osu-lazer's TransactionalCommitComponent.
    public abstract class TransactionalObject
    {
        int _changesStarted;

        /// <summary>
        /// Whether any changes were started but not yet completed.
        /// </summary>
        public bool TransactionPending => _changesStarted > 0;

        public virtual void BeginChange()
        {
            _changesStarted++;
        }

        /// <summary>
        /// Completes the outermost transaction.
        /// <para>If it's the last transaction, all changes are committed.</para>
        /// </summary>
        public virtual void EndChange()
        {
            if (_changesStarted == 0)
                throw new InvalidOperationException($"Can't call {nameof(EndChange)} without calling {nameof(BeginChange)} first.");

            _changesStarted--;

            if (_changesStarted == 0)
                Commit();
        }

        /// <summary>
        /// Forces a <see cref="Commit"/> without starting a transaction.
        /// <para>Can't be used while a transaction is pending.</para>
        /// </summary>
        public void ForceCommit()
        {
            if (_changesStarted > 0)
                return;

            Commit();
        }

        protected abstract void Commit();
    }
}
