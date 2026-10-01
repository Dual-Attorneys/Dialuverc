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
        public CharacterSides Side { get; init; }

        public Thought(ThoughtGuid guid, string nameKey, string descriptionKey, CharacterSides side)
        {
            if (nameKey is null)
                throw new ArgumentNullException(nameof(nameKey), "Name can't be null");

            if (descriptionKey is null)
                throw new ArgumentNullException(nameof(descriptionKey), "Description can't be null");

            Guid = guid;

            NameKey = nameKey;
            DescriptionKey = descriptionKey;

            Side = side;
        }
    }
}
