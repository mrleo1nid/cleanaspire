# Справочник по реализации удаляемых компонентов

## Описание

Этот документ содержит подробные примеры реализации демонстрационных сущностей и функций, которые были удалены из проекта CleanAspire. Сохранен для справки и возможного восстановления функционала в будущих проектах.

**Дата создания:** 2026-01-15
**Проект:** CleanAspire
**Архитектура:** Clean Architecture с разделением на Domain, Application, Infrastructure, API слои

---

## 1. Демонстрационные сущности (Clean Architecture Pattern)

### 1.1 Product (Продукт) - Полная реализация

#### Domain Layer - Сущность

**Файл:** `src/CleanAspire.Domain/Entities/Product.cs`

```csharp
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Represents a product entity.
/// </summary>
public class Product : BaseAuditableEntity, IAuditTrial
{
    /// <summary>
    /// Gets or sets the SKU of the product.
    /// </summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category of the product.
    /// </summary>
    public ProductCategory Category { get; set; } = ProductCategory.Electronics;

    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the currency of the product price.
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure of the product.
    /// </summary>
    public string? UOM { get; set; }
}

/// <summary>
/// Represents the category of a product.
/// </summary>
public enum ProductCategory
{
    Electronics,
    Furniture,
    Clothing,
    Food,
    Beverages,
    HealthCare,
    Sports,
}
```

**Ключевые особенности:**
- Наследуется от `BaseAuditableEntity` (предоставляет Id, Created, CreatedBy, LastModified, LastModifiedBy)
- Реализует интерфейс `IAuditTrial` для отслеживания изменений
- Enum `ProductCategory` для строгой типизации категорий

---

#### Infrastructure Layer - EF Core Configuration

**Файл:** `src/CleanAspire.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`

```csharp
using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the Product entity.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Primary key with max length
        builder.Property(x => x.Id).HasMaxLength(50);

        // Convert enum to string in database
        builder.Property(x => x.Category).HasConversion<string>();

        // Unique constraint on Name
        builder.HasIndex(x => x.Name).IsUnique();

        // Name is required with max length
        builder.Property(x => x.Name).HasMaxLength(80).IsRequired();

        // Ignore domain events (not persisted)
        builder.Ignore(e => e.DomainEvents);
    }
}
```

**Ключевые особенности:**
- Enum хранится как строка в БД для читаемости
- Уникальный индекс на Name для предотвращения дубликатов
- DomainEvents исключены из маппинга (используются для event-driven архитектуры)

---

#### Application Layer - DTO

**Файл:** `src/CleanAspire.Application/Features/Products/DTOs/ProductDto.cs`

```csharp
namespace CleanAspire.Application.Features.Products.DTOs;

public class ProductDto
{
    public string Id { get; set; }
    public string SKU { get; set; }
    public string Name { get; set; }
    public ProductCategory Category { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Currency { get; set; }
    public string? UOM { get; set; }
}
```

---

#### Application Layer - Create Command (CQRS Pattern)

**Файл:** `src/CleanAspire.Application/Features/Products/Commands/CreateProductCommand.cs`

```csharp
using CleanAspire.Application.Features.Products.DTOs;
using CleanAspire.Application.Features.Products.EventHandlers;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Products.Commands;

// Command object (immutable record)
public record CreateProductCommand(
    string SKU,
    string Name,
    ProductCategory Category,
    string? Description,
    decimal Price,
    string? Currency,
    string? UOM
) : IFusionCacheRefreshRequest<ProductDto>, IRequiresValidation
{
    // Tags for cache invalidation
    public IEnumerable<string>? Tags => new[] { "products" };
}

// Command Handler (separates request from handling logic)
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<ProductDto> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken
    )
    {
        var product = new Product
        {
            SKU = request.SKU,
            Name = request.Name,
            Category = request.Category,
            Description = request.Description,
            Price = request.Price,
            Currency = request.Currency,
            UOM = request.UOM,
        };

        // Add domain event (for event-driven architecture)
        product.AddDomainEvent(new ProductCreatedEvent(product));

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return new ProductDto()
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
        };
    }
}
```

**Ключевые особенности:**
- CQRS pattern: Command отделен от Handler
- Immutable record для command (thread-safe)
- Domain Events для асинхронной обработки
- Cache invalidation через Tags
- Validation через `IRequiresValidation`

---

#### API Layer - Minimal API Endpoints

**Файл:** `src/CleanAspire.Api/Endpoints/ProductEndpointRegistrar.cs`

```csharp
using CleanAspire.Application.Common.Models;
using CleanAspire.Application.Features.Products.Commands;
using CleanAspire.Application.Features.Products.DTOs;
using CleanAspire.Application.Features.Products.Queries;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

public class ProductEndpointRegistrar(ILogger<ProductEndpointRegistrar> logger) : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/products")
            .WithTags("products")
            .RequireAuthorization();

        // GET /products/ - Get all products
        group
            .MapGet("/", async ([FromServices] IMediator mediator) =>
            {
                var query = new GetAllProductsQuery();
                return await mediator.Send(query);
            })
            .Produces<IEnumerable<ProductDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithSummary("Get all products")
            .WithDescription("Returns a list of all products in the system.");

        // GET /products/{id} - Get product by ID
        group
            .MapGet("/{id}", (IMediator mediator, [FromRoute] string id) =>
                mediator.Send(new GetProductByIdQuery(id))
            )
            .Produces<ProductDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get product by ID")
            .WithDescription("Returns the details of a specific product by its unique ID.");

        // POST /products/ - Create product
        group
            .MapPost("/", ([FromServices] IMediator mediator, [FromBody] CreateProductCommand command) =>
                mediator.Send(command)
            )
            .Produces<ProductDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Create a new product")
            .WithDescription("Creates a new product with the provided details.");

        // PUT /products/ - Update product
        group
            .MapPut("/", ([FromServices] IMediator mediator, [FromBody] UpdateProductCommand command) =>
                mediator.Send(command)
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update an existing product")
            .WithDescription("Updates the details of an existing product.");

        // DELETE /products/ - Delete products
        group
            .MapDelete("/", ([FromServices] IMediator mediator, [FromBody] DeleteProductCommand command) =>
                mediator.Send(command)
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Delete products by IDs")
            .WithDescription("Deletes one or more products by their unique IDs.");

        // POST /products/pagination - Get with pagination
        group
            .MapPost("/pagination", ([FromServices] IMediator mediator, [FromBody] ProductsWithPaginationQuery query) =>
                mediator.Send(query)
            )
            .Produces<PaginatedResult<ProductDto>>(StatusCodes.Status200OK)
            .WithSummary("Get products with pagination")
            .WithDescription("Returns a paginated list of products based on search keywords, page size, and sorting options.");

        // GET /products/export - Export to CSV
        group
            .MapGet("/export", async ([FromQuery] string keywords, [FromServices] IMediator mediator) =>
            {
                var result = await mediator.Send(new ExportProductsQuery(keywords));
                result.Position = 0;
                return Results.File(result, "text/csv", "exported-products.csv");
            })
            .Produces(StatusCodes.Status200OK)
            .WithSummary("Export Products to CSV")
            .WithDescription("Exports the product data to a CSV file based on the provided keywords.");

        // POST /products/import - Import from CSV
        group
            .MapPost("/import", async (
                [FromForm] FileUploadRequest request,
                HttpContext context,
                [FromServices] IMediator mediator
            ) =>
            {
                var response = new List<FileUploadResponse>();
                foreach (var file in request.Files)
                {
                    // Validate file type
                    if (Path.GetExtension(file.FileName).ToLower() != ".csv")
                    {
                        logger.LogWarning($"Invalid file type: {file.FileName}");
                        return Results.BadRequest("Only CSV files are supported.");
                    }

                    // Copy file to memory stream
                    var filestream = file.OpenReadStream();
                    var stream = new MemoryStream();
                    await filestream.CopyToAsync(stream);
                    stream.Position = 0;
                    var fileSize = stream.Length;

                    // Send the file stream to ImportProductsCommand
                    var importCommand = new ImportProductsCommand(stream);
                    await mediator.Send(importCommand);

                    response.Add(new FileUploadResponse
                    {
                        Path = file.FileName,
                        Url = $"Imported {file.FileName}",
                        Size = fileSize,
                    });
                }

                return TypedResults.Ok(response);
            })
            .DisableAntiforgery()
            .Accepts<FileUploadRequest>("multipart/form-data")
            .Produces<IEnumerable<FileUploadResponse>>()
            .WithMetadata(new ConsumesAttribute("multipart/form-data"))
            .WithSummary("Import Products from CSV")
            .WithDescription("Imports product data from one or more CSV files.");
    }
}
```

**Ключевые особенности:**
- Minimal API pattern (ASP.NET Core)
- Mediator pattern для разделения API и бизнес-логики
- Полная документация с OpenAPI (Swagger)
- Поддержка экспорта/импорта CSV
- Authorization на весь группу endpoints

---

### 1.2 Stock (Складской запас)

#### Domain Layer - Сущность с отношениями

**Файл:** `src/CleanAspire.Domain/Entities/Stock.cs`

```csharp
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Represents a stock entity.
/// </summary>
public class Stock : BaseAuditableEntity, IAuditTrial
{
    /// <summary>
    /// Gets or sets the product ID (Foreign Key).
    /// </summary>
    public string? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product associated with the stock (Navigation Property).
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Gets or sets the quantity of the stock.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the location of the stock.
    /// </summary>
    public string Location { get; set; } = string.Empty;
}
```

**Ключевые особенности:**
- Foreign Key к Product (ProductId)
- Navigation Property для EF Core (Product)
- Nullable для поддержки optional relationships

---

#### API Endpoints - Domain-specific operations

```csharp
// POST /stocks/dispatch - Dispatching stock from warehouse
group
    .MapPost("/dispatch", ([FromServices] IMediator mediator, [FromBody] StockDispatchingCommand command) =>
        mediator.Send(command)
    )
    .Produces(StatusCodes.Status204NoContent)
    .WithSummary("Dispatch stock from warehouse")
    .WithDescription("Reduces stock quantity when dispatching products from warehouse.");

// POST /stocks/receive - Receiving stock to warehouse
group
    .MapPost("/receive", ([FromServices] IMediator mediator, [FromBody] StockReceivingCommand command) =>
        mediator.Send(command)
    )
    .Produces(StatusCodes.Status204NoContent)
    .WithSummary("Receive stock to warehouse")
    .WithDescription("Increases stock quantity when receiving products to warehouse.");

// POST /stocks/pagination - Get with pagination
group
    .MapPost("/pagination", ([FromServices] IMediator mediator, [FromBody] StocksWithPaginationQuery query) =>
        mediator.Send(query)
    )
    .Produces<PaginatedResult<StockDto>>(StatusCodes.Status200OK)
    .WithSummary("Get stocks with pagination");
```

**Ключевые особенности:**
- Domain-specific операции (dispatch/receive) вместо generic CRUD
- Бизнес-логика инкапсулирована в Commands

#### Application Layer - Commands

**Файл:** `src/CleanAspire.Application/Features/Stocks/Commands/StockDispatchingCommand.cs`

```csharp
public record StockDispatchingCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Location { get; set; } = string.Empty;
    public IEnumerable<string>? Tags => new[] { "stocks" };
}

public class StockDispatchingCommandHandler : IRequestHandler<StockDispatchingCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public async ValueTask<Unit> Handle(
        StockDispatchingCommand request,
        CancellationToken cancellationToken
    )
    {
        // Validate that the product exists
        var product = await _context.Products.FirstOrDefaultAsync(
            p => p.Id == request.ProductId,
            cancellationToken
        );

        if (product == null)
        {
            throw new KeyNotFoundException(
                $"Product with Product ID '{request.ProductId}' was not found."
            );
        }

        // Check if the stock record exists for the given ProductId and Location
        var existingStock = await _context.Stocks.FirstOrDefaultAsync(
            s => s.ProductId == request.ProductId && s.Location == request.Location,
            cancellationToken
        );

        if (existingStock == null)
        {
            throw new KeyNotFoundException(
                $"No stock record found for Product ID '{request.ProductId}' at Location '{request.Location}'."
            );
        }

        // Validate that the stock quantity is sufficient
        if (existingStock.Quantity < request.Quantity)
        {
            throw new InvalidOperationException(
                $"Insufficient stock quantity. Available: {existingStock.Quantity}, Requested: {request.Quantity}"
            );
        }

        // Reduce the stock quantity
        existingStock.Quantity -= request.Quantity;

        // If stock quantity is zero, remove the stock record
        if (existingStock.Quantity == 0)
        {
            _context.Stocks.Remove(existingStock);
        }
        else
        {
            // Update the stock record
            _context.Stocks.Update(existingStock);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
```

**Файл:** `src/CleanAspire.Application/Features/Stocks/Commands/StockReceivingCommand.cs`

```csharp
public record StockReceivingCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Location { get; set; } = string.Empty;
    public IEnumerable<string>? Tags => new[] { "stocks" };
}

public class StockReceivingCommandHandler : IRequestHandler<StockReceivingCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public async ValueTask<Unit> Handle(
        StockReceivingCommand request,
        CancellationToken cancellationToken
    )
    {
        // Validate that the product exists
        var product = await _context.Products.FirstOrDefaultAsync(
            p => p.Id == request.ProductId,
            cancellationToken
        );

        if (product == null)
        {
            throw new KeyNotFoundException(
                $"Product with Product ID '{request.ProductId}' was not found."
            );
        }

        // Check if the stock record already exists for the given ProductId and Location
        var existingStock = await _context.Stocks.FirstOrDefaultAsync(
            s => s.ProductId == request.ProductId && s.Location == request.Location,
            cancellationToken
        );

        if (existingStock != null)
        {
            // If the stock record exists, update the quantity
            existingStock.Quantity += request.Quantity;
            _context.Stocks.Update(existingStock);
        }
        else
        {
            // If no stock record exists, create a new one
            var newStockEntry = new Stock
            {
                ProductId = request.ProductId,
                Location = request.Location,
                Quantity = request.Quantity,
            };

            _context.Stocks.Add(newStockEntry);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
```

#### Infrastructure Layer - EF Core Configuration

**Файл:** `src/CleanAspire.Infrastructure/Persistence/Configurations/StockConfiguration.cs`

```csharp
public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.Property(x => x.ProductId).HasMaxLength(50).IsRequired();

        builder
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Location).HasMaxLength(12).IsRequired();

        builder.Ignore(e => e.DomainEvents);
    }
}
```

---

### 1.3 Tenant (Мультитенанси)

#### Domain Layer - Simple Entity

**Файл:** `src/CleanAspire.Domain/Entities/Tenant.cs`

```csharp
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities;

public class Tenant : IEntity<string>
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    // GUID v7 для сортировки по времени создания
    public string Id { get; set; } = Guid.CreateVersion7().ToString();
}
```

**Ключевые особенности:**
- Использует GUID v7 (time-ordered UUID)
- Минимальная структура для демонстрации мультитенанси
- Реализует `IEntity<string>` для generic repositories

---

#### API - AllowAnonymous для публичных endpoints

```csharp
// GET /tenants/ - Get all organizations (AllowAnonymous)
group
    .MapGet("/", async ([FromServices] IMediator mediator) =>
    {
        var query = new GetAllTenantsQuery();
        return await mediator.Send(query);
    })
    .AllowAnonymous() // Публичный доступ для выбора организации при регистрации
    .Produces<IEnumerable<TenantDto>>(StatusCodes.Status200OK)
    .WithSummary("Get all organizations")
    .WithDescription("Returns a list of all available organizations.");

// GET /tenants/{id} - Get organization by ID (AllowAnonymous)
group
    .MapGet("/{id}", (IMediator mediator, [FromRoute] string id) =>
        mediator.Send(new GetTenantByIdQuery(id))
    )
    .AllowAnonymous()
    .Produces<TenantDto>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);
```

**Ключевые особенности:**
- AllowAnonymous для регистрации пользователей
- Защищенные endpoints для управления (Create/Update/Delete)

---

## 2. Регистрация пользователей (Sign Up Flow)

### 2.1 Backend - API Endpoint

**Файл:** `src/CleanAspire.Api/IdentityApiAdditionalEndpointsExtensions.cs`

```csharp
// POST /account/signup
identityGroup
    .MapPost("/signup", async Task<Results<Ok, ValidationProblem, ProblemHttpResult>> (
        [FromBody] SignupRequest request,
        [FromServices] IServiceProvider serviceProvider
    ) =>
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var emailSender = serviceProvider.GetRequiredService<IEmailSender<ApplicationUser>>();

        // Create user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            Nickname = request.Nickname,
            Provider = "Local",
            TenantId = request.TenantId,
            TimeZoneId = request.TimeZoneId,
            LanguageCode = request.LanguageCode
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return CreateValidationProblem(result);
        }

        // Send email confirmation
        await SendConfirmationEmailAsync(user, userManager, emailSender, request.Email);

        return TypedResults.Ok();
    })
    .AllowAnonymous()
    .WithOpenApi();

// Request DTO
public class SignupRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? Nickname { get; set; }
    public string Provider { get; set; } = "Local";
    public string? TenantId { get; set; }
    public string? TimeZoneId { get; set; }
    public string? LanguageCode { get; set; }
}
```

---

### 2.2 Frontend - Blazor Signup Page

**Файл:** `src/CleanAspire.ClientApp/Pages/Account/SignUp.razor`

```razor
@page "/account/signup"

@using System.ComponentModel.DataAnnotations
@using CleanAspire.ClientApp.Components.Autocompletes

<PageTitle>@L["Signup"]</PageTitle>
<MudPaper Elevation="3" Class="pa-8" Width="100%" MaxWidth="500px">
    <div class="d-flex flex-row align-center gap-3 my-3">
        <MudBlazorLogo Style="width:60px;height:60px"></MudBlazorLogo>
        <MudText Typo="Typo.h5">@L["Create an Account"]</MudText>
    </div>

    <div class="d-flex flex-column gap-2">
        <div class="d-flex flex-row gap-1">
            <MudText>@L["Have an account?"]</MudText>
            <MudLink Href="/account/signin">@L["Sign In"]</MudLink>
        </div>

        <EditForm Model="@model" OnValidSubmit="OnValidSubmit">
            <DataAnnotationsValidator />
            <div class="d-flex flex-column gap-2">
                <!-- Multi-tenant autocomplete -->
                <MultiTenantAutocomplete T="TenantDto"
                    For="(()=>model.Tenant)"
                    Placeholder="@L["Organization"]"
                    Label="@L["Select Organization"]"
                    Required="true"
                    RequiredError="@L["Organization selection is required"]"
                    @bind-Value="@model.Tenant">
                </MultiTenantAutocomplete>

                <MudTextField @bind-Value="model.Email"
                    For="@(() => model.Email)"
                    Label="@L["Email"]"
                    Placeholder="@L["Email"]"
                    Required="true"
                    RequiredError="@L["Email is required"]">
                </MudTextField>

                <MudTextField @bind-Value="model.Nickname"
                    For="@(() => model.Nickname)"
                    Label="@L["Nickname"]"
                    Placeholder="Nickname">
                </MudTextField>

                <PasswordInput Field="@(() => model.Password)"
                    @bind-Value="@model.Password"
                    Label="@L["Password"]"
                    Placeholder="@L["Password"]"
                    RequiredError="@L["Password is required"]" />

                <PasswordInput Field="@(() => model.ConfirmPassword)"
                    @bind-Value="@model.ConfirmPassword"
                    Label="@L["Confirm password"]"
                    Placeholder="@L["Confirm password"]"
                    RequiredError="@L["Confirm password is required"]" />

                <TimeZoneAutocomplete T="string"
                    For="@(() => model.TimeZoneId)"
                    @bind-Value="model.TimeZoneId"
                    Label="@L["Time Zone"]"
                    Placeholder="@L["Time Zone"]">
                </TimeZoneAutocomplete>

                <LanguageAutocomplete T="string"
                    For="@(() => model.LanguageCode)"
                    @bind-Value="model.LanguageCode"
                    Label="@L["Language"]"
                    Placeholder="@L["Language"]">
                </LanguageAutocomplete>

                <div class="d-flex flex-row align-center justify-space-between">
                    <MudCheckBox @bind-Value="model.CheckPrivacy"
                        For="@(() => model.CheckPrivacy)">
                        @L["I agree to the terms and privacy"]
                    </MudCheckBox>
                </div>

                <MudButton Disabled="@waiting"
                    ButtonType="ButtonType.Submit"
                    Color="Color.Primary">
                    @L["Signup"]
                </MudButton>
            </div>
        </EditForm>
    </div>
</MudPaper>

@code {
    private bool waiting = false;
    private SignupModel model = new();

    private async Task OnValidSubmit(EditContext context)
    {
        var online = await OnlineStatusInterop.GetOnlineStatusAsync();
        if (!online)
        {
            Snackbar.Add(L["You are offline. Please check your internet connection."], Severity.Error);
            return;
        }

        waiting = true;
        var result = await ApiClientServiceProxy.ExecuteAsync(async () =>
        {
            await ApiClient.Account.Signup.PostAsync(new SignupRequest()
            {
                Email = model.Email,
                Password = model.Password,
                LanguageCode = model.LanguageCode,
                Nickname = model.Nickname,
                Provider = "Local",
                TimeZoneId = model.TimeZoneId,
                TenantId = model.Tenant?.Id
            });
            return true;
        });

        result.Switch(
            ok =>
            {
                Snackbar.Add(L["Account created successfully. Please check your email to verify your account."], Severity.Success);
                Navigation.NavigateTo("/account/signupsuccessful");
                waiting = false;
            },
            invalid =>
            {
                Snackbar.Add(L[invalid.Detail ?? "Failed validation"], Severity.Error);
                waiting = false;
            },
            error =>
            {
                Snackbar.Add(error.Message, Severity.Error);
                waiting = false;
            });
    }

    public class SignupModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(30, ErrorMessage = "Password must be at least 6 characters long.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[\W_]).{6,}$",
            ErrorMessage = "Password must be at least 6 characters long and contain at least one letter, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the privacy policy.")]
        public bool CheckPrivacy { get; set; }

        [RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "Nickname can only contain letters, numbers, and underscores.")]
        [MaxLength(50, ErrorMessage = "Nickname cannot exceed 50 characters.")]
        public string? Nickname { get; set; }

        [MaxLength(50, ErrorMessage = "TimeZoneId cannot exceed 50 characters.")]
        public string? TimeZoneId { get; set; }

        [MaxLength(10, ErrorMessage = "LanguageCode cannot exceed 10 characters.")]
        public string? LanguageCode { get; set; }

        public string Provider { get; set; } = "Local";

        public TenantDto? Tenant { get; set; }
    }
}
```

**Ключевые особенности:**
- MudBlazor UI components
- Встроенная валидация (DataAnnotations)
- Offline detection
- Локализация (L["..."])
- Multi-tenant support

---

## 3. OAuth интеграция (Google & Microsoft)

### 3.1 Google OAuth Flow

#### Backend - Generate Login URL

```csharp
// GET /account/google/loginUrl
identityGroup
    .MapGet("/google/loginUrl", (
        [FromServices] IConfiguration configuration,
        HttpContext context
    ) =>
    {
        var clientId = configuration["Authentication:Google:ClientId"];
        var redirectUri = $"{context.Request.Scheme}://{context.Request.Host}/external-login";
        var state = Guid.NewGuid().ToString();
        var scope = "openid profile email";

        var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
            $"client_id={Uri.EscapeDataString(clientId)}&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
            $"response_type=code&" +
            $"scope={Uri.EscapeDataString(scope)}&" +
            $"state={state}";

        return TypedResults.Ok(new { url = authUrl });
    })
    .AllowAnonymous()
    .WithOpenApi();
```

#### Backend - Handle Callback

```csharp
// POST /account/google/signIn
identityGroup
    .MapPost("/google/signIn", async (
        [FromBody] GoogleSignInRequest request,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IConfiguration configuration,
        [FromServices] SignInManager<ApplicationUser> signInManager
    ) =>
    {
        var clientId = configuration["Authentication:Google:ClientId"];
        var clientSecret = configuration["Authentication:Google:ClientSecret"];

        // Exchange authorization code for access token
        var tokenResponse = await ExchangeCodeForTokenAsync(request.Code, clientId, clientSecret);

        // Get user info from Google
        var userInfo = await GetGoogleUserInfoAsync(tokenResponse.AccessToken);

        // Find or create user
        var user = await userManager.FindByEmailAsync(userInfo.Email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userInfo.Email,
                Email = userInfo.Email,
                Nickname = userInfo.Name,
                Provider = "Google",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user);
        }

        // Sign in user
        await signInManager.SignInAsync(user, isPersistent: true);

        return TypedResults.Ok(new { success = true });
    })
    .AllowAnonymous()
    .WithOpenApi();
```

#### Frontend - Callback Handler

**Файл:** `src/CleanAspire.ClientApp/Pages/Account/GoogleLoginCallback.razor`

```razor
@page "/external-login"
@inject NavigationManager Navigation
@inject ApiClient ApiClient
@inject ISnackbar Snackbar

@code {
    protected override async Task OnInitializedAsync()
    {
        var uri = new Uri(Navigation.Uri);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        if (query.TryGetValue("code", out var code))
        {
            try
            {
                await ApiClient.Account.Google.SignIn.PostAsync(new GoogleSignInRequest
                {
                    Code = code.ToString()
                });

                Snackbar.Add("Successfully signed in with Google!", Severity.Success);
                Navigation.NavigateTo("/");
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Google sign-in failed: {ex.Message}", Severity.Error);
                Navigation.NavigateTo("/account/signin");
            }
        }
        else if (query.TryGetValue("error", out var error))
        {
            Snackbar.Add($"Google authentication error: {error}", Severity.Error);
            Navigation.NavigateTo("/account/signin");
        }
    }
}
```

#### Frontend - Login Button

```razor
<!-- In SignIn.razor -->
<MudButton Variant="Variant.Outlined"
    Color="Color.Default"
    FullWidth="true"
    StartIcon="@Icons.Custom.Brands.Google"
    OnClick="OnGoogleLogin">
    @L["Login with Google"]
</MudButton>

@code {
    private async Task OnGoogleLogin()
    {
        try
        {
            var response = await ApiClient.Account.Google.LoginUrl.GetAsync();
            Navigation.NavigateTo(response.Url, forceLoad: true);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to initiate Google login: {ex.Message}", Severity.Error);
        }
    }
}
```

**Ключевые особенности:**
- OAuth 2.0 Authorization Code Flow
- Automatic user creation on first login
- Email auto-confirmation for OAuth users
- Provider tracking ("Google", "Microsoft", "Local")

---

### 3.2 Microsoft OAuth Flow

Аналогичная реализация с изменениями:

```csharp
// Microsoft OAuth endpoints
var authUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?" +
    $"client_id={clientId}&" +
    $"response_type=code&" +
    $"redirect_uri={redirectUri}&" +
    $"scope=openid profile email";

// Token endpoint
var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

// User info endpoint
var userInfoEndpoint = "https://graph.microsoft.com/v1.0/me";
```

**Конфигурация:**

```json
{
  "Authentication": {
    "Microsoft": {
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret",
      "TenantId": "common"
    }
  }
}
```

---

## 4. Двухфакторная аутентификация (2FA)

### 4.1 TOTP Authenticator Setup

```csharp
// POST /account/generateAuthenticator
identityGroup
    .MapPost("/generateAuthenticator", async (
        [FromServices] UserManager<ApplicationUser> userManager,
        ClaimsPrincipal user
    ) =>
    {
        var currentUser = await userManager.GetUserAsync(user);

        // Generate authenticator key
        var unformattedKey = await userManager.GetAuthenticatorKeyAsync(currentUser);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await userManager.ResetAuthenticatorKeyAsync(currentUser);
            unformattedKey = await userManager.GetAuthenticatorKeyAsync(currentUser);
        }

        var email = await userManager.GetEmailAsync(currentUser);

        // Generate QR code URI for authenticator apps
        var authenticatorUri = GenerateQrCodeUri(email, unformattedKey);

        return TypedResults.Ok(new
        {
            sharedKey = FormatKey(unformattedKey),
            authenticatorUri = authenticatorUri
        });
    })
    .RequireAuthorization();

private string GenerateQrCodeUri(string email, string unformattedKey)
{
    const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    return string.Format(
        AuthenticatorUriFormat,
        Uri.EscapeDataString("CleanAspire"),
        Uri.EscapeDataString(email),
        unformattedKey);
}
```

### 4.2 Enable 2FA

```csharp
// POST /account/enable2fa
identityGroup
    .MapPost("/enable2fa", async (
        [FromBody] Enable2faRequest request,
        [FromServices] UserManager<ApplicationUser> userManager,
        ClaimsPrincipal user
    ) =>
    {
        var currentUser = await userManager.GetUserAsync(user);

        // Verify the code
        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            currentUser,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            request.Code
        );

        if (!isValid)
        {
            return TypedResults.BadRequest("Invalid verification code");
        }

        await userManager.SetTwoFactorEnabledAsync(currentUser, true);

        return TypedResults.Ok(new { message = "2FA enabled successfully" });
    })
    .RequireAuthorization();
```

### 4.3 Disable 2FA

```csharp
// GET /account/disable2fa
routeGroup
    .MapGet(
        "/disable2fa",
        async Task<Results<Ok, NotFound, BadRequest>> (
            ClaimsPrincipal claimsPrincipal,
            HttpContext context
        ) =>
        {
            var userManager = context.RequestServices.GetRequiredService<
                UserManager<TUser>
            >();
            var logger = context
                .RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("Disable2FA");
            var user = await userManager.GetUserAsync(claimsPrincipal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }
            var isTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                return TypedResults.BadRequest();
            }
            var result = await userManager.SetTwoFactorEnabledAsync(user, false);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to disable 2FA");
                return TypedResults.BadRequest();
            }

            logger.LogInformation("User has disabled 2FA.");
            return TypedResults.Ok();
        }
    )
    .RequireAuthorization();
```

### 4.4 Login with 2FA

```csharp
// POST /account/login2fa
routeGroup
    .MapPost(
        "/login2fa",
        async Task<Results<Ok, ProblemHttpResult, NotFound>> (
            [FromBody] LoginRequest login,
            [FromQuery] bool? useCookies,
            [FromQuery] bool? useSessionCookies,
            HttpContext context
        ) =>
        {
            var signInManager = context.RequestServices.GetRequiredService<
                SignInManager<TUser>
            >();
            var userManager = context.RequestServices.GetRequiredService<
                UserManager<TUser>
            >();
            var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
            var isPersistent = (useCookies == true) && (useSessionCookies != true);
            signInManager.AuthenticationScheme = useCookieScheme
                ? IdentityConstants.ApplicationScheme
                : IdentityConstants.BearerScheme;

            var user = await userManager.FindByNameAsync(login.Email);
            if (user == null)
            {
                return TypedResults.NotFound();
            }

            var result = await signInManager.PasswordSignInAsync(
                login.Email,
                login.Password,
                isPersistent,
                lockoutOnFailure: true
            );

            if (result.RequiresTwoFactor)
            {
                if (!string.IsNullOrEmpty(login.TwoFactorCode))
                {
                    result = await signInManager.TwoFactorAuthenticatorSignInAsync(
                        login.TwoFactorCode,
                        isPersistent,
                        rememberClient: isPersistent
                    );
                }
                else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
                {
                    result = await signInManager.TwoFactorRecoveryCodeSignInAsync(
                        login.TwoFactorRecoveryCode
                    );
                }
            }

            if (!result.Succeeded)
            {
                return TypedResults.Problem(
                    result.ToString(),
                    statusCode: StatusCodes.Status401Unauthorized
                );
            }

            return TypedResults.Ok();
        }
    )
    .AllowAnonymous();
```

### 4.5 Generate Recovery Codes

```csharp
// GET /account/generateRecoveryCodes
routeGroup
    .MapGet(
        "generateRecoveryCodes",
        async Task<Results<Ok<RecoveryCodesResponse>, NotFound, BadRequest>> (
            ClaimsPrincipal claimsPrincipal,
            HttpContext context
        ) =>
        {
            var userManager = context.RequestServices.GetRequiredService<
                UserManager<TUser>
            >();
            var user = await userManager.GetUserAsync(claimsPrincipal);
            if (user is null)
            {
                return TypedResults.NotFound();
            }
            var isTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                return TypedResults.BadRequest();
            }
            int codeCount = 8;
            var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(
                user,
                codeCount
            );
            if (recoveryCodes is null)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.Ok(new RecoveryCodesResponse(recoveryCodes!));
        }
    )
    .RequireAuthorization();
```

### 4.6 Request/Response Models

```csharp
internal sealed record AuthenticatorResponse(string SharedKey, string AuthenticatorUri);

internal sealed record Enable2faRequest(string? AppName, string VerificationCode);

internal sealed record RecoveryCodesResponse(IEnumerable<string> Codes);
```

---

## 5. ApplicationUser расширения

### 5.1 Полная модель пользователя

```csharp
public class ApplicationUser : IdentityUser
{
    // Profile fields
    public string? Nickname { get; set; }
    public string? TimeZoneId { get; set; }
    public string? LanguageCode { get; set; }
    public string? AvatarUrl { get; set; }

    // OAuth provider tracking
    public string Provider { get; set; } = "Local"; // "Local", "Google", "Microsoft"

    // Multi-tenant support
    public string? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    // Hierarchical organization
    public string? SuperiorId { get; set; }
    public ApplicationUser? Superior { get; set; }

    // Refresh token (for JWT)
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
```

**Ключевые особенности:**
- Расширение стандартного IdentityUser
- Multi-tenant через TenantId
- OAuth provider tracking
- Hierarchical users (Superior/Subordinate)
- Refresh token для JWT authentication

---

## 6. Рекомендации по реализации в новых проектах

### 6.1 Clean Architecture Layers

```
Domain Layer (Core)
├── Entities (Product, Stock, Tenant)
├── Enums (ProductCategory)
└── Interfaces (IEntity, IAuditTrial)

Application Layer
├── Features
│   ├── Products
│   │   ├── Commands (Create, Update, Delete, Import)
│   │   ├── Queries (GetAll, GetById, Pagination, Export)
│   │   ├── DTOs (ProductDto)
│   │   └── EventHandlers (ProductCreatedEvent)
│   ├── Stocks
│   └── Tenants
└── Common
    ├── Models (PaginatedResult)
    └── Interfaces (IApplicationDbContext)

Infrastructure Layer
├── Persistence
│   ├── Configurations (ProductConfiguration)
│   └── ApplicationDbContext
└── Services (EmailSender, etc.)

API/Presentation Layer
├── Endpoints (ProductEndpointRegistrar)
└── Extensions (IdentityApiExtensions)
```

### 6.2 Ключевые паттерны

1. **CQRS (Command Query Responsibility Segregation)**
   - Commands для изменения данных
   - Queries для чтения данных
   - Разделение ответственности

2. **Mediator Pattern**
   - Разделение API от бизнес-логики
   - Упрощение тестирования
   - Централизованная обработка запросов

3. **Repository Pattern**
   - Абстракция доступа к данным
   - `IApplicationDbContext` вместо прямого DbContext

4. **Domain Events**
   - Event-driven architecture
   - Асинхронная обработка side effects

5. **DTO (Data Transfer Objects)**
   - Разделение domain models от API models
   - Защита от over-posting

### 6.3 Лучшие практики

1. **Валидация**
   - FluentValidation или DataAnnotations
   - Валидация на уровне Command/Query
   - `IRequiresValidation` marker interface

2. **Кэширование**
   - FusionCache для distributed caching
   - Cache invalidation через Tags
   - `IFusionCacheRefreshRequest<T>` interface

3. **Аудит**
   - `IAuditTrial` interface
   - Автоматическое отслеживание изменений
   - Created/Modified timestamps

4. **Локализация**
   - Resource files для переводов
   - `IStringLocalizer` для backend
   - `L["key"]` для Blazor components

5. **Безопасность**
   - RequireAuthorization по умолчанию
   - AllowAnonymous только где необходимо
   - Provider tracking для OAuth users

---

## 7. Database Migrations

### 7.1 Создание миграции для Product

```bash
dotnet ef migrations add AddProduct --project src/CleanAspire.Infrastructure --startup-project src/CleanAspire.Api
```

### 7.2 Удаление сущностей

```bash
# Создать миграцию для удаления
dotnet ef migrations add RemoveDemoEntities --project src/CleanAspire.Infrastructure --startup-project src/CleanAspire.Api

# Применить миграцию
dotnet ef database update --project src/CleanAspire.Infrastructure --startup-project src/CleanAspire.Api
```

---

## Заключение

Все удаляемые компоненты следуют Clean Architecture принципам:
- Разделение ответственности (Separation of Concerns)
- Dependency Inversion (зависимости направлены к Domain)
- Testability (легко тестируемые компоненты)
- SOLID принципы

Документация сохранена для возможного восстановления функционала или использования как reference implementation в новых проектах.

---

**Автор:** Claude Code Assistant
**Дата:** 2026-01-15
**Проект:** CleanAspire
**Версия документа:** 1.0
