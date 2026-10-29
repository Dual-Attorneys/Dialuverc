using System.Collections.Specialized;

namespace Dialuverc.Editor.Base.Localization
{
    /// <summary>
    /// Provides a read-only view of a <see cref="LocalizationStore"/>, only allowing retrieval of localized values.
    /// </summary>
    public interface IReadOnlyLocalizationStore : INotifyCollectionChanged
    {
        public bool TryGetValue(string key, out string? value);
    }
}
