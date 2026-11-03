using Dialuverc.Editor.Base.Observability;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Dialuverc.Editor.Base.Localization
{
    /// <summary>
    /// A scoped store containing string key-value pairs keyed by locale.
    /// </summary>
    public class LocalizationStore : IReadOnlyLocalizationStore
    {
        readonly IReadOnlyObservableValue<string> _localeChoice;

        public string KeysNamespace { get; private set; }

        public event EventHandler<DictionaryChangeEventArgs<string, ICollection<KeyValuePair<string, string?>>>>? CollectionChanged;

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

            DictionaryChangeEventArgs<string, ICollection<KeyValuePair<string, string?>>> eventArgs;

            if (!Lookup.TryGetValue(_localeChoice.Value!, out ImmutableDictionary<string, string>? lookupForKey))
            {
                lookupForKey = ImmutableDictionary<string, string>.Empty.Add(key, value);

                eventArgs = new DictionaryChangeEventArgs<string, ICollection<KeyValuePair<string, string?>>>
                (
                    DictionaryChangeType.Add,
                    new KeyValuePair<string, ICollection<KeyValuePair<string, string?>>>
                    (
                        _localeChoice.Value!,
                        new KeyValuePair<string, string?>[]
                        {
                            new KeyValuePair<string, string?>(key, value)
                        }
                    )
                );
            }
            else
            {
                lookupForKey.TryGetValue(key, out string? previousValue);

                if (value == previousValue)
                    return;

                eventArgs = new DictionaryChangeEventArgs<string, ICollection<KeyValuePair<string, string?>>>
                (
                    new KeyValuePair<string, ICollection<KeyValuePair<string, string?>>>
                    (
                        _localeChoice.Value!,
                        new KeyValuePair<string, string?>[]
                        {
                            new KeyValuePair<string, string?>(key, value)
                        }
                    ),
                    new KeyValuePair<string, ICollection<KeyValuePair<string, string?>>>
                    (
                        _localeChoice.Value!,
                        new KeyValuePair<string, string?>[]
                        {
                            new KeyValuePair<string, string?>(key, previousValue)
                        }
                    )
                );

                lookupForKey = lookupForKey.SetItem(key, value);
            }

            Lookup = Lookup.SetItem(_localeChoice.Value!, lookupForKey);

            CollectionChanged?.Invoke(this, eventArgs);
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
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

        // We're assuming a key's definition only comes from at most one source.
        // As a consequence, removing that source should wipe the key and any associated values across all locales.
        public void RemoveKeyForAllLocales(string key)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            List<KeyValuePair<string, ICollection<KeyValuePair<string, string?>>>> removals = new();

            foreach (KeyValuePair<string, ImmutableDictionary<string, string>> innerLookup in Lookup)
            {
                ImmutableDictionary<string, string> innerLookupAfterRemove = innerLookup.Value.Remove(key);

                // Same instance is returned if Remove(key) fails, in which case we should do nothing.
                if (innerLookupAfterRemove == innerLookup.Value)
                    continue;

                Lookup = Lookup.SetItem(innerLookup.Key, innerLookupAfterRemove);

                removals.Add(new KeyValuePair<string, ICollection<KeyValuePair<string, string?>>>
                (
                    innerLookup.Key,
                    new KeyValuePair<string, string?>[]
                    {
                        // Should be safe to not null check here, as only reached if the Remove(key) above completes,
                        // meaning the key exists in this inner lookup.
                        new KeyValuePair<string, string?>(key, innerLookup.Value[key])
                    }
                ));
            }

            CollectionChanged?.Invoke(this, new DictionaryChangeEventArgs<string, ICollection<KeyValuePair<string, string?>>>
            (
                DictionaryChangeType.Remove,
                removals
            ));
        }

        void ThrowIfLocaleChoiceIsInvalid()
        {
            // Empty string represents invariant culture (from Microsoft docs).
            if (_localeChoice.Value is null)
                throw new InvalidOperationException("Cannot add a localization with null locale choice");
        }
    }
}
