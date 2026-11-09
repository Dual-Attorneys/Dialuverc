using Dialuverc.Editor.Base.IO;

namespace Dialuverc.Editor.Base.Project
{
    /// <summary>
    /// <inheritdoc cref="IEditorTrackable"/>
    /// <para>Exposes one-way exporting capability for game consumption.</para>
    /// </summary>
    public interface IGameExportable : IEditorTrackable
    {
        /// <summary>
        /// Returns all <see cref="IExportable"/>s whose target is a game.
        /// </summary>
        public IEnumerable<IExportable> GetGameExportables();
    }
}
