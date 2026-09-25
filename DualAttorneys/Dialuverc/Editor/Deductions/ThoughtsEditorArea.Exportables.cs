using System.Collections.Immutable;
using System.Text.Json;
using Dialuverc.Editor.Base.IO;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    public partial class ThoughtsEditorArea
    {
        private class EditorThoughtsImportable : JsonImportable<ImmutableList<EditorThought>>
        {
            public override string ExportPath => $"{nameof(ThoughtsEditorArea)}_Thoughts.json";

            readonly ThoughtsEditorArea _area;

            public EditorThoughtsImportable(ThoughtsEditorArea area, JsonSerializerOptions jsonOptions) : base(jsonOptions)
            {
                _area = area;
            }

            public override ImmutableList<EditorThought> GetToExport() => _area._thoughts;

            public override void OnImported(ImmutableList<EditorThought>? result)
            {
                // TODO: This could need better error handling.
                if (result is null)
                    result = ImmutableList<EditorThought>.Empty;

                // TODO: Some thinking has to go into this.
                // If we didn't do anything before importing, we may not want to allow Undo.
                // Possibly we may want importing to clear the history.
                // For now, the safest option is to just handle it as any other change.
                _area.BeginChange();

                _area._thoughts = result;

                _area.EndChange();
            }
        }
    }
}
