namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// Represents an object which can be exported and eventually restored from a serialized format.
    /// </summary>
    public interface IExportable
    {
        /// <summary>
        /// File path used for exporting/importing. Can be used to group multiple files under the same folder.
        /// </summary>
        public string ExportPath { get; }

        public void SerializeForExport(Stream stream);
        
        public void DeserializeForImport(Stream stream);
    }
}
