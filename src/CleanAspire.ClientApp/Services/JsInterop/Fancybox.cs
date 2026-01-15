// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.JSInterop;

namespace CleanAspire.ClientApp.Services.JsInterop;

public sealed class Fancybox
{
    private readonly IJSRuntime _jsRuntime;

    public Fancybox(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<ValueTask> Preview(string defaultUrl, IEnumerable<string> images)
    {
        var jsmodule = await _jsRuntime
            .InvokeAsync<IJSObjectReference>("import", "/js/fancybox.js")
            .ConfigureAwait(false);
        return jsmodule.InvokeVoidAsync("filepreview", defaultUrl, images);
    }
}
