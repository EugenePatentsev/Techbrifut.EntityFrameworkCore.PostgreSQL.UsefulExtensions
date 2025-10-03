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

        if (valueArg is SqlConstantExpression or SqlParameterExpression)
        {
            patternExpression = CreatePatternExpression(method, valueArg);
        }
        else
        {
            throw new NotSupportedException("Only SqlConstantExpression and SqlParameterExpression are supported");
        }

        return _sqlFactory.ILike(sourceArg, patternExpression, _sqlFactory.Constant(@"\"));
    }

    private SqlExpression CreatePatternExpression(MethodInfo method, SqlExpression parameter)
    {
        var escapedParameter = EscapeParameterForLike(parameter);

        if (method == ILikeStartsWithMethod)
        {
            return _sqlFactory.Add(escapedParameter, _sqlFactory.Constant("%"));
        }

        if (method == ILikeEndsWithMethod)
        {
            return _sqlFactory.Add(_sqlFactory.Constant("%"), escapedParameter);
        }

        if (method == ILikeContainsMethod)
        {
            return _sqlFactory.Add(
                _sqlFactory.Add(_sqlFactory.Constant("%"), escapedParameter),
                _sqlFactory.Constant("%")
            );
        }

        throw new NotSupportedException("Only ILikeStartsWith, ILikeEndsWith, and ILikeContains methods are supported");
    }

    private SqlExpression EscapeParameterForLike(SqlExpression parameter)
    {
        var escaped = _sqlFactory.Function(
            "REPLACE",
            [parameter, _sqlFactory.Constant(@"\"), _sqlFactory.Constant(@"\\")],
            nullable: true,
            argumentsPropagateNullability: [true, false, false],
            typeof(string)
        );

        escaped = _sqlFactory.Function(
            "REPLACE",
            [escaped, _sqlFactory.Constant("%"), _sqlFactory.Constant(@"\%")],
            nullable: true,
            argumentsPropagateNullability: [true, false, false],
            typeof(string)
        );

        escaped = _sqlFactory.Function(
            "REPLACE",
            [escaped, _sqlFactory.Constant("_"), _sqlFactory.Constant(@"\_")],
            nullable: true,
            argumentsPropagateNullability: [true, false, false],
            typeof(string)
        );

        return escaped;
    }
}