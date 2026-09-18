namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// Determines which target to export for.
    /// </summary>
    public enum ExportTarget
    {
        /// <summary>
        /// Keep all information. Result should be usable to restore this same object.
        /// </summary>
        Editor,

        /// <summary>
        /// Strip all information not necessary to represent this object in the game.
        /// </summary>
        Game,
    }
}