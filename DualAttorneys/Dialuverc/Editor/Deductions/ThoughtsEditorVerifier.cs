using Dialuverc.Editor.Base.Extensions;
using Dialuverc.Editor.Base.Localization;
using Dialuverc.Editor.Base.Verifier;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    internal static class ThoughtsEditorVerifier
    {
        public static IReadOnlyList<Problem> Run(AddEditScratchpads<ThoughtsEditorFields> scratchpads, IList<EditorThought> thoughts, IReadOnlyLocalizationStore localizationStore)
        {
            List<Problem> problems = new List<Problem>();

            if (scratchpads.AddScratchpad is not null)
            {
                if (localizationStore.TryGetValue(scratchpads.AddScratchpad.NameKey, out string? duplicateNameKey))
                    VerifyPossibleKeyDuplication(duplicateNameKey!, t => t.RuntimeThought.NameKey, thoughts, problems);

                if (localizationStore.TryGetValue(scratchpads.AddScratchpad.DescriptionKey, out string? duplicateDescriptionKey))
                    VerifyPossibleKeyDuplication(duplicateDescriptionKey!, t => t.RuntimeThought.DescriptionKey, thoughts, problems);
            }

            if (scratchpads.EditScratchpad is not null)
            {
                if (localizationStore.TryGetValue(scratchpads.EditScratchpad.NameKey, out string? duplicateNameKey))
                    VerifyPossibleKeyDuplication(duplicateNameKey!, t => t.RuntimeThought.NameKey, thoughts, problems);

                if (localizationStore.TryGetValue(scratchpads.EditScratchpad.DescriptionKey, out string? duplicateDescriptionKey))
                    VerifyPossibleKeyDuplication(duplicateDescriptionKey!, t => t.RuntimeThought.DescriptionKey, thoughts, problems);
            }

            return problems;
        }

        static void VerifyPossibleKeyDuplication(string keyToVerify, Func<EditorThought, string> getter, IList<EditorThought> thoughts,List<Problem> problemsList)
        {
            int indexOfThought = thoughts.FindIndex(t => getter(t) == keyToVerify);

            // Allow re-defining a key when no Thought is actually using it.
            if (indexOfThought < 0)
                return;

            problemsList.Add(new Problem(ProblemSeverity.Error, $"Key \"{keyToVerify}\" is already defined for thought at index {indexOfThought}."));
        }
    }
}
