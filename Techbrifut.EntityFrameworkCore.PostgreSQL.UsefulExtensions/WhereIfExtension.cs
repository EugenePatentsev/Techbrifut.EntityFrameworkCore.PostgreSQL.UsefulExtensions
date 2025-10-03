using System.Linq.Expressions;

namespace Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

public static class WhereIfExtension
{
    public static IQueryable<TSource> WhereIf<TSource>(
        this IQueryable<TSource> source,
        bool condition,
        Expression<Func<TSource, bool>> predicate
    )
    {
        return condition ? source.Where(predicate) : source;
    }

    public static IQueryable<TSource> WhereIfIsNotNullOrEmpty<TSource>(
        this IQueryable<TSource> source,
        string? value,
        Expression<Func<TSource, bool>> predicate
    )
    {
        return source.WhereIf(!string.IsNullOrEmpty(value), predicate);
    }

    public static IQueryable<TSource> WhereIfIsNotNullOrWhiteSpace<TSource>(
        this IQueryable<TSource> source,
        string? value,
        Expression<Func<TSource, bool>> predicate
    )
    {
        return source.WhereIf(!string.IsNullOrWhiteSpace(value), predicate);
    }

    public static IQueryable<TSource> WhereIfIsNotNullOrEmpty<TSource>(
        this IQueryable<TSource> source,
        Guid? value,
        Expression<Func<TSource, bool>> predicate
    )
    {
        return source.WhereIf(value != null && value != Guid.Empty, predicate);
    }

    public static IQueryable<TSource> OrWhereIf<TSource>(
        this IQueryable<TSource> source,
        bool condition,
        Expression<Func<TSource, bool>> predicate
    )
    {
        return condition ? source.OrWhere(predicate) : source;
    }

    public static IQueryable<TSource> OrWhereIfIsNotNullOrEmpty<TSource>(
        this IQueryable<TSource> source,
        string? value,
        Expression<Func<TSource, bool>> predicate
    )
    {
        return source.OrWhereIf(!string.IsNullOrEmpty(value), predicate);
    }

    public static IQueryable<TSource> OrWhereIfIsNotNullOrWhiteSpace<TSource>(
        this IQueryable<TSource> source,
        string? value,
        Expression<Func<TSource, bool>> predicate
    )
    {
        return source.OrWhereIf(!string.IsNullOrWhiteSpace(value), predicate);
    }

    public static IQueryable<TSource> OrWhereIfIsNotNullOrEmpty<TSource>(
        this IQueryable<TSource> source,
        Guid? value,
        Expression<Func<TSource, bool>> predicate
    )
    {
        return source.OrWhereIf(value != null && value != Guid.Empty, predicate);
    }
}