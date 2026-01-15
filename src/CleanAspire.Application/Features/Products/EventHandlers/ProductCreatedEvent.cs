// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain;
using CleanAspire.Domain.Entities;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CleanAspire.Application.Features.Products.EventHandlers;

/// <summary>
/// Represents an event triggered when a product is created.
/// Purpose:
/// 1. To signal the creation of a product.
/// 2. Used in the domain event notification mechanism to pass product details to subscribers.
/// </summary>
public class ProductCreatedEvent : DomainEvent
{
    /// <summary>
    /// Constructor to initialize the event and pass the created product instance.
    /// </summary>
    /// <param name="item">The created product instance.</param>
    public ProductCreatedEvent(Product item)
    {
        Item = item; // Assigns the provided product instance to the read-only property
    }

    /// <summary>
    /// Gets the product instance associated with the event.
    /// </summary>
    public Product Item { get; }
}

/// <summary>
/// Handler for ProductCreatedEvent domain events.
/// Logs when a product is created.
/// </summary>
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductCreatedEventHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the ProductCreatedEvent notification.
    /// </summary>
    /// <param name="notification">The product created event notification.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public ValueTask Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handled domain event '{EventType}' with notification: {@Notification}",
            notification.GetType().Name,
            notification
        );
        return ValueTask.CompletedTask;
    }
}
