namespace EventGenerator.Extensions;

public static class ListExstensions
{
    public static void SortBy<T, TKey>(this List<T> list, Func<T, TKey> selector, IComparer<TKey>? comparer = null)
    {
        if (comparer == null) comparer = Comparer<TKey>.Default;

        list.Sort((a, b) => comparer.Compare(selector(a), selector(b)));
    }
}
