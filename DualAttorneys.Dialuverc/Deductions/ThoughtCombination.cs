namespace DualAttorneys.Dialuverc.Deductions
{
    /// <summary>
    /// Represents an arrangement of <see cref="Thought"/>s over 3 slots where <see langword="null"/> represents an empty slot.
    /// </summary>
    public readonly struct ThoughtCombination : IEquatable<ThoughtCombination>
    {
        public readonly ThoughtGuid? SlotOne;
        public readonly ThoughtGuid? SlotTwo;
        public readonly ThoughtGuid? SlotThree;

        public ThoughtCombination(ThoughtGuid? slotOne, ThoughtGuid? slotTwo, ThoughtGuid? slotThree)
        {
            SlotOne = slotOne;
            SlotTwo = slotTwo;
            SlotThree = slotThree;
        }

        public bool Equals(ThoughtCombination other) => SlotOne == other.SlotOne && SlotTwo == other.SlotTwo && SlotThree == other.SlotThree;
        public override bool Equals(object? obj) => obj is ThoughtCombination otherCombination && Equals(otherCombination);

        public static bool operator ==(ThoughtCombination left, ThoughtCombination right) => left.Equals(right);
        public static bool operator !=(ThoughtCombination left, ThoughtCombination right) => !left.Equals(right);

        public override int GetHashCode() => HashCode.Combine(SlotOne, SlotTwo, SlotThree);
    }
}
