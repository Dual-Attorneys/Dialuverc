using System.Collections.Immutable;
using System.Text.Json;
using Dialuverc.Editor.Base.IO;
using DualAttorneys.Dialuverc.Editor.Deductions;

// TODO: Find better naming?
public class ThoughtsEditorExportable : IExportable
{
    public string ExportPath => $"{nameof(ThoughtsEditorArea)}.Editor";

    Func<IReadOnlyList<EditorThought>> _getToExport;
    Action<ImmutableList<EditorThought>> _setImported;

    JsonSerializerOptions _jsonOptions;

    public ThoughtsEditorExportable(Func<IReadOnlyList<EditorThought>> getToExport,
        Action<ImmutableList<EditorThought>> setImported,
        JsonSerializerOptions jsonOptions)
    {
        ArgumentNullException.ThrowIfNull(setImported);
        ArgumentNullException.ThrowIfNull(getToExport);
        ArgumentNullException.ThrowIfNull(jsonOptions);

        _getToExport = getToExport;
        _setImported = setImported;
        _jsonOptions = jsonOptions;
    }

    public void DeserializeForImport(Stream stream)
    {
        ImmutableList<EditorThought>? result =
            JsonSerializer.Deserialize<ImmutableList<EditorThought>>(stream, _jsonOptions);

        // TODO: Proper error handling instead of just passing an empty list.
        if (result is null)
            result = ImmutableList<EditorThought>.Empty;

        _setImported.Invoke(result);
    }

    public void SerializeForExport(Stream stream)
    {
        IReadOnlyList<EditorThought> toExport = _getToExport.Invoke();

        JsonSerializer.Serialize(stream, toExport, _jsonOptions);
    }
}