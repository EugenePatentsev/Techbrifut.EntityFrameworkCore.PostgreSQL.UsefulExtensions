using Microsoft.EntityFrameworkCore;
using Techbrifut.EntityFrameworkCore.PostgreSQL.UsefulExtensions;

namespace ExampleApp;

public static class Program
{
    public static async Task Main()
    {
        await RecreateDatabase();
        await CreateUsersAsync();
        await UseEqualsLowerCaseAsync();
        await UseILikeAsync();
        await UseILikeStartsWithAsync();
        await UseILikeEndsWithAsync();
        await UseILikeContainsAsync();
        await UseWhereIfAsync();
        await UseOrWhereIfAsync();
        await UseAndAsync();
    }

    private static async Task RecreateDatabase()
    {
        Console.WriteLine("Recreating database...");

        await using var dbContext = AppDbContext.Create();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        Console.WriteLine("Database recreated.");
        Console.WriteLine();
    }

    private static async Task CreateUsersAsync()
    {
        Console.WriteLine("Creating users...");

        await using var db = AppDbContext.Create();

        User[] users =
        [
            new() { FirstName = "Alice", LastName = "Smith" },
            new() { FirstName = "Bob", LastName = "Johnson" },
            new() { FirstName = "Charlie", LastName = "Williams" },
            new() { FirstName = "Diana", LastName = "Brown" },
            new() { FirstName = "Ethan", LastName = "Davis" },
            new() { FirstName = "Fiona", LastName = "Clark" },
            new() { FirstName = "George", LastName = "Miller" },
            new() { FirstName = "Henry", LastName = "Wilson" },
            new() { FirstName = "John", LastName = "Taylor" },
            new() { FirstName = "Kevin", LastName = "Thomas" },
            new() { FirstName = "Lisa", LastName = "Moore" },
            new() { FirstName = "Nancy", LastName = "Jones" },
            new() { FirstName = "Oliver", LastName = "Wilson" },
            new() { FirstName = "Peter", LastName = "Thompson" },
            new() { FirstName = "Quinn", LastName = "White" },
            new() { FirstName = "Ryan", LastName = "Harris" },
            new() { FirstName = "Sarah", LastName = "Martin" },
            new() { FirstName = "Timothy", LastName = "Thompson" },
            new() { FirstName = "Uma", LastName = "Thompson" }
        ];

        await db.Users.AddRangeAsync(users);
        await db.SaveChangesAsync();

        Console.WriteLine("Users created.");
        Console.WriteLine();
    }

    private static async Task UseEqualsLowerCaseAsync()
    {
        await using var db = AppDbContext.Create();

        var users = await db.Users.AsNoTracking()
            .Where(user => user.FirstName.EqualsLowerCase("alice"))
            .ToListAsync();

        Console.WriteLine("Users with first name 'alice':");

        foreach (var user in users)
        {
            Console.WriteLine(user.FullName);
        }

        Console.WriteLine();
    }

    private static async Task UseILikeAsync()
    {
        await using var db = AppDbContext.Create();

        var users = await db.Users.AsNoTracking()
            .Where(user => user.FullName.ILike("%john%"))
            .ToListAsync();

        Console.WriteLine("Users with full name matching '%john%':");

        foreach (var user in users)
        {
            Console.WriteLine(user.FullName);
        }

        Console.WriteLine();
    }

    private static async Task UseILikeStartsWithAsync()
    {
        await using var db = AppDbContext.Create();

        var users = await db.Users.AsNoTracking()
            .Where(user => user.LastName.ILikeStartsWith("thomp"))
            .ToListAsync();

        Console.WriteLine("Users with last name starting with 'thomp':");

        foreach (var user in users)
        {
            Console.WriteLine(user.FullName);
        }

        Console.WriteLine();
    }

    private static async Task UseILikeEndsWithAsync()
    {
        await using var db = AppDbContext.Create();

        var users = await db.Users.AsNoTracking()
            .Where(user => user.LastName.ILikeEndsWith("son"))
            .ToListAsync();

        Console.WriteLine("Users with last name ending with 'son':");

        foreach (var user in users)
        {
            Console.WriteLine(user.FullName);
        }

        Console.WriteLine();
    }

    private static async Task UseILikeContainsAsync()
    {
        await using var db = AppDbContext.Create();

        var users = await db.Users.AsNoTracking()
            .Where(user => user.LastName.ILikeContains("il"))
            .ToListAsync();

        Console.WriteLine("Users with last name containing 'il':");

        foreach (var user in users)
        {
            Console.WriteLine(user.FullName);
        }

        Console.WriteLine();
    }

    private static async Task UseWhereIfAsync()
    {
        await using var db = AppDbContext.Create();

        var filter = new Filter(FirstName: "George", LastName: null);

        var users = await db.Users.AsNoTracking()
            .WhereIfIsNotNullOrEmpty(filter.FirstName, user => user.FirstName.EqualsLowerCase(filter.FirstName!))
            .WhereIfIsNotNullOrEmpty(filter.LastName, user => user.LastName.EqualsLowerCase(filter.LastName!))
            .ToListAsync();

        Console.WriteLine(filter);

        foreach (var user in users)
        {
            Console.WriteLine(user.FullName);
        }

        Console.WriteLine();
    }

    private static async Task UseOrWhereIfAsync()
    {
        await using var db = AppDbContext.Create();

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
            // (lower(u."FirstName") = lower(@filter_FirstName) AND lower(u."LastName") = lower(@filter_LastName))
            .OrWhereIf(specialFilter.IncludeAlice, user => user.FirstName.EqualsLowerCase("Alice"))
            // ... OR lower(u."FirstName") = lower('Alice')
            .ToListAsync();

        Console.WriteLine($"{filter} [{specialFilter}]");

        foreach (var user in users)
        {
            Console.WriteLine(user.FullName);
        }

        Console.WriteLine();
    }

    private static async Task UseAndAsync()
    {
        await using var db = AppDbContext.Create();

        var users = await db.Users.AsNoTracking()
            .OrWhere(user => user.FirstName.EqualsLowerCase("Alice"))
            .OrWhere(user => user.FirstName.EqualsLowerCase("Bob"))
            // (lower(u."FirstName") = lower('Alice') OR lower(u."FirstName") = lower('Bob'))
            .And()
            // ... AND ...
            .OrWhere(user => user.LastName.EqualsLowerCase("Smith"))
            .OrWhere(user => user.LastName.EqualsLowerCase("Taylor"))
            // (lower(u."LastName") = lower('Smith') OR lower(u."LastName") = lower('Taylor'))
            .ToListAsync();

        Console.WriteLine("Users with first name 'Alice' or 'Bob' and last name 'Smith' or 'Taylor':");

        foreach (var user in users)
        {
            Console.WriteLine(user.FullName);
        }

        Console.WriteLine();
    }
}