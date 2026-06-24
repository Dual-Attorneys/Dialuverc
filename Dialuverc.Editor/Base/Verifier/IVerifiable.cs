namespace Dialuverc.Editor.Base.Verifier
{
    /// <summary>
    /// Identifies something that supports being analyzed for any <see cref="Problem"/>s.
    /// </summary>
    public interface IVerifiable
    {
        /// <summary>
        /// Runs a series of checks and returns any found <see cref="Problem"/>s.
        /// </summary>
        IReadOnlyList<Problem> Verify();
    }
}
