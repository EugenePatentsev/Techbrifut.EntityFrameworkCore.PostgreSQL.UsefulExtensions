using Microsoft.EntityFrameworkCore;

namespace Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

public static class StringExtensions
{
    [DbFunction]
    public static bool ILike(this string source, string pattern)
        => throw new NotSupportedException("Only for EF Core translation");

    [DbFunction]
    public static bool ILikeStartsWith(this string source, string value)
        => throw new NotSupportedException("Only for EF Core translation");

    [DbFunction]
    public static bool ILikeEndsWith(this string source, string value)
        => throw new NotSupportedException("Only for EF Core translation");

    [DbFunction]
    public static bool ILikeContains(this string source, string value)
        => throw new NotSupportedException("Only for EF Core translation");

    [DbFunction]
    public static bool EqualsLowerCase(this string source, string value)
        => throw new NotSupportedException("Only for EF Core translation");
}