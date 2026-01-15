// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.JSInterop;

namespace CleanAspire.ClientApp.Services.JsInterop;

public sealed class Swiper
{
    private readonly IJSRuntime _jsRuntime;

    public Swiper(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<ValueTask> Initialize(string elment, bool reverse = false)
    {
        var jsmodule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            "/js/carousel.js"
        );
        return jsmodule.InvokeVoidAsync("initializeSwiper", elment, reverse);
    }
}
