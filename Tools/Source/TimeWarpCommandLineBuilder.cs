namespace System.CommandLine.Builder
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Tools;

  internal class TimeWarpCommandLineBuilder : CommandLineBuilder
  {
    private static ServiceProvider ServiceProvider { get; set; }

    private IServiceCollection ServiceCollection { get; set; }

    private XmlDocReader XmlDocReader { get; }

    public TimeWarpCommandLineBuilder(Command aRootCommand = null)
                    : base(aRootCommand ?? new RootCommand())
    {
      var startup = new Startup();
      ServiceCollection = new ServiceCollection();

      startup.ConfigureServices(ServiceCollection);
      startup.Configure(this);

      ServiceCollection.AddMediatR();

      ServiceProvider = ServiceCollection.BuildServiceProvider();

      string xmlPath = Assembly.GetEntryAssembly().Location.Replace(".dll", ".xml");
      XmlDocReader = new XmlDocReader(xmlPath);

      UseMediatorCommands();
    }

    public TimeWarpCommandLineBuilder UseMediatorCommands()
    {
      // Get all serviceDescriptors that implement IRequestHandler
      string iRequestHandlerName = typeof(IRequestHandler<>).Name;
      const char GenericBackTic = '`';
      iRequestHandlerName = iRequestHandlerName.Substring(0, iRequestHandlerName.IndexOf(GenericBackTic));

      IEnumerable<ServiceDescriptor> serviceDescriptors = ServiceCollection.Where(aServiceDescriptor =>
        aServiceDescriptor.ServiceType.IsConstructedGenericType && aServiceDescriptor.Lifetime == ServiceLifetime.Transient &&
        aServiceDescriptor.ServiceType.Name.Contains(iRequestHandlerName) &&
        aServiceDescriptor.ServiceType.IsVisible);

      // Add Command for each IRequest Handled
      foreach (ServiceDescriptor serviceDescriptor in serviceDescriptors)
      {
        BuildCommandForRequest(serviceDescriptor.ServiceType.GenericTypeArguments[0]);
      }
      return this;
    }

    private void BuildCommandForRequest(Type aType)
    {
      var command = new Command
      (
        name: aType.Name.Replace("Request", ""),
        description: XmlDocReader.GetDescriptionForType(aType),
        handler: new MediatorCommandHandler(aType, ServiceProvider.GetService<IMediator>())
      );

      // Add command Options from properties
      foreach (PropertyInfo propertyInfo in aType.GetProperties())
      {
        var option = new Option(
          alias: $"--{propertyInfo.Name}",
          description: XmlDocReader.GetDescriptionForPropertyInfo(propertyInfo),
          argument: CreateGenericArgument(propertyInfo.PropertyType),
          isHidden: false);
        command.AddOption(option);
      }

      this.AddCommand(command);
    }

    private Argument CreateGenericArgument(Type aPropertyType)
    {
      Type argumentType = typeof(Argument<>);
      Type genericArgumentType = argumentType.MakeGenericType(aPropertyType);
      return Activator.CreateInstance(genericArgumentType) as Argument;
    }
  }
}