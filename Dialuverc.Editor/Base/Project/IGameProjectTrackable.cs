using Dialuverc.Editor.Base.IO;

namespace Dialuverc.Editor.Base.Project
{
    /// <summary>
    /// <inheritdoc cref="IEditorProjectTrackable"/><br/>
    /// Exposes one-way exporting capability for game consumption.
    /// </summary>
    public interface IGameProjectTrackable : IEditorProjectTrackable
    {
        /// <summary>
        /// Returns all <see cref="IExportable"/>s whose target is a game.
        /// </summary>
        public IEnumerable<IExportable> GetGameExportables();
    }
}
