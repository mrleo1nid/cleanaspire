// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Purpose:
// 1. **`IEndpointRegistrar` Interface**:
//    - Provides a contract for defining endpoint registration logic.
//    - Ensures consistency across all endpoint registration implementations by enforcing a common method (`RegisterRoutes`).

namespace CleanAspire.Api.Endpoints;

/// <summary>
/// Defines a contract for registering endpoint routes.
/// </summary>
public interface IEndpointRegistrar
{
    /// <summary>
    /// Registers the routes for the application.
    /// </summary>
    /// <param name="routes">The <see cref="IEndpointRouteBuilder"/> to add routes to.</param>
    void RegisterRoutes(IEndpointRouteBuilder routes);
}
