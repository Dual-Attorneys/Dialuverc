namespace DualAttorneys.Dialuverc.Deductions
{
    public record class Thought
    {
        public ThoughtGuid Guid { get; init; }

        public string NameKey { get; init; }
        public string DescriptionKey { get; init; }

        /// <summary>
        /// Represents which character this thought can be given to.
        /// </summary>
        public CharacterSide Side { get; init; }

        public Thought(ThoughtGuid guid, string nameKey, string descriptionKey, CharacterSide side)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(nameKey, nameof(nameKey));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(descriptionKey, nameof(descriptionKey));

            Guid = guid;

            NameKey = nameKey;
            DescriptionKey = descriptionKey;

            Side = side;
        }
    }
}
