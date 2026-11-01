namespace Dialuverc.Editor.Base.Extensions
{
    public static class IListExtensions
    {
        public static int FindIndex<T>(this IList<T> list, Predicate<T> match)
        {
            ArgumentNullException.ThrowIfNull(match, nameof(match));

            for (int i = 0; i < list.Count; i++)
            {
                if (match(list[i]))
                    return i;
            }

            return -1;
        }
    }
}
