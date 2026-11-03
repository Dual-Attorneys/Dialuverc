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

        void ThrowIfLocaleChoiceIsInvalid()
        {
            // Empty string represents invariant culture (from Microsoft docs).
            if (_localeChoice.Value is null)
                throw new InvalidOperationException("Cannot add a localization with null locale choice");
        }
    }
}
