namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// Represents an object which can be exported (i.e. serialized).
    /// </summary>
    public interface IExportable
    {
        /// <summary>
        /// A path relative to the folder this <see cref="IExportable"/> is being saved in.<br/>
        /// Can be used to group multiple <see cref="IExportable"/>s under the same subfolder.
        /// </summary>
        public string ExportPath { get; }

        public void SerializeForExport(Stream stream);
    }
}
