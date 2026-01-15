// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Products.DTOs;
using CleanAspire.Application.Features.Stocks.DTOs;

namespace CleanAspire.Application.Features.Stocks.Queryies;

public record StocksWithPaginationQuery(
    string Keywords,
    int PageNumber = 0,
    int PageSize = 15,
    string OrderBy = "Id",
    string SortDirection = "Descending"
) : IFusionCacheRequest<PaginatedResult<StockDto>>
{
    public IEnumerable<string>? Tags => new[] { "stocks" };
    public string CacheKey =>
        $"stockswithpagination_{Keywords}_{PageNumber}_{PageSize}_{OrderBy}_{SortDirection}";
}

public class StocksWithPaginationQueryHandler
    : IRequestHandler<StocksWithPaginationQuery, PaginatedResult<StockDto>>
{
    private readonly IApplicationDbContext _context;

    public StocksWithPaginationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<PaginatedResult<StockDto>> Handle(
        StocksWithPaginationQuery request,
        CancellationToken cancellationToken
    )
    {
        var data = await _context
            .Stocks.Include(x => x.Product)
            .OrderBy(request.OrderBy, request.SortDirection)
            .ProjectToPaginatedDataAsync(
                condition: x =>
                    x.Location.Contains(request.Keywords)
                    || (
                        x.Product != null
                        && (
                            x.Product.Name.Contains(request.Keywords)
                            || x.Product.SKU.Contains(request.Keywords)
                            || (
                                x.Product.Description != null
                                && x.Product.Description.Contains(request.Keywords)
                            )
                        )
                    ),
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                mapperFunc: t =>
                {
                    var productId = t.ProductId ?? string.Empty;
                    var product = t.Product;
                    return new StockDto
                    {
                        Id = t.Id,
                        ProductId = productId,
                        Product =
                            product != null
                                ? new ProductDto
                                {
                                    Category = product.Category,
                                    Currency = product.Currency,
                                    Description = product.Description,
                                    Id = product.Id,
                                    Name = product.Name,
                                    Price = product.Price,
                                    SKU = product.SKU,
                                    UOM = product.UOM,
                                }
                                : new ProductDto
                                {
                                    Category = ProductCategory.Electronics,
                                    Currency = null,
                                    Description = null,
                                    Id = string.Empty,
                                    Name = string.Empty,
                                    Price = 0,
                                    SKU = string.Empty,
                                    UOM = null,
                                },
                        Quantity = t.Quantity,
                        Location = t.Location,
                    };
                },
                cancellationToken: cancellationToken
            );
        return data;
    }
}
