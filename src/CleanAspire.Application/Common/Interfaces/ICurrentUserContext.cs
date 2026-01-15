// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Security.Claims;

namespace CleanAspire.Application.Common.Services;

/// <summary>
/// Interface representing the current user context.
/// </summary>
public interface ICurrentUserContext
{
    ClaimsPrincipal? GetCurrentUser();

    void Set(ClaimsPrincipal? user);
    void Clear();
}
