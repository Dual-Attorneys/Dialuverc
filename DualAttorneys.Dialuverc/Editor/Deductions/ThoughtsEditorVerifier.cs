using Dialuverc.Editor.Base.Verifier;
using DualAttorneys.Dialuverc.Deductions;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    internal static class ThoughtsEditorVerifier
    {
        const string _unformattedDuplicateMessage = "Duplicate {0} '{1}' at indices {2}.";

        public static IReadOnlyList<Problem> Run(IReadOnlyList<Thought> thoughts)
        {
            List<Problem> allProblems = new List<Problem>();

            DuplicatesInfo[] nameDuplicates = GetDuplicates(thoughts, t => t.NameKey);
            DuplicatesInfo[] descriptionDuplicates = GetDuplicates(thoughts, t => t.DescriptionKey);

            // Names may be used as a human-readable way to get to a GUID.
            // Descriptions can "safely" be duplicate.
            AddDuplicationProblems(allProblems, nameDuplicates, nameof(Thought.NameKey), ProblemSeverity.Error);
            AddDuplicationProblems(allProblems, descriptionDuplicates, nameof(Thought.DescriptionKey), ProblemSeverity.Warning);

            return allProblems;
        }

        static DuplicatesInfo[] GetDuplicates(IReadOnlyList<Thought> thoughts, Func<Thought, string> criteria)
        {
            DuplicatesInfo[] duplicatesInfo =
                thoughts.Select((t, i) => new ValueTuple<Thought, int>(t, i))
                .GroupBy(a => criteria(a.Item1))
                .Where(g => g.Count() > 1)
                .Select(g => new DuplicatesInfo(
                    g.Key,
                    g.Select(e => e.Item2).ToArray()))
                .ToArray();

            return duplicatesInfo;
        }

        static void AddDuplicationProblems(List<Problem> problemsCollection, DuplicatesInfo[] duplicatesInfoCollection, string propertyName, ProblemSeverity severity)
        {
            foreach (DuplicatesInfo duplicatesInfo in duplicatesInfoCollection)
            {
                Problem problem = new Problem(severity,
                    string.Format(_unformattedDuplicateMessage, propertyName, duplicatesInfo.Value, string.Join(", ", duplicatesInfo.Indices)));

                problemsCollection.Add(problem);
            }
        }

        /// <summary>
        /// Contains information on a <see cref="Value"/> that is duplicated across multiple <see cref="Thought"/>s in the same collection.
        /// </summary>
        readonly struct DuplicatesInfo
        {
            /// <summary>
            /// The duplicate value.
            /// </summary>
            public readonly string Value;

            /// <summary>
            /// The indices which contain the duplicate values.
            /// </summary>
            public readonly int[] Indices;

            public DuplicatesInfo(string value, int[] indices)
            {
                Value = value;
                Indices = indices;
            }
        }
    }
}