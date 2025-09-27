// ReSharper disable InconsistentNaming

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

internal class ILikeTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo ILikeMethod =
        typeof(StringExtensions).GetMethod(nameof(StringExtensions.ILike))!;

    private static readonly MethodInfo ILikeStartsWithMethod =
        typeof(StringExtensions).GetMethod(nameof(StringExtensions.ILikeStartsWith))!;

    private static readonly MethodInfo ILikeEndsWithMethod =
        typeof(StringExtensions).GetMethod(nameof(StringExtensions.ILikeEndsWith))!;

    private static readonly MethodInfo ILikeContainsMethod =
        typeof(StringExtensions).GetMethod(nameof(StringExtensions.ILikeContains))!;

    private readonly NpgsqlSqlExpressionFactory _sqlFactory;

    public ILikeTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        if (sqlExpressionFactory is not NpgsqlSqlExpressionFactory sqlFactory)
        {
            throw new ArgumentException("Only NpgsqlSqlExpressionFactory is supported");
        }

        _sqlFactory = sqlFactory;
    }

    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger
    )
    {
        if (method == ILikeMethod)
        {
            return _sqlFactory.ILike(arguments[0], arguments[1], _sqlFactory.Constant(@"\"));
        }

        if (method != ILikeStartsWithMethod && method != ILikeEndsWithMethod && method != ILikeContainsMethod)
        {
            return null;
        }

        var sourceArg = arguments[0];
        var valueArg = arguments[1];

        SqlExpression patternExpression;

        if (valueArg is SqlConstantExpression { Value: string value })
        {
            patternExpression = CreatePatternExpression(method, value);
        }
        else
        {
            throw new NotSupportedException("Only string values are supported");
        }

        return _sqlFactory.ILike(sourceArg, patternExpression, _sqlFactory.Constant(@"\"));
    }

    private SqlExpression CreatePatternExpression(MethodInfo method, string value)
    {
        string escapedValue = EscapeLikePattern(value);

        if (method == ILikeStartsWithMethod)
        {
            return _sqlFactory.Constant($"{escapedValue}%");
        }

        if (method == ILikeEndsWithMethod)
        {
            return _sqlFactory.Constant($"%{escapedValue}");
        }

        if (method == ILikeContainsMethod)
        {
            return _sqlFactory.Constant($"%{escapedValue}%");
        }

        throw new NotSupportedException("Only ILikeStartsWith, ILikeEndsWith, and ILikeContains methods are supported");
    }

    private static string EscapeLikePattern(string input)
    {
        return input
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_");
    }
}