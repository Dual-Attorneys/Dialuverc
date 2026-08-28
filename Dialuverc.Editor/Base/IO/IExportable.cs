namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// Represents an object which can be exported (both for editor and game consumption) and eventually restored from a serialized format.
    /// </summary>
    public interface IExportable
    {
        /// <summary>
        /// File name used for exporting/importing.
        /// </summary>
        public string ExportName { get; }

        // TODO: This should be split in a way that allows the "editor project" to be saved
        // separately from the output that will be used by the game.
        public string SerializeForExport();
    }
}
