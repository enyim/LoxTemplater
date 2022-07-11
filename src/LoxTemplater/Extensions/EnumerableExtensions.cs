namespace Enyim.LoxTempl;

public static class EnumerableExtensions
{
    public static bool None<TSource>(this IEnumerable<TSource> source)
        => !source.Any();

    public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => !source.Any(predicate);
}
