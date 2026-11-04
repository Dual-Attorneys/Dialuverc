namespace Dialuverc.Editor.Base.Observability
{
    public class DictionaryChangeEventArgs<K, T> where K : notnull
    {
        public readonly DictionaryChangeType ChangeType;

        public readonly ICollection<KeyValuePair<K, T>>? NewItems;
        public readonly ICollection<KeyValuePair<K, T>>? OldItems;

        public DictionaryChangeEventArgs(DictionaryChangeType changeType, ICollection<KeyValuePair<K, T>> involvedItems)
        {
            ChangeType = changeType;

            switch (ChangeType)
            {
                case DictionaryChangeType.Add:

                    NewItems = involvedItems;
                    break;

                case DictionaryChangeType.Remove:

                    OldItems = involvedItems;
                    break;

                default:
                    throw new ArgumentException($"{nameof(changeType)} can only be {DictionaryChangeType.Add} or {DictionaryChangeType.Remove}");
            }
        }

        public DictionaryChangeEventArgs(DictionaryChangeType changeType, KeyValuePair<K, T> involvedItem)
            : this(changeType, new KeyValuePair<K, T>[] { involvedItem }) { }

        public DictionaryChangeEventArgs(KeyValuePair<K, T> newItem, KeyValuePair<K, T> oldItem)
        {
            ChangeType = DictionaryChangeType.Replace;

            NewItems = new KeyValuePair<K, T>[] { newItem };
            OldItems = new KeyValuePair<K, T>[] { oldItem };
        }
    }

    public enum DictionaryChangeType
    {
        Add,
        Remove,
        Replace,
    }
}
