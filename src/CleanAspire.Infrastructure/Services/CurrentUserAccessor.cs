// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Security.Claims;
using CleanArchitecture.Blazor.Infrastructure.Extensions;
using CleanAspire.Application.Common.Services;

namespace CleanAspire.Infrastructure.Services;

/// <summary>
/// Provides access to the current user's session information.
/// </summary>
public class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserAccessor"/> class.
    /// </summary>
    /// <param name="currentUserContext">The current user context.</param>
    public CurrentUserAccessor(ICurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Gets the session information of the current user.
    /// </summary>
    public ClaimsPrincipal? User => _currentUserContext.GetCurrentUser();

    public string? UserId => User?.GetUserId();
}
