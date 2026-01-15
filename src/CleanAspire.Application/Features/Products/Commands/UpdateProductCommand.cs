// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Products.EventHandlers;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Products.Commands;

// Command object that encapsulates the data required to update a product.
// Each field corresponds to a property in ProductDto.
public record UpdateProductCommand(
    string Id,
    string SKU,
    string Name,
    ProductCategory Category,
    string? Description,
    decimal Price,
    string? Currency,
    string? UOM
) : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public IEnumerable<string>? Tags => new[] { "products" };
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken
    )
    {
        var product = await _context.Products.FindAsync(
            new object[] { request.Id },
            cancellationToken
        );
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with Id '{request.Id}' was not found.");
        }

        product.SKU = request.SKU;
        product.Name = request.Name;
        product.Category = request.Category;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Currency = request.Currency;
        product.UOM = request.UOM;

        product.AddDomainEvent(new ProductUpdatedEvent(product));
        _context.Products.Update(product);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
