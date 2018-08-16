// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BlazorState.EndToEnd.Tests.Infrastructure.ServerFixtures;
using Xunit;
using Xunit.Abstractions;

namespace BlazorState.EndToEnd.Tests.Infrastructure
{
  public abstract class ServerTestBase<TServerFixture>
        : BrowserTestBase, IClassFixture<TServerFixture>
        where TServerFixture : ServerFixture
  {
    protected readonly TServerFixture _serverFixture;

    public ServerTestBase(BrowserFixture browserFixture, TServerFixture serverFixture, ITestOutputHelper output)
        : base(browserFixture, output)
    {
      _serverFixture = serverFixture;
    }

    public void Navigate(string relativeUrl, bool noReload = false)
    {
      var absoluteUrl = new Uri(_serverFixture.RootUri, relativeUrl);

      if (noReload)
      {
        string existingUrl = Browser.Url;
        if (string.Equals(existingUrl, absoluteUrl.AbsoluteUri, StringComparison.Ordinal))
        {
          return;
        }
      }

      Browser.Navigate().GoToUrl("about:blank");
      Browser.Navigate().GoToUrl(absoluteUrl);
    }
  }
}