namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// Represents an object which can be exported (i.e. serialized).
    /// </summary>
    public interface IExportable
    {
        /// <summary>
        /// A path relative to the storage this <see cref="IExportable"/> is being exported in.<br/>
        /// Can be used to group multiple <see cref="IExportable"/>s under the same sub-storage.
        /// </summary>
        public string RelativePath { get; }

        public void SerializeForExport(Stream stream);
    }
}
