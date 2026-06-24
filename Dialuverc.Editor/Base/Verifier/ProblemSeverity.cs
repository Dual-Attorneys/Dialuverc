namespace Dialuverc.Editor.Base.Verifier
{
    /// <summary>
    /// How severe a <see cref="Problem"/> is.
    /// </summary>
    public enum ProblemSeverity
    {
        /// <summary>
        /// An error occurred and verification couldn't be completed.
        /// </summary>
        Failed,

        /// <summary>
        /// A suggestion that can safely be ignored.
        /// </summary>
        Negligible,

        /// <summary>
        /// A possible mistake.<br/>
        /// Should be fixed if possible, but isn't a dealbreaker.
        /// </summary>
        Warning,

        /// <summary>
        /// A must-fix.
        /// </summary>
        Error,
    }
}
