using Dialuverc.Editor.Base.Localization;
using Dialuverc.Editor.Base.Observability;

namespace Dialuverc.Editor.Tests.Base
{
    public class LocalizationStoreTests
    {
        ObservableValue<string> _localeChoice;
        LocalizationStore _localizationStore;

        [SetUp]
        public void SetUp()
        {
            _localeChoice = new ObservableValue<string>("it-IT");
            _localizationStore = new LocalizationStore(_localeChoice, "namespace");
        }

        [Test]
        public void AddKeyValuePairEventFiresCorrectly()
        {
            _localizationStore.CollectionChanged += (sender, args) =>
            {
                Assert.That(sender, Is.EqualTo(_localizationStore));
                Assert.That(args.NewItems, Has.Count.EqualTo(1));
                Assert.That(args.OldItems, Is.Null);
                Assert.That(args.ChangeType, Is.EqualTo(DictionaryChangeType.Add));

                var item = args.NewItems.First();

                Assert.That(item.Key, Is.EqualTo(_localeChoice.Value));
                Assert.That(item.Value, Has.Count.EqualTo(1));

                var innerItem = item.Value.First();

                Assert.That(innerItem.Key, Is.EqualTo("key"));
                Assert.That(innerItem.Value, Is.EqualTo("value"));
            };

            _localizationStore.AddOrSetValue("key", "value");
        }

        [Test]
        public void ReplaceValueEventFiresCorrectly()
        {
            _localizationStore.AddOrSetValue("key", "oldValue");

            _localizationStore.CollectionChanged += (sender, args) =>
            {
                Assert.That(sender, Is.EqualTo(_localizationStore));
                Assert.That(args.NewItems, Has.Count.EqualTo(1));
                Assert.That(args.OldItems, Has.Count.EqualTo(1));
                Assert.That(args.ChangeType, Is.EqualTo(DictionaryChangeType.Replace));

                var newItem = args.NewItems.First();

                Assert.That(newItem.Key, Is.EqualTo(_localeChoice.Value));
                Assert.That(newItem.Value, Has.Count.EqualTo(1));

                var innerNewItem = newItem.Value.First();

                Assert.That(innerNewItem.Key, Is.EqualTo("key"));
                Assert.That(innerNewItem.Value, Is.EqualTo("newValue"));

                var oldItem = args.OldItems.First();

                Assert.That(oldItem.Key, Is.EqualTo(_localeChoice.Value));
                Assert.That(oldItem.Value, Has.Count.EqualTo(1));

                var innerOldItem = oldItem.Value.First();

                Assert.That(innerOldItem.Key, Is.EqualTo("key"));
                Assert.That(innerOldItem.Value, Is.EqualTo("oldValue"));
            };

            _localizationStore.AddOrSetValue("key", "newValue");
        }

        [Test]
        public void RemoveKeyEventFiresCorrectly()
        {
            _localizationStore.AddOrSetValue("key", "it-IT");
            _localizationStore.AddOrSetValue("differentKey", "it-IT");

            _localeChoice.ChangeValue("en-US");
            _localizationStore.AddOrSetValue("key", "en-US");

            _localeChoice.ChangeValue("en-BR");
            _localizationStore.AddOrSetValue("differentKey", "en-BR");

            _localizationStore.CollectionChanged += (sender, args) =>
            {
                Assert.That(sender, Is.EqualTo(_localizationStore));
                Assert.That(args.NewItems, Is.Null);
                Assert.That(args.OldItems, Has.Count.EqualTo(2));
                Assert.That(args.ChangeType, Is.EqualTo(DictionaryChangeType.Remove));

                var ITRemoval = args.OldItems.FirstOrDefault(i => i.Key == "it-IT");
                var USRemoval = args.OldItems.FirstOrDefault(i => i.Key == "en-US");

                // KVP is a struct, which forces us to check whether the values INSIDE are default or valid.
                // No need to check the "different key" case individually, as checking for Count == 2 and both keys being valid
                // result in the same conclusion.
                Assert.That(ITRemoval.Key, Is.Not.Null);
                Assert.That(USRemoval.Key, Is.Not.Null);

                Assert.That(ITRemoval.Value, Has.Count.EqualTo(1));
                Assert.That(USRemoval.Value, Has.Count.EqualTo(1));

                var innerITRemoval = ITRemoval.Value.First();
                var innerUSRemoval = USRemoval.Value.First();

                Assert.That(innerITRemoval.Key, Is.EqualTo("key"));
                Assert.That(innerITRemoval.Value, Is.EqualTo("it-IT"));

                Assert.That(innerUSRemoval.Key, Is.EqualTo("key"));
                Assert.That(innerUSRemoval.Value, Is.EqualTo("en-US"));
            };

            _localizationStore.RemoveKeyForAllLocales("key");
        }
    }
}
