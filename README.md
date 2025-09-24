# EntityFrameworkCore: PostgreSQL Useful Extensions

## Features

- **Case-insensitive string matching for PostgreSQL**:
  - `ILike`, `ILikeStartsWith`, `ILikeEndsWith`, `ILikeContains` extension methods for use in LINQ-to-Entities queries, translated to PostgreSQL `ILIKE`.
- **Case-insensitive equality**:
  - `EqualsLowerCase` extension method for comparing strings in a case-insensitive manner.
- **Dynamic query composition**:
  - `OrWhere` extension method for combining multiple predicates with OR logic in LINQ queries.

## Setup

Register the useful extensions in your `DbContext` configuration:

```csharp
var optionsBuilder = new DbContextOptionsBuilder()
    .UseNpgsql("ConnectionString")
    .UseUsefulExtensions(); // <- Add here
```

Or, if using dependency injection:

```csharp
services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql("ConnectionString")
    .UseUsefulExtensions() // <- Add here
);
```

## Usage

### Case-insensitive string matching

```csharp
// Find users whose name matches pattern, case-insensitively
var users = await db.Users.AsNoTracking()
    .Where(user => user.FirstName.ILike("%john%"))
    .ToListAsync();
```

### StartsWith/EndsWith/Contains (case-insensitive)

```csharp
var users = await db.Users.AsNoTracking()
    .Where(user => user.FirstName.ILikeStartsWith("jo")) // matches names starting with literal "jo"
    .ToListAsync();

var usersEnding = await db.Users.AsNoTracking()
    .Where(user => user.FirstName.ILikeEndsWith("son")) // matches names ending with literal "son"
    .ToListAsync();

var usersContaining = await db.Users.AsNoTracking()
    .Where(user => user.FullName.ILikeContains("Smith")) // matches users whose full name contains literal "Smith"
    .ToListAsync();
```

| Method                | SQL Translation                        | Parameter Value                |
|-----------------------|----------------------------------------|-------------------------------|
| ILikeStartsWith(col, v) | col ILIKE @p ESCAPE '\\'               | escaped(v) + "%"              |
| ILikeEndsWith(col, v)   | col ILIKE @p ESCAPE '\\'               | "%" + escaped(v)              |
| ILikeContains(col, v)   | col ILIKE @p ESCAPE '\\'               | "%" + escaped(v) + "%"        |
- Escaping: backslash (\\) is used as the escape character; existing \\, %, _ are escaped.

### Case-insensitive equality

```csharp
var users = await db.Users.AsNoTracking()
    .Where(user => user.FirstName.EqualsLowerCase("john"))
    .ToListAsync();
```

Translates to `lower(source) = lower(value)`, enabling case-insensitive equality in SQL.

### Dynamic OR queries

```csharp
var users = await db.Users.AsNoTracking()
    .Where(user => user.FirstName.EqualsLowerCase("john"))
    .OrWhere(user => user.FirstName.EqualsLowerCase("jane"))
    .ToListAsync();
```

If the query has no `Where`, behaves like a regular `Where` with the provided predicate

## Notes

- These extension methods are only supported in LINQ-to-Entities queries and will be translated to SQL.
- Do **NOT** use them in client-side code; they will throw `NotSupportedException`.
- Consider proper indexing (e.g., functional indexes on lower(column)) when using case-insensitive comparisons for performance.
- For ILIKE with leading wildcards (e.g., %value), regular B-Tree indexes won't be used. Consider trigram (pg_trgm) indexes for faster pattern searches.
