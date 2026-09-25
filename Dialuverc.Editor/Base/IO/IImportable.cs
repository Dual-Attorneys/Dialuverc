namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// Represents an object which can be imported (i.e. deserialized).<br/>
    /// Implies <see cref="IExportable"/> as the object has to be exported at least once for there to be something to import.
    /// </summary>
    // Note: Intended usage of this *does not* include importing arbitrary files. As a consequence, path used is dependent on IExportable.
    public interface IImportable : IExportable
    {
        public void DeserializeForImport(Stream stream);
    }
}
