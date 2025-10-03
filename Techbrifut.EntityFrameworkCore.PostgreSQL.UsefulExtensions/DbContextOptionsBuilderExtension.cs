using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

public static class DbContextOptionsBuilderExtension
{
    public static DbContextOptionsBuilder UseUsefulExtensions(this DbContextOptionsBuilder builder)
    {
        return builder.ReplaceService<IMethodCallTranslatorProvider, TranslatorProvider>();
    }
}