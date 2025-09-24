// ReSharper disable InconsistentNaming

using Microsoft.EntityFrameworkCore;

namespace Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

/// <summary>
/// Provides string extension methods that can be translated by EF Core into PostgreSQL-specific SQL.
/// </summary>
/// <remarks>
/// These methods are placeholders intended to be used only inside LINQ-to-Entities queries. Each method throws
/// <see cref="NotSupportedException"/> when evaluated on the client. EF Core translates calls to these methods
/// to the appropriate PostgreSQL SQL using custom method translators registered via this package.
/// </remarks>
public static class StringExtensions
{
    /// <summary>
    /// Matches the <paramref name="source"/> string against a SQL ILIKE <paramref name="pattern"/> (case-insensitive LIKE).
    /// </summary>
    /// <param name="source">The string column or expression being filtered.</param>
    /// <param name="pattern">The SQL LIKE pattern to match (e.g., "%john_*").</param>
    /// <returns>
    /// True if the database value matches the supplied pattern using case-insensitive comparison; otherwise false.
    /// </returns>
    /// <remarks>
    /// This method is only for use in LINQ queries; it will be translated to PostgreSQL ILIKE at the database side.
    /// Executing it on the client will throw <see cref="NotSupportedException"/>.
    /// </remarks>
    [DbFunction]
    public static bool ILike(this string source, string pattern)
        => throw new NotSupportedException("Only for EF Core translation");

    /// <summary>
    /// Checks whether <paramref name="source"/> starts with <paramref name="value"/> using a case-insensitive comparison
    /// translated to PostgreSQL ILIKE.
    /// </summary>
    /// <param name="source">The string column or expression being filtered.</param>
    /// <param name="value">The value the source should start with.</param>
    /// <returns>True if the database value starts with the specified value (case-insensitive); otherwise false.</returns>
    /// <remarks>
    /// Only translatable in LINQ-to-Entities. Throws on client evaluation.
    /// Note: The provided value is treated as a literal and is escaped for LIKE metacharacters.
    /// Characters '%', '_' and '\' will be escaped (ESCAPE '\'), so wildcards from the input are not applied.
    /// If you need to use wildcards yourself, use ILike(source, pattern) and pass the explicit pattern (e.g., "%val%" ).
    /// </remarks>
    [DbFunction]
    public static bool ILikeStartsWith(this string source, string value)
        => throw new NotSupportedException("Only for EF Core translation");

    /// <summary>
    /// Checks whether <paramref name="source"/> ends with <paramref name="value"/> using a case-insensitive comparison
    /// translated to PostgreSQL ILIKE.
    /// </summary>
    /// <param name="source">The string column or expression being filtered.</param>
    /// <param name="value">The value the source should end with.</param>
    /// <returns>True if the database value ends with the specified value (case-insensitive); otherwise false.</returns>
    /// <remarks>
    /// Only translatable in LINQ-to-Entities. Throws on client evaluation.
    /// Note: The provided value is treated as a literal and is escaped for LIKE metacharacters.
    /// Characters '%', '_' and '\' will be escaped (ESCAPE '\'), so wildcards from the input are not applied.
    /// If you need to use wildcards yourself, use ILike(source, pattern) and pass the explicit pattern (e.g., "%val%" ).
    /// </remarks>
    [DbFunction]
    public static bool ILikeEndsWith(this string source, string value)
        => throw new NotSupportedException("Only for EF Core translation");

    /// <summary>
    /// Checks whether <paramref name="source"/> contains <paramref name="value"/> using a case-insensitive comparison
    /// translated to PostgreSQL ILIKE with wildcards on both sides.
    /// </summary>
    /// <param name="source">The string column or expression being filtered.</param>
    /// <param name="value">The value the source should contain.</param>
    /// <returns>True if the database value contains the specified value (case-insensitive); otherwise false.</returns>
    /// <remarks>
    /// Only translatable in LINQ-to-Entities. Throws on client evaluation.
    /// Note: The provided value is treated as a literal and is escaped for LIKE metacharacters.
    /// Characters '%', '_' and '\' will be escaped (ESCAPE '\'), so wildcards from the input are not applied.
    /// If you need to use wildcards yourself, use ILike(source, pattern) and pass the explicit pattern (e.g., "%val%" ).
    /// </remarks>
    [DbFunction]
    public static bool ILikeContains(this string source, string value)
        => throw new NotSupportedException("Only for EF Core translation");

    /// <summary>
    /// Compares <paramref name="source"/> and <paramref name="value"/> for equality using a case-insensitive
    /// comparison performed in the database by calling <c>lower()</c> on both operands.
    /// </summary>
    /// <param name="source">The string column or expression being compared.</param>
    /// <param name="value">The value to compare to.</param>
    /// <returns>True if the lowercase values are equal; otherwise false.</returns>
    /// <remarks>Only translatable in LINQ-to-Entities. Throws on client evaluation.</remarks>
    [DbFunction]
    public static bool EqualsLowerCase(this string source, string value)
        => throw new NotSupportedException("Only for EF Core translation");
}