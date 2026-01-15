// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Application.Features.Products.DTOs;

// A DTO representing a product, used to transfer data between application layers.
// By default, field names match the corresponding entity fields. For enums or referenced entities, a Dto suffix is used.
public class ProductDto
{
    public string Id { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; } = 0;
    public string? Currency { get; set; }
    public string? UOM { get; set; }
}

// An enumeration representing possible product categories.
