using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Dialuverc.Editor.Base.Localization
{
    /// <summary>
    /// Provides a read-only view of a <see cref="LocalizationStore"/>, only allowing retrieval of localized values.
    /// </summary>
    public interface IReadOnlyLocalizationStore : INotifyCollectionChanged
    {
        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value);
    }
}
