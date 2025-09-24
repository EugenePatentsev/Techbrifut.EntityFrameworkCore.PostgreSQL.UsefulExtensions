using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

public static class DbContextOptionsBuilderExtension
{
    /// <summary>
    /// Enables custom method translators so calls from this package can be evaluated on the database side.
    /// </summary>
    /// <param name="builder">The options builder.</param>
    /// <returns>The same builder for chaining.</returns>
    public static DbContextOptionsBuilder UseUsefulExtensions(this DbContextOptionsBuilder builder)
    {
        return builder.ReplaceService<IMethodCallTranslatorProvider, TranslatorProvider>();
    }
}