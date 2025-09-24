using System.Linq.Expressions;

namespace Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

/// <summary>
/// Utilities for composing IQueryable filters in a fluent, expression-safe way.
/// </summary>
public static class QueryableExtension
{
    /// <summary>
    /// Appends the given predicate to the most recent Where of the query using a logical OR.
    ///
    ///
    /// .
    /// </summary>
    /// <typeparam name="TSource">Element type of the sequence.</typeparam>
    /// <param name="source">The query to extend.</param>
    /// <param name="predicate">The condition to combine with OR.</param>
    /// <returns>
    /// A new query whose last Where predicate is combined with the supplied predicate via OR; or a query with a single Where if none existed.
    /// </returns>
    /// <exception cref="ArgumentNullException">source or predicate is null.</exception>
    /// <exception cref="InvalidOperationException">The existing Where uses more than one parameter.</exception>
    public static IQueryable<TSource> OrWhere<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        if (source.Expression is not MethodCallExpression whereCall || !IsQueryableWhereCall(whereCall))
        {
            return source.Where(predicate);
        }

        var existingLambda = (LambdaExpression) StripQuotes(whereCall.Arguments[1]);

        if (existingLambda.Parameters.Count != 1)
        {
            throw new InvalidOperationException("Or currently supports only Where(predicate) with a single parameter");
        }

        var parameter = existingLambda.Parameters[0];

        var replacedNewBody = new ParameterReplaceVisitor(predicate.Parameters[0], parameter)
            .Visit(predicate.Body);

        var combinedBody = Expression.OrElse(existingLambda.Body, replacedNewBody);
        var combinedLambda = Expression.Lambda<Func<TSource, bool>>(combinedBody, parameter);

        var newWhereCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.Where),
            [typeof(TSource)],
            whereCall.Arguments[0],
            Expression.Quote(combinedLambda)
        );

        return source.Provider.CreateQuery<TSource>(newWhereCall);
    }

    private static bool IsQueryableWhereCall(MethodCallExpression call)
    {
        return call.Method.DeclaringType == typeof(Queryable) && call.Method.Name == nameof(Queryable.Where);
    }

    private static Expression StripQuotes(Expression e)
    {
        while (e.NodeType == ExpressionType.Quote)
        {
            e = ((UnaryExpression) e).Operand;
        }

        return e;
    }

    private sealed class ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : base.VisitParameter(node);
    }
}