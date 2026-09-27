using System.Text.Json;

namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// An <see cref="IImportable"/> which supports serializing/deserializing to/from Json.
    /// </summary>
    /// <typeparam name="T">The type to import/export. May need a <see cref="System.Text.Json.Serialization.JsonConverter"/>.</typeparam>
    public class JsonImportable<T> : IImportable
    {
        public string ExportPath { get; private set; }

        readonly Func<T> _getter;
        readonly Action<T?> _setter;

        readonly JsonSerializerOptions _jsonOptions;

        public JsonImportable(string exportPath, Func<T> getter, Action<T?> setter, JsonSerializerOptions jsonOptions)
        {
            ArgumentNullException.ThrowIfNull(exportPath);
            ArgumentNullException.ThrowIfNull(getter);
            ArgumentNullException.ThrowIfNull(setter);
            ArgumentNullException.ThrowIfNull(jsonOptions);

            ExportPath = exportPath;
            _getter = getter;
            _setter = setter;
            _jsonOptions = jsonOptions;
        }

        public void DeserializeForImport(Stream stream)
        {
            T? result = JsonSerializer.Deserialize<T>(stream, _jsonOptions);

            _setter.Invoke(result);
        }

        public void SerializeForExport(Stream stream) => JsonSerializer.Serialize(stream, _getter.Invoke(), _jsonOptions);
    }
}
