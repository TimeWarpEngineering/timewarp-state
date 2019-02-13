namespace Microsoft.AspNetCore.Builder
{
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Components.Server;
  using Microsoft.AspNetCore.StaticFiles;
  using Microsoft.Extensions.FileProviders;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Net.Http.Headers;
  using System.Net.Mime;
  using System.Reflection;
  using Microsoft.AspNetCore.Http.Headers;


  /// <summary>
  /// Provides extension methods that add Blazor-related middleware to the ASP.NET pipeline.
  /// </summary>
  public static class BlazorDualModeApplicationBuilderExtensions
  {

    /// <summary>
    /// Configures the middleware pipeline to work with Blazor.
    /// </summary>
    /// <typeparam name="TProgram">Any type from the client app project. This is used to identify the client app assembly.</typeparam>
    /// <param name="aApplicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
    /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
    public static IApplicationBuilder UseBlazorDualMode<TProgram>(
        this IApplicationBuilder aApplicationBuilder)
    {
      Assembly clientAssemblyInServerBinDir = typeof(TProgram).Assembly;
      return aApplicationBuilder.UseBlazorDualMode(new BlazorOptions
      {
        ClientAssemblyPath = clientAssemblyInServerBinDir.Location,
      })
      .UseRazorComponents<TProgram>();
    }

    /// <summary>
    /// Configures the middleware pipeline to work with Blazor.
    /// </summary>
    /// <param name="aApplicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
    /// <param name="aBlazorOptions">Options to configure the middleware.</param>
    /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
    public static IApplicationBuilder UseBlazorDualMode(
        this IApplicationBuilder aApplicationBuilder,
        BlazorOptions aBlazorOptions)
    {
      aApplicationBuilder.UseStaticFiles();
      // TODO: Make the .blazor.config file contents sane
      // Currently the items in it are bizarre and don't relate to their purpose,
      // hence all the path manipulation here. We shouldn't be hardcoding 'dist' here either.
      var env = (IHostingEnvironment)aApplicationBuilder.ApplicationServices.GetService(typeof(IHostingEnvironment));
      var config = BlazorConfig.Read(aBlazorOptions.ClientAssemblyPath);

      // The rebuild stuff is private and I dont' want to pull in all the source.
      // I frankly never get the auto stuff to work anyway
      //if (env.IsDevelopment() && config.EnableAutoRebuilding)
      //{
      //  if (env.ApplicationName.Equals(DevServerApplicationName, StringComparison.OrdinalIgnoreCase))
      //  {
      //    app.UseDevServerAutoRebuild(config);
      //  }
      //  else
      //  {
      //    //app.UseHostedAutoRebuild(config, env.ContentRootPath);
      //  }
      //}

      // First, match the request against files in the client app dist directory
      aApplicationBuilder.UseStaticFiles(new StaticFileOptions
      {
        FileProvider = new PhysicalFileProvider(config.DistPath),
        ContentTypeProvider = CreateContentTypeProvider(config.EnableDebugging),
        OnPrepareResponse = SetCacheHeaders
      });

      // * Before publishing, we serve the wwwroot files directly from source
      //   (and don't require them to be copied into dist).
      //   In this case, WebRootPath will be nonempty if that directory exists.
      // * After publishing, the wwwroot files are already copied to 'dist' and
      //   will be served by the above middleware, so we do nothing here.
      //   In this case, WebRootPath will be empty (the publish process sets this).
      if (!string.IsNullOrEmpty(config.WebRootPath))
      {
        aApplicationBuilder.UseStaticFiles(new StaticFileOptions
        {
          FileProvider = new PhysicalFileProvider(config.WebRootPath),
          OnPrepareResponse = SetCacheHeaders
        });
      }

      return aApplicationBuilder;
    }

    internal static void SetCacheHeaders(StaticFileResponseContext aStaticFileResponseContext)
    {
      // By setting "Cache-Control: no-cache", we're allowing the browser to store
      // a cached copy of the response, but telling it that it must check with the
      // server for modifications (based on Etag) before using that cached copy.
      // Longer term, we should generate URLs based on content hashes (at least
      // for published apps) so that the browser doesn't need to make any requests
      // for unchanged files.
      ResponseHeaders headers = aStaticFileResponseContext.Context.Response.GetTypedHeaders();
      if (headers.CacheControl == null)
      {
        headers.CacheControl = new CacheControlHeaderValue
        {
          NoCache = true
        };
      }
    }

    private static IContentTypeProvider CreateContentTypeProvider(bool aEnableDebugging)
    {
      var result = new FileExtensionContentTypeProvider();
      AddMapping(result, ".dll", MediaTypeNames.Application.Octet);

      if (aEnableDebugging)
      {
        AddMapping(result, ".pdb", MediaTypeNames.Application.Octet);
      }

      return result;
    }

    private static void AddMapping(FileExtensionContentTypeProvider aProvider, string aName, string aMimeType)
    {
      if (!aProvider.Mappings.ContainsKey(aName))
      {
        aProvider.Mappings.Add(aName, aMimeType);
      }
    }
  }
}
