// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Security.Claims;
using CleanAspire.Application.Common.Services;

namespace CleanAspire.Infrastructure.Services;

/// <summary>
/// Represents the current user context, holding session information.
/// </summary>
public class CurrentUserContext : ICurrentUserContext
{
    private static AsyncLocal<ClaimsPrincipal?> _currentUser = new AsyncLocal<ClaimsPrincipal?>();

    public ClaimsPrincipal? GetCurrentUser() => _currentUser.Value;

    public void Set(ClaimsPrincipal? user) => _currentUser.Value = user;

    public void Clear() => _currentUser.Value = null;
}
