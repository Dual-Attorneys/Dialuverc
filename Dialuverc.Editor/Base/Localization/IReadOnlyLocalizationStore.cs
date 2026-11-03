using System.Diagnostics.CodeAnalysis;
using Dialuverc.Editor.Base.Observability;

namespace Dialuverc.Editor.Base.Localization
{
    /// <summary>
    /// Provides a read-only view of a <see cref="LocalizationStore"/>, only allowing retrieval of localized values.
    /// </summary>
    public interface IReadOnlyLocalizationStore
    {
        /// <summary>
        /// Notifies consumers that the key-value pair they're using may be outdated (or no longer existing).
        /// </summary>
        public event EventHandler<DictionaryChangeEventArgs<string, ICollection<KeyValuePair<string, string?>>>>? CollectionChanged;

        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value);
    }
}
