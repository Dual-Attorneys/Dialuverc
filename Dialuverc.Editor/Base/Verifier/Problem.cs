namespace Dialuverc.Editor.Base.Verifier
{
    /// <summary>
    /// Something that has to be brought to the attention of the user.
    /// </summary>
    public class Problem
    {
        public readonly ProblemSeverity Severity;

        public readonly string Description = "No description.";

        public Problem(ProblemSeverity severity, string description)
        {
            Severity = severity;

            if (!string.IsNullOrWhiteSpace(description))
                Description = description;
        }
    }
}
