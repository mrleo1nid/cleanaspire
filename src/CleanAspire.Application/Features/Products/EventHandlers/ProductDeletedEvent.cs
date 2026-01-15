// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain;
using CleanAspire.Domain.Entities;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CleanAspire.Application.Features.Products.EventHandlers;

/// <summary>
/// Represents an event triggered when a product is deleted.
/// Purpose:
/// 1. To signal the deletion of a product.
/// 2. Used in the domain event notification mechanism to inform subscribers about the deleted product.
/// </summary>
public class ProductDeletedEvent : DomainEvent
{
    /// <summary>
    /// Constructor to initialize the event and pass the deleted product instance.
    /// </summary>
    /// <param name="item">The deleted product instance.</param>
    public ProductDeletedEvent(Product item)
    {
        Item = item; // Assigns the provided product instance to the read-only property
    }

    /// <summary>
    /// Gets the product instance associated with the event.
    /// </summary>
    public Product Item { get; }
}

/// <summary>
/// Handler for ProductDeletedEvent domain events.
/// Logs when a product is deleted.
/// </summary>
public class ProductDeletedEventHandler : INotificationHandler<ProductDeletedEvent>
{
    private readonly ILogger<ProductDeletedEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductDeletedEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ProductDeletedEventHandler(ILogger<ProductDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the ProductDeletedEvent notification.
    /// </summary>
    /// <param name="notification">The product deleted event notification.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public ValueTask Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handled domain event '{EventType}' with notification: {@Notification}",
            notification.GetType().Name,
            notification
        );
        return ValueTask.CompletedTask;
    }
}
