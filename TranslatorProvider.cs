using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

namespace Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
internal sealed class TranslatorProvider : NpgsqlMethodCallTranslatorProvider
{
    public TranslatorProvider(
        RelationalMethodCallTranslatorProviderDependencies dependencies,
        IModel model,
        IDbContextOptions contextOptions
    ) : base(dependencies, model, contextOptions)
    {
        var sqlExpressionFactory = (NpgsqlSqlExpressionFactory) dependencies.SqlExpressionFactory;

        AddTranslators([
            new ILikeTranslator(sqlExpressionFactory),
            new EqualsLowerCaseTranslator(sqlExpressionFactory)
        ]);
    }
}