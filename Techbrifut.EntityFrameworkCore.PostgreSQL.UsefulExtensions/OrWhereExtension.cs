using System.Linq.Expressions;

namespace Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

public static class OrWhereExtension
{
    public static IQueryable<TSource> OrWhere<TSource>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, bool>> predicate
    )
    {
        if (source.Expression is not MethodCallExpression whereCall || !IsQueryableWhereCall(whereCall))
        {
            return source.Where(predicate);
        }

        var existingLambda = (LambdaExpression) StripQuotes(whereCall.Arguments[1]);

        if (existingLambda.Parameters.Count != 1)
        {
            throw new NotSupportedException("Or currently supports only Where(predicate) with a single parameter");
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

    public static IQueryable<TSource> And<TSource>(this IQueryable<TSource> source)
    {
        return source.Where(_ => false);
    }

    public static IQueryable<TSource> AndIf<TSource>(this IQueryable<TSource> source, bool condition)
    {
        return condition ? source.And() : source;
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