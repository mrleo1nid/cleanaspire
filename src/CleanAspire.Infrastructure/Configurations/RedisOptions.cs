// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Infrastructure.Configurations;

public class RedisOptions
{
    public const string Key = "Redis";
    public string ConnectionString { get; set; } = "localhost:6379";
    public bool Enabled { get; set; } = true;
}
