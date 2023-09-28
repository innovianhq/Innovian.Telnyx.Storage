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
//Assumes use of Startup.cs wherein `services` is an `IServiceCollection`. If using top-level statements, replace `services` with `builder.Services`
services.AddHttpClient(); //Mandatory so IHttpClientFactory is registered - handled for you in the above extension registrations
services.AddSingleton(c => {
  //Obtain the API key somehow here
  //var apiKey = "";
  var httpFactory = c.GetRequiredService<IHttpClientFactory>();
  return new TelnyxStorageService(httpFactory, apiKey);
});
```
