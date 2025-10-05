using System.Linq.Expressions;

namespace Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

public static class WhereGroupExtension
{
    public static IQueryable<TSource> BeginWhereGroup<TSource>(this IQueryable<TSource> source)
    {
        return source.Where(_ => true);
    }

    public static IQueryable<TSource> EndWhereGroup<TSource>(this IQueryable<TSource> source)
    {
        var predicates = new List<LambdaExpression>();
        var currentExpression = source.Expression;
        Expression? baseExpression = null;

        while (currentExpression is MethodCallExpression methodCall && IsQueryableWhereCall(methodCall))
        {
            var predicate = (LambdaExpression) StripQuotes(methodCall.Arguments[1]);

            if (IsAlwaysTruePredicate(predicate))
            {
                baseExpression = methodCall.Arguments[0];
                break;
            }

            predicates.Add(predicate);
            currentExpression = methodCall.Arguments[0];
        }

        if (baseExpression == null)
        {
            throw new InvalidOperationException("EndWhereGroup() called without a matching BeginWhereGroup()");
        }

        if (predicates.Count == 0)
        {
            return source.Provider.CreateQuery<TSource>(baseExpression);
        }

        predicates.Reverse();

        var parameter = Expression.Parameter(typeof(TSource), "x");
        Expression? combinedBody = null;

        foreach (var predicate in predicates)
        {
            var replacedBody = new ParameterReplaceVisitor(predicate.Parameters[0], parameter)
                .Visit(predicate.Body);

            combinedBody = combinedBody == null
                ? replacedBody
                : Expression.AndAlso(combinedBody, replacedBody);
        }

        if (combinedBody == null)
        {
            return source.Provider.CreateQuery<TSource>(baseExpression);
        }

        var combinedLambda = Expression.Lambda<Func<TSource, bool>>(combinedBody, parameter);

        var newWhereCall = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.Where),
            [typeof(TSource)],
            baseExpression,
            Expression.Quote(combinedLambda)
        );

        return source.Provider.CreateQuery<TSource>(newWhereCall);
    }

    public static IQueryable<TSource> BeginWhereGroupIf<TSource>(this IQueryable<TSource> source, bool condition)
    {
        return condition ? source.BeginWhereGroup() : source;
    }

    public static IQueryable<TSource> EndWhereGroupIf<TSource>(this IQueryable<TSource> source, bool condition)
    {
        return condition ? source.EndWhereGroup() : source;
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

    private static bool IsAlwaysTruePredicate(LambdaExpression lambda)
    {
        return lambda.Body is ConstantExpression { Value: true };
    }

    private sealed class ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : base.VisitParameter(node);
    }
}