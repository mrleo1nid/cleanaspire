// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Products.Commands;

namespace CleanAspire.Application.Features.Products.Validators;

/// <summary>
/// Validator for DeleteProductCommand.
/// Uses FluentValidation to apply validation rules for deleting products.
/// </summary>
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    /// <summary>
    /// Initializes validation rules for the IDs of products to be deleted.
    /// </summary>
    public DeleteProductCommandValidator()
    {
        // Validate that the IDs collection is not empty
        RuleFor(command => command.Ids)
            .NotEmpty()
            .WithMessage("At least one product ID is required.")
            .Must(ids => ids != null && ids.All(id => !string.IsNullOrWhiteSpace(id)))
            .WithMessage("Product IDs must not be empty or whitespace.");
    }
}
