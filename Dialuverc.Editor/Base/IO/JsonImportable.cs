using System.Text.Json;

namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// An <see cref="IImportable"/> which supports serializing/deserializing to/from Json.
    /// </summary>
    /// <typeparam name="T">The type to import/export. May need a <see cref="System.Text.Json.Serialization.JsonConverter"/>.</typeparam>
    public abstract class JsonImportable<T> : IImportable
    {
        public abstract string ExportPath { get; }

        JsonSerializerOptions _jsonOptions;

        public JsonImportable(JsonSerializerOptions jsonOptions)
        {
            ArgumentNullException.ThrowIfNull(jsonOptions);

            _jsonOptions = jsonOptions;
        }

        public void DeserializeForImport(Stream stream)
        {
            T? result = JsonSerializer.Deserialize<T>(stream, _jsonOptions);

            OnImported(result);
        }

        public void SerializeForExport(Stream stream) => JsonSerializer.Serialize(stream, GetToExport(), _jsonOptions);

        /// <summary>
        /// Retrieves the instance of <typeparamref name="T"/> to export.
        /// </summary>
        public abstract T GetToExport();

        /// <summary>
        /// Called once deserialization is completed.
        /// </summary>
        public abstract void OnImported(T? result);
    }
}
