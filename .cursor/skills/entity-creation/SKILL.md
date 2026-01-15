---
name: entity-creation
description: Step-by-step guide for creating a new entity in CleanAspire project following the established architecture pattern.

---

# Creating a New Entity in CleanAspire

When creating a new entity in the CleanAspire project, follow these steps in order:

## 1. Create Entity Class

Create a new class for the entity (e.g., `Product.cs`) in the `Entities` directory of the `CleanAspire.Domain` project.

- The class should typically inherit from `BaseAuditableEntity` and implement the `IAuditTrial` interface for audit tracking (created/modified timestamps).

**Example:**
```csharp
public class Product : BaseAuditableEntity, IAuditTrial
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    // Other properties...
}
```

## 2. Add DbSet to IApplicationDbContext

In the `IApplicationDbContext.cs` file in the `CleanAspire.Application` project, add a `DbSet<TEntity>` property.

**Example:**
```csharp
public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    // Other DbSets...
}
```

## 3. Implement DbSet in ApplicationDbContext

In the `ApplicationDbContext.cs` file in the `CleanAspire.Infrastructure` project, implement the `DbSet<TEntity>` property.

**Example:**
```csharp
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Product> Products { get; set; }
    // Other DbSets...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Additional configurations
    }
}
```

## 4. Define Entity Configuration

Create a configuration class for the entity in the `Persistence\Configurations` directory under the `CleanAspire.Infrastructure` project.

- Create a new configuration class (e.g., `ProductConfiguration.cs`)
- Implement `IEntityTypeConfiguration<TEntity>`
- Define constraints, field lengths, foreign keys, etc.

**Example:**
```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(100);
        // Add other property configurations here...
    }
}
```

## 5. Create Migration

You have two options for creating migrations:

### Option 1: Create migrations for all database providers (Recommended)

Use the provided PowerShell script to create migrations for all providers simultaneously:

```powershell
.\scripts\Add-Migration-All.ps1 -MigrationName "YourMigrationName"
```

This creates migrations for PostgreSQL and SQLite in one command.

You can optionally specify a different startup project:
```powershell
.\scripts\Add-Migration-All.ps1 -MigrationName "YourMigrationName" -StartupProject "src\CleanAspire.AppHost\CleanAspire.AppHost.csproj"
```

### Option 2: Create migration for a single provider

1. Ensure the correct startup project is selected (`CleanAspire.AppHost` or `CleanAspire.Api`)
2. Open **Package Manager Console** in Visual Studio
3. Set the default project to match your database configuration in `appsettings.json` (e.g., `Migrators.SQLite` or `Migrators.PostgreSQL`)
4. Run:
   ```powershell
   PM> Add-Migration Product
   PM> Update-Database
   ```

## 6. Remove All Migrations (if needed)

To remove all migrations from all database provider projects, use the provided PowerShell script:

```powershell
.\scripts\Remove-Migrations-All.ps1
```

The script will prompt for confirmation. To skip confirmation (useful for automation):
```powershell
.\scripts\Remove-Migrations-All.ps1 -Confirm
```

This removes all migration files from PostgreSQL and SQLite projects, allowing you to start fresh with new migrations.

## Important Notes

- Always follow the order: Entity Class → DbSet Interface → DbSet Implementation → Configuration → Migration
- Use `BaseAuditableEntity` and `IAuditTrial` for audit tracking
- Prefer Option 1 (all providers) for migrations to maintain consistency across database providers
- Ensure the correct startup project is set before creating migrations manually
