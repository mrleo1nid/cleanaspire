// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Domain.Common;

public interface IMustHaveTenant
{
    string TenantId { get; set; }
}

public interface IMayHaveTenant
{
    string? TenantId { get; set; }
}
