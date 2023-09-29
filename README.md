# Innovian.Telnyx.Storage

This is an unofficial .NET library for the Telnyx Storage API, targetting .NET 7.

## Installation
Using the .NET Core CLI tools:
```sh
dotnet add package Innovian.Telnyx.Storage
```

Using the NuGet CLI:
```sh
nuget install Innovian.Telnyx.Storage
```

Using the Package Manager Console:
```powershell
Install-Package Innovian.Telnyx.Storage
```

From within Visual Studio:

1. Open the Solution Explorer.
2. Right-click on a project within your solution.
3. Click on "Manage NuGet Packages...".
4. Click on the "Browse" tab and search for "Innovian.Telnyx.Storage".
5. Click on the Innovian.Telnyx.Storage package, select the appropriate version in the right-tab and click *Install*.

## Usage

### Authentication
The Telnyx authenticates requests using the API key generated from within your Telnyx account. When logged into Telnyx, click on "Account Settings" in the left-nav to expand it, then select "Keys & Credentials" from the list that appears. You'll be looking at a page title "API Keys". Either use an existing and active 
API key by clicking the "copy" button to the right of any of the keys listed in the table or click the green button labeled "Create API Key" in the top right of the page, then click Create and Copy to generate and copy the newly created API key to your clipboard. This API key typically starts with "KEY..."

Any of these keys can be used with this library or the official Telnyx libraries for the other functionality without restriction.

Dependency injection using top-level statements:
```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTelnyxClient(opt =>
{
  opt.TelnyxApiKey = "KEY..."; //Insert the Telnyx API key here
});

//...
var app = builder.Build();
//...
app.Run();
```

Dependency injection via `Startup.cs` registration:
```cs
//Startup.cs

public void ConfigureServices(IServiceCollection services)
{
  services.AddTelnyxClient(opt =>
  {
    opt.TelnyxApiKey = "KEY..."; //Insert the Telnyx API key here
  });
}
```

Manual DI registration (used if you need access to the service provider context to obtain the API key):
```cs
// Assumes use of Startup.cs wherein `services` is an `IServiceCollection`.
// If using top-level statements, replace `services` with `builder.Services`
services.AddHttpClient(); //Mandatory so IHttpClientFactory is registered - handled for you in the above extension registrations
services.AddSingleton(c => {
  var apiKey = ""; //Retrieve this from somewhere
  var httpFactory = c.GetRequiredService<IHttpClientFactory>();
  return new TelnyxStorageService(httpFactory, apiKey);
});
```

You can register this library via Autofac as well by installing the [Autofac.Extensions.DependencyInjection](https://www.nuget.org/packages/Autofac.Extensions.DependencyInjection) package to your project first, then using the following snippet:
```cs
public static class CompositionRoot
{
  public static IContainer Builder()
  {
    var builder = new ContainerBuilder();
    //...
    var services = new ServiceCollection();
    services.AddTelnyxClient(opt => {
      opt.TelnyxApiKey = ""; //Retrieve this from somewhere
    });
    builder.Populate(services);
    return builder.Build();
  }
}
```

Finally, this library also supports [delegate factories](https://docs.autofac.org/en/latest/advanced/delegate-factories.html) in case you want to register with [Autofac](https://www.nuget.org/packages/Autofac) and need to acquire the API key at a later point. This example uses the [Autofac.Extensions.DependencyInjection](https://www.nuget.org/packages/Autofac.Extensions.DependencyInjection) package to simplify registering the `IHttpClientFactory`.
```cs
//CompositionRoot.cs
public class MyExample
{
  var builder = new ContainerBuilder();

  //Register the TelnyxStorageService
  builder.RegisterType<TelnyxStorageService>();

  //Register the IHttpClientFactory
  var services = new ServiceCollection();
  services.AddHttpClient();
  builder.Populate(services);
  using var container = builder.Build();

  //Create a local scope and retrieve the factory from the Autofac container
  using var scope = container.BeginLifetimeScope();
  var createStorageClient = scope.Resolve<TelnyxStorageService.Factory>();

  //Autofac automatically injects the IHttpClientFactory for us, we just need to pass in the API key
  var telnyxApiKey = ""; //Retrieve this from somewhere
  var telnyxClient = createStorageClient(telnyxApiKey); //Returns an ITelnyxStorageService ready to use
}
```

## Example usage
To use the service, inject the `ITelnyxStorageService` interface into the constructor of the type you're using it in as in the following:
```cs
public class MyClass
{
  private readonly ITelnyxStorageService _storageService;

  public MyClass (ITelnyxStorageService storageService)
  {
    _storageService = storageService;
  }

  public async Task DoSoemthingAsync()
  {
    //Do something...
  }
}
```

Here's an example showing injection into another class after it's been registered via Autofac's delegate factory approach:
```cs
public class MyOtherClass
{
  private readonly ITelenyxStorageService _telnyxClient;

  public MyOtherClass(TelnyxStorageService.Factory telnyxFactory)
  {
    var telnyxApiKey = ""; //Retrieve this from somewhere
    _telnyxClient = telnyxFactory(telnyxApiKey);
  }

  public async Task DoSomethingAsync()
  {
    //Do something...
  }
}
```

## Running unit tests
The unit tests most run through one integrated test which iterates through all the functionality in the library. It requires an active API key to run through the tests and will briefly incur charges (though it's likely to fit well within the free usage threshold). Your API key will need to be stored in an environment variable named "TelnyxApiKey" for the test runner to pick it up. In order for the tests to pass, the bucket name **must** not be in use. The tests use a GUID as part of the name generation, so this generally shouldn't be a problem, but heads up nonetheless.

## Contributing
Contributions are welcome. 

## License
Innovian.Telnyx.Storage is MIT licensed.
