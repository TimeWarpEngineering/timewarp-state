// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
namespace BlazorState.Server
{
  using Microsoft.AspNetCore;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;

  public class Program
  {
    public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseConfiguration(new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build())
            .UseStartup<Startup>()
            .Build();

    public static void Main(string[] args) => BuildWebHost(args).Run();
  }
}