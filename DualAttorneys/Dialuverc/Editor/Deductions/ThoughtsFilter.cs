using Dialuverc.Editor.Base.Localization;
using DualAttorneys.Dialuverc.Deductions;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    /// <summary>
    /// Defines a set of criteria against which a <see cref="Thought"/> can be tested.
    /// </summary>
    public class ThoughtsFilter
    {
        readonly IReadOnlyLocalizationStore _localizationStore;

        public CharacterSide SideFilter { get; set; } = CharacterSide.Any;

        /// <summary>
        /// An always-case-insensitive string that must be contained in a <see cref="Thought"/>'s localized name OR description to pass.
        /// </summary>
        public string TextFilter { get; set; } = string.Empty;

        /// <summary>
        /// Whether passing the filter requires an exact <see cref="CharacterSide"/> match.
        /// <para>When <see langword="false"/>, <see cref="CharacterSide.Any"/> passes for all sides.</para>
        /// </summary>
        public bool Strict { get; set; } = false;

        public ThoughtsFilter(IReadOnlyLocalizationStore localizationStore)
        {
            ArgumentNullException.ThrowIfNull(localizationStore, nameof(localizationStore));

            _localizationStore = localizationStore;
        }

        bool PassesSidesFilter(CharacterSide side)
        {
            if (SideFilter == side)
                return true;

            if (!Strict &&
                (SideFilter == CharacterSide.Any || side == CharacterSide.Any))
                return true;

            return false;
        }

        bool PassesTextFilter(string localizationKey)
        {
            // We probably do not want to throw on null if this is UI-facing.
            if (string.IsNullOrWhiteSpace(localizationKey))
                return true;

            if (string.IsNullOrWhiteSpace(TextFilter))
                return true;

            if (!_localizationStore.TryGetValue(localizationKey, out string? foundValue))
                return true;

            return foundValue.Contains(TextFilter, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Whether the <paramref name="thought"/> passes all current filters.
        /// </summary>
        public bool PassesAll(Thought thought) =>
            PassesSidesFilter(thought.Side) &&
            (PassesTextFilter(thought.NameKey) || (PassesTextFilter(thought.DescriptionKey)));
    }
}
