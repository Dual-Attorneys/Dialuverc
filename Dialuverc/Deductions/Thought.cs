namespace Dialuverc.Deductions
{
    public class Thought
    {
        public readonly Guid Guid;

        public readonly string NameKey;
        public readonly string DescriptionKey;

        /// <summary>
        /// Represents which character this thought can be given to.
        /// </summary>
        public readonly CharacterSides Side;

        public Thought(Guid guid, string nameKey, string descriptionKey, CharacterSides side)
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
