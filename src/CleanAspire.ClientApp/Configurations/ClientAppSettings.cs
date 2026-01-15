// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.ClientApp.Configurations;

public class ClientAppSettings
{
    public const string KEY = nameof(ClientAppSettings);
    public string AppName { get; set; } = "Blazor Aspire";
    public string Version { get; set; } = "0.0.1";
    public string ServiceBaseUrl { get; set; } = "https://apiservice.blazorserver.com";
}
