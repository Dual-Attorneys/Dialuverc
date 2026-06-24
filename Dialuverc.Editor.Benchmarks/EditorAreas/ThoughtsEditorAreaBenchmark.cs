using BenchmarkDotNet.Attributes;
using Dialuverc.Deductions;
using Dialuverc.Editor.Base;
using Dialuverc.Editor.Deductions;

namespace Dialuverc.Editor.Benchmarks.EditorAreas
{
    [MemoryDiagnoser]
    public class ThoughtsEditorAreaBenchmark
    {
        // This value was agreed upon as it's probably many more than will be made in practice.
        const int _baseAmountOfThoughts = 200;

        ThoughtsEditorArea _area;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _area = new ThoughtsEditorArea();

            for (int i = 0; i < _baseAmountOfThoughts; i++)
            {
                _area.AddThought(
                    $"Thought number {i}. This is the name.",
                    $"Thought number {i}'s description. Longer string so it's more realistic. Still short so here's more text. This should be more than enough.",
                    CharacterSides.Any);
            }
        }

        [Benchmark]
        public void UndoThenRedo()
        {
            _area.RestorePreviousState(RestoreDirection.Previous);

            _area.RestorePreviousState(RestoreDirection.Next);
        }

        int _stateSerializationCounter = 0;

        // All list operations in this area are similar.
        // This one is the easiest to use for benchmarking and still triggers a full state save.
        [Benchmark]
        public void StateSave()
        {
            Guid guidToEdit = _area.Thoughts[0].Guid;

            // Saving still happens even if the new state is identical to the current one.
            _area.EditThought(guidToEdit, "New thought name for the edit.", $"New thought description. Some meaningless text here to add some length. Added on iteration {_stateSerializationCounter}", CharacterSides.Any);

            _stateSerializationCounter++;
        }

        // Making enough Edits to make Verify have a significantly higher impact than it would with no problems found
        // would likely take up most of the time and memory spent in the benchmark.
        // 0-problem-verifies are the most common case and having more than a few issues is not realistic anyway.
        [Benchmark]
        public void Verify()
        {
            _area.Verify();
        }
    }
}
