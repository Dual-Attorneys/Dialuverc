using DualAttorneys.Dialuverc.Deductions;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    public static class EditorThoughtExtensions
    {
        /// <summary>
        /// Checks whether this <see cref="Thought"/> has the same values as another, ignoring <see cref="Thought.Guid"/>.
        /// </summary>
        public static bool HasSameValues(this Thought thought, Thought? other)
            => other is not null && HasSameValues(thought, other.NameKey, other.DescriptionKey, other.Side);

        public static bool HasSameValues(this Thought thought, string nameKey, string descriptionKey, CharacterSides side)
            => thought.NameKey == nameKey && thought.DescriptionKey == descriptionKey && thought.Side == side;
    }
}
