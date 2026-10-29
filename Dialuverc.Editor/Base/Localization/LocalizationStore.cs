using Dialuverc.Editor.Base.Observability;
using System.Collections.Immutable;

namespace Dialuverc.Editor.Base.Localization
{
    /// <summary>
    /// A scoped store containing string key-value pairs keyed by locale.
    /// </summary>
    public class LocalizationStore : IReadOnlyLocalizationStore
    {
        readonly IReadOnlyObservableValue<string> _localeChoice;

        public string KeysNamespace { get; private set; }

        // Keyed by locale and then by key.
        public ImmutableDictionary<string, ImmutableDictionary<string, string>> Lookup { get; set; }
            = ImmutableDictionary<string, ImmutableDictionary<string, string>>.Empty;

        public LocalizationStore(IReadOnlyObservableValue<string> localeChoice, string keysNamespace)
        {
            ArgumentNullException.ThrowIfNull(localeChoice, nameof(localeChoice));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(keysNamespace, nameof(keysNamespace));

            _localeChoice = localeChoice;
            KeysNamespace = keysNamespace;
        }

        public void AddOrSetValue(string key, string value)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(value, nameof(value));

            ThrowIfLocaleChoiceIsInvalid();

            if (!Lookup.TryGetValue(_localeChoice.Value!, out ImmutableDictionary<string, string>? lookupForKey))
                lookupForKey = ImmutableDictionary<string, string>.Empty.Add(key, value);
            else
                lookupForKey = lookupForKey.SetItem(key, value);

            Lookup = Lookup.SetItem(_localeChoice.Value!, lookupForKey);
        }

        public bool TryGetValue(string key, out string? value)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            ThrowIfLocaleChoiceIsInvalid();

            value = null;

            if (!Lookup.TryGetValue(_localeChoice.Value!, out ImmutableDictionary<string, string>? lookupForKey))
                return false;

            if (!lookupForKey.TryGetValue(key, out value))
                return false;

            return true;
        }

        void ThrowIfLocaleChoiceIsInvalid()
        {
            // Empty string represents invariant culture (from Microsoft docs).
            if (_localeChoice.Value is null)
                throw new InvalidOperationException("Cannot add a localization with null locale choice");
        }
    }
}
