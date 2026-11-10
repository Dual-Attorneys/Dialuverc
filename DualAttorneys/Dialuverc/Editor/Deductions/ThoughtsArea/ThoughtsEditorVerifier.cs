using Dialuverc.Editor.Base.Extensions;
using Dialuverc.Editor.Base.Localization;
using Dialuverc.Editor.Base.Verifier;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;
using DualAttorneys.Dialuverc.Editor.Deductions.ThoughtsArea;

namespace DualAttorneys.Dialuverc.Editor.Deductions.Thoughts
{
    internal static class ThoughtsEditorVerifier
    {
        public static IReadOnlyList<Problem> Run(AddEditScratchpads<ThoughtsEditorFields> scratchpads, ThoughtGuid? selectionGuid, IList<EditorThought> thoughts, IReadOnlyLocalizationStore localizationStore)
        {
            List<Problem> problems = new List<Problem>();

            if (scratchpads.AddScratchpad is not null)
            {
                string nameKey = scratchpads.AddScratchpad.NameKey;

                if (localizationStore.TryGetValue(nameKey, out string? duplicateNameKey))
                    VerifyPossibleKeyDuplication(nameKey,
                        t => t.RuntimeThought.NameKey == nameKey,
                        thoughts, problems);

                string descriptionKey = scratchpads.AddScratchpad.DescriptionKey;

                if (localizationStore.TryGetValue(descriptionKey, out string? duplicateDescriptionKey))
                    VerifyPossibleKeyDuplication(descriptionKey,
                        t => t.RuntimeThought.DescriptionKey == descriptionKey,
                        thoughts, problems);
            }

            // Check whether the match's Guid is the same as the thought we're editing.
            // If that's the case, there's no actual duplicated keys.
            if (scratchpads.EditScratchpad is not null)
            {
                string nameKey = scratchpads.EditScratchpad.NameKey;

                if (localizationStore.TryGetValue(nameKey, out string? duplicateNameKey))
                    VerifyPossibleKeyDuplication(nameKey,
                        t => t.RuntimeThought.NameKey == nameKey && t.RuntimeThought.Guid != selectionGuid,
                        thoughts, problems);

                string descriptionKey = scratchpads.EditScratchpad.DescriptionKey;

                if (localizationStore.TryGetValue(descriptionKey, out string? duplicateDescriptionKey))
                    VerifyPossibleKeyDuplication(descriptionKey,
                        t => t.RuntimeThought.DescriptionKey == descriptionKey && t.RuntimeThought.Guid != selectionGuid,
                        thoughts, problems);
            }

            return problems;
        }

        static void VerifyPossibleKeyDuplication(string keyToVerify, Predicate<EditorThought> match, IList<EditorThought> thoughts, List<Problem> problemsList)
        {
            int indexOfThought = thoughts.FindIndex(match);

            // Allow re-defining a key when no Thought is actually using it.
            if (indexOfThought < 0)
                return;

            problemsList.Add(new Problem(ProblemSeverity.Error, $"Key \"{keyToVerify}\" is already defined for thought at index {indexOfThought}."));
        }
    }
}
