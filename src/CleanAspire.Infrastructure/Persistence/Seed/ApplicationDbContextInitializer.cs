// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Identities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanAspire.Infrastructure.Persistence.Seed;

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager
    )
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsRelational())
            {
                // Check if there are pending migrations before attempting to migrate
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation(
                        "Applying {Count} pending migration(s)...",
                        pendingMigrations.Count()
                    );

                    try
                    {
                        await _context.Database.MigrateAsync();
                        _logger.LogInformation("Migrations applied successfully.");
                    }
                    catch (Microsoft.Data.Sqlite.SqliteException migrateEx)
                        when (migrateEx.Message.Contains(
                                "already exists",
                                StringComparison.OrdinalIgnoreCase
                            )
                            || migrateEx.Message.Contains(
                                "duplicate",
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    {
                        // Migration failed because tables already exist
                        // This can happen if database was created manually or migration history is out of sync
                        _logger.LogWarning(
                            migrateEx,
                            "Migration failed because tables already exist. Checking migration history..."
                        );

                        // Check migration history and log status
                        var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                        var allMigrations = _context.Database.GetMigrations();
                        var missingInHistory = allMigrations.Except(appliedMigrations).ToList();

                        if (missingInHistory.Any())
                        {
                            _logger.LogWarning(
                                "Found {Count} migration(s) not recorded in history: {Migrations}. "
                                    + "Database tables exist but migration history is incomplete. "
                                    + "Consider resetting the database or manually updating the __EFMigrationsHistory table.",
                                missingInHistory.Count,
                                string.Join(", ", missingInHistory)
                            );
                        }
                        else
                        {
                            _logger.LogInformation(
                                "Migration history is synchronized. Database appears to be in a valid state despite the error."
                            );
                        }

                        // Don't throw - allow application to continue
                        return;
                    }
                }
                else
                {
                    _logger.LogInformation("Database is up to date. No pending migrations.");
                }
            }
        }
        catch (Microsoft.Data.Sqlite.SqliteException sqliteEx)
            when (sqliteEx.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase)
                || sqliteEx.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            )
        {
            // Handle case where tables exist but migration history might be missing
            _logger.LogWarning(
                sqliteEx,
                "Database tables already exist. This may indicate a migration history mismatch. "
                    + "Attempting to synchronize migration history..."
            );

            // Try to ensure the migration history is up to date
            try
            {
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                var allMigrations = _context.Database.GetMigrations();
                var missingInHistory = allMigrations.Except(appliedMigrations).ToList();

                if (missingInHistory.Any())
                {
                    _logger.LogWarning(
                        "Found {Count} migration(s) not recorded in history: {Migrations}. "
                            + "The database may need to be reset or migration history manually updated.",
                        missingInHistory.Count,
                        string.Join(", ", missingInHistory)
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "Migration history is synchronized. Database appears to be in a valid state."
                    );
                }
            }
            catch (Exception innerEx)
            {
                _logger.LogError(
                    innerEx,
                    "Error checking migration history. Database may be in an inconsistent state."
                );
            }

            // Don't throw - allow the application to continue if tables already exist
            // This is a common scenario during development
        }
        catch (Exception ex)
        {
            // Check if it's a nested SQLite exception
            if (
                ex.InnerException is Microsoft.Data.Sqlite.SqliteException innerSqliteEx
                && (
                    innerSqliteEx.Message.Contains(
                        "already exists",
                        StringComparison.OrdinalIgnoreCase
                    )
                    || innerSqliteEx.Message.Contains(
                        "duplicate",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            )
            {
                _logger.LogWarning(
                    innerSqliteEx,
                    "Database tables already exist (nested exception). Allowing application to continue."
                );
                // Don't throw - allow the application to continue
                return;
            }

            _logger.LogError(ex, "An error occurred while initialising the database");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding process...");
        try
        {
            _logger.LogInformation("Starting user seeding...");
            await SeedUsersAsync();
            _logger.LogInformation("User seeding completed.");

            _logger.LogInformation("Starting data seeding...");
            await SeedDataAsync();
            _logger.LogInformation("Data seeding completed.");

            _context.ChangeTracker.Clear();
            _logger.LogInformation("Database seeding process completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        // Check if tenants table exists by trying to query it
        try
        {
            if (!(await _context.Tenants.AnyAsync()))
            {
                var tenants = new List<Tenant>()
                {
                    new()
                    {
                        Name = "Org - 1",
                        Description = "Organization 1",
                        Id = Guid.CreateVersion7().ToString(),
                    },
                    new()
                    {
                        Name = "Org - 2",
                        Description = "Organization 2",
                        Id = Guid.CreateVersion7().ToString(),
                    },
                };
                _context.Tenants.AddRange(tenants);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
            when (ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("relation", StringComparison.OrdinalIgnoreCase)
                || (
                    ex.InnerException?.Message?.Contains(
                        "does not exist",
                        StringComparison.OrdinalIgnoreCase
                    ) ?? false
                )
                || (
                    ex.InnerException?.Message?.Contains(
                        "relation",
                        StringComparison.OrdinalIgnoreCase
                    ) ?? false
                )
            )
        {
            _logger.LogWarning(
                ex,
                "Tenants table does not exist yet. Ensure migrations are applied before seeding."
            );
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking tenants table. Skipping seed.");
            return;
        }

        if (await _userManager.Users.AnyAsync())
        {
            _logger.LogInformation("Users already exist. Skipping user seeding.");
            return;
        }

        _logger.LogInformation("Retrieving tenant for user seeding...");
        var tenant = await _context.Tenants.FirstAsync();
        var tenantId = tenant.Id;
        _logger.LogInformation("Tenant retrieved: {TenantId}", tenantId);

        var defaultPassword = "P@ssw0rd!";
        _logger.LogInformation("Seeding users...");

        var adminUser = new ApplicationUser
        {
            UserName = "Administrator",
            Nickname = "Administrator",
            Email = "admin@example.com",
            EmailConfirmed = true,
            LanguageCode = "en-US",
            TimeZoneId = "Asia/Shanghai",
            TwoFactorEnabled = false,
        };

        _logger.LogInformation("Creating administrator user...");
        var adminResult = await _userManager.CreateAsync(adminUser, defaultPassword);
        if (!adminResult.Succeeded)
        {
            _logger.LogError(
                "Failed to create administrator user. Errors: {Errors}",
                string.Join(", ", adminResult.Errors.Select(e => e.Description))
            );
            throw new InvalidOperationException(
                $"Failed to create administrator user: {string.Join(", ", adminResult.Errors.Select(e => e.Description))}"
            );
        }
        _logger.LogInformation(
            "Administrator user created successfully. UserId: {UserId}",
            adminUser.Id
        );

        var demoUser = new ApplicationUser
        {
            UserName = "Demo",
            Nickname = "Demo",
            Email = "Demo@example.com",
            EmailConfirmed = true,
            LanguageCode = "en-US",
            TimeZoneId = "Asia/Shanghai",
            TwoFactorEnabled = false,
            SuperiorId = adminUser.Id,
        };

        _logger.LogInformation("Creating demo user...");
        var demoResult = await _userManager.CreateAsync(demoUser, defaultPassword);
        if (!demoResult.Succeeded)
        {
            _logger.LogError(
                "Failed to create demo user. Errors: {Errors}",
                string.Join(", ", demoResult.Errors.Select(e => e.Description))
            );
            throw new InvalidOperationException(
                $"Failed to create demo user: {string.Join(", ", demoResult.Errors.Select(e => e.Description))}"
            );
        }
        _logger.LogInformation("Demo user created successfully. UserId: {UserId}", demoUser.Id);
        _logger.LogInformation("User seeding completed successfully.");
    }

    private async Task SeedDataAsync()
    {
        if (await _context.Products.AnyAsync())
        {
            _logger.LogInformation("Products already exist. Skipping data seeding.");
            return;
        }

        _logger.LogInformation("Seeding data...");
        _logger.LogInformation("Creating product list...");
        var products = new List<Product>
        {
            new Product
            {
                Name = "Ikea LACK Coffee Table",
                Description =
                    "Simple and stylish coffee table from Ikea, featuring a modern design and durable surface. Perfect for living rooms or offices.",
                Price = 25,
                SKU = "LACK-COFFEE-TABLE",
                UOM = "PCS",
                Currency = "USD",
                Category = ProductCategory.Furniture,
            },
            new Product
            {
                Name = "Nike Air Zoom Pegasus 40",
                Description =
                    "Lightweight and responsive running shoes with advanced cushioning and a breathable mesh upper. Ideal for athletes and daily runners.",
                Price = 130,
                SKU = "NIKE-PEGASUS-40",
                UOM = "PCS",
                Currency = "USD",
                Category = ProductCategory.Sports,
            },
            new Product
            {
                Name = "Adidas Yoga Mat",
                Description =
                    "Non-slip yoga mat with a 6mm thickness for optimal cushioning and support during workouts. Suitable for yoga, pilates, or general exercises.",
                Price = 45,
                SKU = "ADIDAS-YOGA-MAT",
                UOM = "PCS",
                Currency = "USD",
                Category = ProductCategory.Sports,
            },
            new Product
            {
                Name = "Ikea HEMNES Bed Frame",
                Description =
                    "Solid wood bed frame with a classic design. Offers excellent durability and comfort. Compatible with standard-size mattresses.",
                Price = 199,
                SKU = "HEMNES-BED-FRAME",
                UOM = "PCS",
                Currency = "USD",
                Category = ProductCategory.Furniture,
            },
            new Product
            {
                Name = "Under Armour Men's HeatGear Compression Shirt",
                Description =
                    "High-performance compression shirt designed to keep you cool and dry during intense workouts. Made from moisture-wicking fabric.",
                Price = 35,
                SKU = "UA-HEATGEAR-SHIRT",
                UOM = "PCS",
                Currency = "USD",
                Category = ProductCategory.Sports,
            },
            new Product
            {
                Name = "Apple iPhone 15 Pro",
                Description =
                    "Apple's latest flagship smartphone featuring a 6.1-inch Super Retina XDR display, A17 Pro chip, titanium frame, and advanced camera system with 5x telephoto lens. Ideal for tech enthusiasts and professional users.",
                Price = 1199,
                SKU = "IP15PRO",
                UOM = "PCS",
                Currency = "USD",
                Category = ProductCategory.Electronics,
            },
        };

        _logger.LogInformation("Adding {Count} products to database...", products.Count);
        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Products saved successfully.");
        _logger.LogInformation("Data seeding completed successfully.");
    }
}
