// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanAspire.Infrastructure.Persistence;

public class BlazorContextFactory<TContext> : IDbContextFactory<TContext>
    where TContext : DbContext
{
    private readonly IServiceProvider _provider;

    public BlazorContextFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public TContext CreateDbContext()
    {
        var scope = _provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TContext>();
    }
}
