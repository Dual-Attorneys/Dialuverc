using Dialuverc.Deductions;

namespace Dialuverc.Editor.Deductions
{
    /// <summary>
    /// Defines a set of criteria against which a <see cref="Thought"/> can be tested.
    /// </summary>
    public class ThoughtsFilter
    {
        public CharacterSides SideFilter { get; set; } = CharacterSides.Any;

        /// <summary>
        /// An always-case-insensitive string that must be contained in <see cref="Thought.NameKey"/> to pass.
        /// </summary>
        public string NameKeyFilter { get; set; } = string.Empty;

        /// <summary>
        /// Whether passing the filter requires an exact <see cref="CharacterSides"/> match.
        /// <para>When <see langword="false"/>, <see cref="CharacterSides.Any"/> passes for all sides.</para>
        /// </summary>
        public bool Strict { get; set; } = false;

        bool PassesSidesFilter(CharacterSides side)
        {
            if (SideFilter == side)
                return true;

            if (!Strict && 
                (SideFilter == CharacterSides.Any || side == CharacterSides.Any))
                return true;

            return false;
        }

        bool PassesNameKeyFilter(string nameKey)
        {
            if (nameKey is null)
                throw new ArgumentNullException(nameof(nameKey), $"A {nameof(Thought)} can't have a null name key");

            if (string.IsNullOrWhiteSpace(NameKeyFilter))
                return true;

            return nameKey.Contains(NameKeyFilter, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Whether the <paramref name="thought"/> passes all current filters.
        /// </summary>
        public bool PassesAll(Thought thought) => PassesSidesFilter(thought.Side) && PassesNameKeyFilter(thought.NameKey);
    }
}
