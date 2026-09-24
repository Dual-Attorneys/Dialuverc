namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// Represents an object which can be imported (i.e. deserialized).
    /// </summary>
    // Note: Intended usage of this *does not* include importing arbitrary files. As a consequence, path used is dependent on IExportable.
    // Being "importable" implies being exported at least once before.
    public interface IImportable : IExportable
    {
        public void DeserializeForImport(Stream stream);
    }
}
