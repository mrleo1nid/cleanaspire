// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.JSInterop;

namespace CleanAspire.ClientApp.Services.JsInterop;

public sealed class DownloadFileInterop(IJSRuntime jsRuntime)
{
    public async Task DownloadFileFromStream(string fileName, DotNetStreamReference stream)
    {
        await jsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, stream);
    }
}
