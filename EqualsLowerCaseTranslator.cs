using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

internal sealed class EqualsLowerCaseTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo EqualsLowerCaseMethod =
        typeof(StringExtensions).GetMethod(nameof(StringExtensions.EqualsLowerCase))!;

    private readonly NpgsqlSqlExpressionFactory _sqlFactory;

    public EqualsLowerCaseTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method != EqualsLowerCaseMethod) return null;

        var source = arguments[0];
        var value = arguments[1];

        var lowerSource = _sqlFactory.Function(
            "lower",
            [source],
            nullable: true,
            argumentsPropagateNullability: [true],
            typeof(string)
        );

        var lowerValue = _sqlFactory.Function(
            "lower",
            [value],
            nullable: true,
            argumentsPropagateNullability: [true],
            typeof(string)
        );

        return _sqlFactory.Equal(lowerSource, lowerValue);
    }
}