namespace Dialuverc.Editor.Base.Localization
{
    /// <summary>
    /// Denotes an object having a collection of localizable keys representable in a <see href="https://en.wikipedia.org/wiki/Flat-file_database">flat-file</see>.
    /// </summary>
    public interface IHasLocalizableKeysFlatFile
    {
        // No duplicates should exist in the same "namespace".
        public IReadOnlySet<string> GetLocalizableKeysFlat();
    }
}
