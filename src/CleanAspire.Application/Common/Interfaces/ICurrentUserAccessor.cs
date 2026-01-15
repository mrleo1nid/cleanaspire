// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Security.Claims;

namespace CleanAspire.Application.Common.Services;

/// <summary>
/// Interface to access the current user's session information.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// Gets the current session information of the user.
    /// </summary>
    ClaimsPrincipal? User { get; }
    string? UserId { get; }
    string? TenantId { get; }
}
