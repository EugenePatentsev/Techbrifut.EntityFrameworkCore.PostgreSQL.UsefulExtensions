using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExampleApp;

public sealed class User
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string LastName { get; set; }
    public required string FirstName { get; set; }

    public string FullName { get; private set; } = null!;
}

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);

        builder.Property(user => user.FullName)
            .HasComputedColumnSql($"""
                                   "{nameof(User.FirstName)}" || ' ' || "{nameof(User.LastName)}"
                                   """, stored: true);
    }
}