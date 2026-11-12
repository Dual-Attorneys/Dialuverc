using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;

namespace DualAttorneys.Dialuverc.Editor.Deductions.Verify
{
    /// <summary>
    /// Represents an instance of key duplication.
    /// <para>The original key is assumed to be the one in the collection.</para>
    /// </summary>
    record class KeyDuplicationProblem
    {
        public required string Key { get; init; }

        /// <summary>
        /// Index of the item containing the original key in its collection.
        /// </summary>
        public int Index { get; init; }

        /// <summary>
        /// Name of the field containing the key.
        /// </summary>
        public string FieldName { get; init; } = "Unnamed field";

        /// <summary>
        /// Which scratchpad the duplication is blocking.
        /// </summary>
        public required AddEditEnum ScratchpadSelection { get; init; }
    }
}
