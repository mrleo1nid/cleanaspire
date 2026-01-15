// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.OpenApi;

namespace CleanAspire.Api;

public static class OpenApiTransformersExtensions
{
    public static OpenApiOptions UseCookieAuthentication(this OpenApiOptions options)
    {
        // Temporarily disabled OpenAPI security scheme wiring due to OpenAPI.NET v2 API changes.
        // Documentation generation will still work without explicit cookie auth scheme.
        return options;
    }
    // Examples transformer removed for .NET 10 RC1. If you need example payloads, reintroduce
    // a schema transformer targeting the updated OpenAPI model in Microsoft.AspNetCore.OpenApi.
}
