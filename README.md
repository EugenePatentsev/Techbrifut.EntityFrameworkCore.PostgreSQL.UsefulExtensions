# EntityFrameworkCore: PostgreSQL Useful Extensions

![GitHub License](https://img.shields.io/github/license/EugenePatentsev/Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions)
[![NuGet Version](https://img.shields.io/nuget/v/Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions?style=flat&logo=nuget)](https://www.nuget.org/packages/Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions?logo=nuget&color=blue)](https://www.nuget.org/packages/Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions)

Entity Framework Core extensions for PostgreSQL: case-insensitive string matching (ILike, EqualsLowerCase), dynamic OR
query composition (OrWhere), conditional filtering (WhereIf), and seamless LINQ-to-SQL translation for efficient,
expressive queries.

## Features

- `EqualsLowerCase`: Case-insensitive equality using `lower()` on both sides: `string.EqualsLowerCase(value)`.
- `ILike`: Case-insensitive pattern matching translated to PostgreSQL ILIKE with proper escaping. Methods:
  `string.ILike(pattern)`, `string.ILikeStartsWith(value)`, `string.ILikeEndsWith(value)`,
  `string.ILikeContains(value)`.
- `OrWhere`: Combines a new predicate with the previous Where predicate using OR, enabling alternative conditions in a
  single Where chain.
- `WhereIf`: Adds filters conditionally (e.g., only when a value is non-empty or a flag is true) to keep queries clean
  and composable.

## Requirements

- .NET 10.0
- Entity Framework Core 10.x
- Npgsql.EntityFrameworkCore.PostgreSQL 10.x

> **Note:** For .NET 8 and .NET 9 support, use package versions v8.x and v9.x respectively.

## Installation

Using .NET CLI:

```bash
dotnet add package Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions
```

Or via NuGet Package Manager: search for
`Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions`.

## Quick start

Register the translators once when configuring your DbContext options:

```csharp
using Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

var optionsBuilder = new DbContextOptionsBuilder()
    .UseNpgsql("Your connection string")
    .UseUsefulExtensions(); // << enable extensions
```

For applications using .NET's built-in DI container (e.g., ASP.NET Core, Blazor, or hosted services), configure your
DbContext in the service collection:

```csharp
using Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql("Your connection string")
    .UseUsefulExtensions() // << enable extensions
);
```

## EqualsLowerCase

Case-insensitive equality without patterns. This translates your LINQ into `lower(..) = lower(..)` in SQL, so both sides
are converted to lowercase before comparing.

```csharp
var users = await db.Users.AsNoTracking()
    .Where(user => user.FirstName.EqualsLowerCase("alice"))
    .ToListAsync();
```

Generated SQL (conceptually)

```sql
WHERE lower(u."Name") = lower(@__name)
```

> - Consider proper indexing (e.g., functional indexes on lower(column)) when using case-insensitive comparisons for
    performance.
> - Prefer `EqualsLowerCase` when you need exact equality ignoring case. Use `ILike*` helpers for pattern-based
    matching (contains/starts/ends).

## ILike

Case-insensitive pattern matching using PostgreSQL's `ILIKE` operator. The extension automatically handles proper
escaping of special characters (`%`, `_`, `\`) to prevent unintended pattern matching.

### ILike

Use custom patterns with `%` and `_` wildcards:

```csharp
var users = await db.Users.AsNoTracking()
    .Where(user => user.FullName.ILike("%john%"))
    .ToListAsync();
```

Generated SQL (conceptually)

```sql
WHERE u."FullName" ILIKE '%john%' ESCAPE '\'
```

### ILikeStartsWith

Match strings that start with a value:

```csharp
var users = await db.Users.AsNoTracking()
    .Where(user => user.LastName.ILikeStartsWith("thomp"))
    .ToListAsync();
```

Generated SQL (conceptually)

```sql
WHERE u."LastName" ILIKE REPLACE(REPLACE(REPLACE(@__value, '\', '\\'), '%', '\%'), '_', '\_') || '%' ESCAPE '\'
```

### ILikeEndsWith

Match strings that end with a value:

```csharp
var users = await db.Users.AsNoTracking()
    .Where(user => user.LastName.ILikeEndsWith("son"))
    .ToListAsync();
```

Generated SQL (conceptually)

```sql
WHERE u."LastName" ILIKE '%' || REPLACE(REPLACE(REPLACE(@__value, '\', '\\'), '%', '\%'), '_', '\_') ESCAPE '\'
```

### ILikeContains

Match strings that contain a value:

```csharp
var users = await db.Users.AsNoTracking()
    .Where(user => user.LastName.ILikeContains("il"))
    .ToListAsync();
```

```sql
WHERE u."LastName" ILIKE '%' || REPLACE(REPLACE(REPLACE(@__value, '\', '\\'), '%', '\%'), '_', '\_') || '%' ESCAPE '\'
```

> - The `ILike*` helpers (`ILikeStartsWith`, `ILikeEndsWith`, `ILikeContains`) automatically escape special LIKE
    characters in the input value to ensure literal matching.
> - Use the basic `ILike` method when you need full control over pattern matching with explicit wildcards.
> - Consider GIN/GiST indexes with pg_trgm extension for better performance on pattern matching queries.

## OrWhere

Combines a new predicate with the previous `Where` predicate using OR logic, enabling alternative conditions in a single
query chain. This is particularly useful when you need to apply conditional OR logic dynamically.

### Basic Usage

Chain `OrWhere` after a `Where` call to create an OR condition:

```csharp
var users = await db.Users.AsNoTracking()
    .Where(user => user.FirstName.EqualsLowerCase("Alice"))
    .OrWhere(user => user.LastName.EqualsLowerCase("Smith"))
    .ToListAsync();
```

Generated SQL (conceptually)

```sql
WHERE lower(u."FirstName") = lower('Alice') OR lower(u."LastName") = lower('Smith')
```

### Grouping Filters with BeginWhereGroup and EndWhereGroup

When building dynamic queries with conditional filters, you often need to group multiple conditions together and combine
them with other filters using OR logic. The `BeginWhereGroup()` and `EndWhereGroup()` methods allow you to create
parenthesized groups of AND conditions that can be combined with OR logic.

#### Basic Grouping

Use `BeginWhereGroup()` to start a group and `EndWhereGroup()` to close it. All `Where` or `WhereIf` calls between these
methods are combined with AND and wrapped in parentheses:

```csharp
var filter = new Filter(FirstName: "Quinn", LastName: "White");

var users = await db.Users.AsNoTracking()
    .BeginWhereGroup()
    .WhereIfIsNotNullOrEmpty(filter.FirstName, user => user.FirstName.EqualsLowerCase(filter.FirstName!))
    .WhereIfIsNotNullOrEmpty(filter.LastName, user => user.LastName.EqualsLowerCase(filter.LastName!))
    .EndWhereGroup()
    .ToListAsync();
```

Generated SQL (conceptually)

```sql
WHERE (lower(u."FirstName") = lower('Quinn') AND lower(u."LastName") = lower('White'))
```

#### Combining Groups with OR Logic

The real power of grouping comes when combining groups with `OrWhere` or `OrWhereIf`:

```csharp
var filter = new Filter(FirstName: "Quinn", LastName: "White");

var specialFilter = new
{
    IncludeAlice = true
};

var users = await db.Users.AsNoTracking()
    .BeginWhereGroup()
    .WhereIfIsNotNullOrEmpty(filter.FirstName, user => user.FirstName.EqualsLowerCase(filter.FirstName!))
    .WhereIfIsNotNullOrEmpty(filter.LastName, user => user.LastName.EqualsLowerCase(filter.LastName!))
    .EndWhereGroup()
    .OrWhereIf(specialFilter.IncludeAlice, user => user.FirstName.EqualsLowerCase("Alice"))
    .ToListAsync();
```

Generated SQL (conceptually)

```sql
WHERE (lower(u."FirstName") = lower('Quinn') AND lower(u."LastName") = lower('White'))
   OR lower(u."FirstName") = lower('Alice')
```

This creates a query that returns users matching both first and last name from the filter, OR users named Alice.

#### Conditional Grouping

Use `BeginWhereGroupIf()` and `EndWhereGroupIf()` to conditionally apply grouping based on a condition:

```csharp
bool applyComplexFilter = true;
var filter = new Filter(FirstName: "Quinn", LastName: "White");

var users = await db.Users.AsNoTracking()
    .BeginWhereGroupIf(applyComplexFilter)
    .WhereIfIsNotNullOrEmpty(filter.FirstName, user => user.FirstName.EqualsLowerCase(filter.FirstName!))
    .WhereIfIsNotNullOrEmpty(filter.LastName, user => user.LastName.EqualsLowerCase(filter.LastName!))
    .EndWhereGroupIf(applyComplexFilter)
    .ToListAsync();
```

If `applyComplexFilter` is `false`, the conditions are applied without grouping parentheses.

> - Always pair `BeginWhereGroup()` with `EndWhereGroup()`. Calling `EndWhereGroup()` without a matching
    `BeginWhereGroup()` will throw an `InvalidOperationException`.
> - Groups are particularly useful when building complex dynamic queries with multiple optional filter sets that need to
    be OR-ed together.
> - Empty groups (where all conditional filters are skipped) are automatically removed from the query.
> - Use `BeginWhereGroupIf()` and `EndWhereGroupIf()` with the same condition value to ensure proper pairing.

## And() with OrWhere groups

Use And() to join OrWhere groups with the logical AND operator. It closes the current OR group and starts a new one that
will be AND-ed with the previous group. The regular Where() cannot be used after And().

```csharp
var users = await db.Users.AsNoTracking()
    .OrWhere(user => user.FirstName.EqualsLowerCase("Alice"))
    .OrWhere(user => user.FirstName.EqualsLowerCase("Bob"))
    .And()
    .OrWhere(user => user.LastName.EqualsLowerCase("Smith"))
    .OrWhere(user => user.LastName.EqualsLowerCase("Taylor"))
    .ToListAsync();
```

Generated SQL (conceptually)

```sql
WHERE
  (lower(u."FirstName") = lower('Alice') OR lower(u."FirstName") = lower('Bob'))
  AND
  (lower(u."LastName") = lower('Smith') OR lower(u."LastName") = lower('Taylor'))
```

> - Use And() only between OrWhere chains to start a new OR-group.
> - Do not call Where() immediately after And(); continue with OrWhere to build the next group.
> - AndIf(condition) conditionally applies And(); if condition is false, the chain continues without introducing a new
    AND group.

# WhereIf

Conditionally applies filters to queries only when specified conditions are met, keeping your query logic clean and
composable. This eliminates the need for branching logic when building dynamic queries based on optional filter
parameters.

## WhereIf

Apply a filter only when a condition is true:

```csharp
bool includeActive = true;

var users = await db.Users.AsNoTracking()
    .WhereIf(includeActive, user => user.IsActive)
    .ToListAsync();
```

Generated SQL (conceptually)

```sql
WHERE u."IsActive" = TRUE
```

If `includeActive` is `false`, no filter is applied and the query returns all users.

## WhereIfIsNotNullOrEmpty (String)

Apply a filter only when a string value is not null or empty:

```csharp
var filter = new Filter(FirstName: "George", LastName: null);

var users = await db.Users.AsNoTracking()
    .WhereIfIsNotNullOrEmpty(filter.FirstName, user => user.FirstName.EqualsLowerCase(filter.FirstName!))
    .WhereIfIsNotNullOrEmpty(filter.LastName, user => user.LastName.EqualsLowerCase(filter.LastName!))
    .ToListAsync();
```

Generated SQL (conceptually)

```sql
WHERE lower(u."FirstName") = lower('George')
```

The `LastName` filter is not applied because the value is `null`.

## WhereIfIsNotNullOrWhiteSpace

Apply a filter only when a string value is not null, empty, or whitespace:

```csharp
string name = "   "; // whitespace only

var users = await db.Users.AsNoTracking()
    .WhereIfIsNotNullOrWhiteSpace(name, user => user.FullName.ILikeContains(name))
    .ToListAsync();
```

No filter is applied since the value contains only whitespace, returning all users.

## WhereIfIsNotNullOrEmpty (Guid)

Apply a filter only when a Guid value is not null or empty:

```csharp
Guid? departmentId = Guid.NewGuid();

var users = await db.Users.AsNoTracking()
    .WhereIfIsNotNullOrEmpty(departmentId, user => user.DepartmentId == departmentId)
    .ToListAsync();
```

Generated SQL (conceptually)

```sql
WHERE u."DepartmentId" = @__departmentId
```

## OrWhereIf Variants

Combine conditional filters with OR logic. These methods follow the same patterns as their `WhereIf` counterparts but
use OR instead of AND:

- **`OrWhereIf`**: Apply an OR filter when a condition is true
- **`OrWhereIfIsNotNullOrEmpty`**: Apply an OR filter when a string is not null or empty
- **`OrWhereIfIsNotNullOrWhiteSpace`**: Apply an OR filter when a string is not null, empty, or whitespace
- **`OrWhereIfIsNotNullOrEmpty`** (Guid): Apply an OR filter when a Guid is not null or empty

```csharp
var filter = new Filter(FirstName: string.Empty, LastName: "White");

var specialFilter = new
{
    IncludeAlice = true
};

var users = await db.Users.AsNoTracking()
    .WhereIfIsNotNullOrEmpty(filter.FirstName, user => user.FirstName.EqualsLowerCase(filter.FirstName!))
    .WhereIfIsNotNullOrEmpty(filter.LastName, user => user.LastName.EqualsLowerCase(filter.LastName!))
    .OrWhereIf(specialFilter.IncludeAlice, user => user.FirstName.EqualsLowerCase("Alice"))
    .ToListAsync();
```

Generated SQL (conceptually)

```sql
WHERE lower(u."LastName") = lower('White') OR lower(u."FirstName") = lower('Alice')