// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Products.EventHandlers;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Products.Commands;

// Command object that encapsulates the IDs of products to be deleted.
public record DeleteProductCommand(params IEnumerable<string> Ids)
    : IFusionCacheRefreshRequest<Unit>,
        IRequiresValidation
{
    public IEnumerable<string>? Tags => new[] { "products" };
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteProductCommandHandler(
        ILogger<DeleteProductCommandHandler> logger,
        IApplicationDbContext dbContext
    )
    {
        _dbContext = dbContext;
    }

    public async ValueTask<Unit> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken
    )
    {
        var products = _dbContext.Products.Where(p => request.Ids.Contains(p.Id));

        foreach (var product in products)
        {
            product.AddDomainEvent(new ProductDeletedEvent(product));
            _dbContext.Products.Remove(product);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
