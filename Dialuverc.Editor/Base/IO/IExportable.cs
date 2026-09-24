namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// Represents an object which can be exported (i.e. serialized).
    /// </summary>
    public interface IExportable
    {
        public string ExportPath { get; }

        public void SerializeForExport(Stream stream);
    }
}
