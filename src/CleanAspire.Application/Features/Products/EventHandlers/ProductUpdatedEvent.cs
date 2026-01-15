// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain;
using CleanAspire.Domain.Entities;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CleanAspire.Application.Features.Products.EventHandlers;

/// <summary>
/// Represents an event triggered when a product is updated.
/// Purpose:
/// 1. To signal that a product has been updated.
/// 2. Used in the domain event notification mechanism to inform subscribers about the updated product details.
/// </summary>
public class ProductUpdatedEvent : DomainEvent
{
    /// <summary>
    /// Constructor to initialize the event and pass the updated product instance.
    /// </summary>
    /// <param name="item">The updated product instance.</param>
    public ProductUpdatedEvent(Product item)
    {
        Item = item; // Assigns the provided product instance to the read-only property
    }

    /// <summary>
    /// Gets the product instance associated with the event.
    /// </summary>
    public Product Item { get; }
}

/// <summary>
/// Handler for ProductUpdatedEvent domain events.
/// Logs when a product is updated.
/// </summary>
public class ProductUpdatedEventHandler : INotificationHandler<ProductUpdatedEvent>
{
    private readonly ILogger<ProductUpdatedEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductUpdatedEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ProductUpdatedEventHandler(ILogger<ProductUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the ProductUpdatedEvent notification.
    /// </summary>
    /// <param name="notification">The product updated event notification.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public ValueTask Handle(ProductUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handled domain event '{EventType}' with notification: {@Notification}",
            notification.GetType().Name,
            notification
        );
        return ValueTask.CompletedTask;
    }
}
