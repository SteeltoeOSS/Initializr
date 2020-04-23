## Creating a 3rdparty Library Dependency

Initializr uses a .NET templating engine called [Stubble](https://github.com/StubbleOrg/Stubble), an implementation of [Mustache](http://mustache.github.com/). 

When adding a new 3rdparty library dependency, the templating allows a lot of flexibility. There are many areas project that can be modified when your dependency has been selected and your project has been generated.

**In order to add your library dependency `PackageReference` you need to update the following `.csproj` and `.json` files:**

+ [Mustache.json](#using-mustachejson) - Describe the dependency parameters that the template engine will use to replace
+ [ReplaceMe.csproj](#using-replacemecsproj) - Adding `PackageReference` to `.csproj` file

**Optional files that can be modified:**

+ [Startup.cs](#using-startupcs) - Setting your services up to use the dependency
+ [Program.cs](#using-programcs) - Adding the `Using` statements and webhost configuration

**Less typical files that can be modified:**

+ Root (/) Directory
  - [Dockerfile](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/Dockerfile)
  - [app.config](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/app.config)
  - [appsettings.json](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/appsettings.json) 
+ Controllers
  - [ValuesController.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/Controllers/ValuesController.cs)
+ Models
  - [ErrorViewModel.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/Models/ErrorViewModel.cs)
  -  [InitializeContext.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/Models/InitializeContext.cs)
  -  [SampleData.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/Models/SampleData.cs)
  -  [TestContext.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/Models/TestContext.cs)
+ Properties
  - [launchSettings.json](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/Properties/launchSettings.json) 
  

### Using Mustache.json
The file [mustache.json](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/mustache.json) handles several areas related to the templating:

+ Describing the dependency under the `Params` section
+ Grouping of dependencies by using `CalculatedParams`
+ Setting conditional inclusions and exclusions
+ Specifying `Versions` for the dependencies

### Using ReplaceMe.csproj
The file [ReplaceMe.csproj](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/ReplaceMe.csproj) is used to add the `PackageReference`(s) for the dependency.

>Note: For this explanation we will name our example project `MyFirstProject`.

This file is used to build `MyFirstProject.csproj` based on the selections made in Initializr.

In order to add a new 3rdparty library dependency to the Initializr, this file will need to contain a section to specify what `PackageReference`(s) needs to be added to the `.csproj` file.

For example, when creating an Initializr dependency template for `MySql` as a 3rdparty library you will see the following in the `ReplaceMe.csproj`:

```
{{#MySql}}
    <PackageReference Include="MySql.Data" Version="{{MySqlVersion}}" />
{{/MySql}}
```
>Note `MySql` tag is described in `mustache.json` file.

When `MySql` is selected from Initializr, the above snippet will be added to the `MyFirstProject.csproj` as noted below: 

```
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    ...
  </PropertyGroup>

  <ItemGroup >
    <!-- MySql Connector PackageReference -->
    <PackageReference Include="MySql.Data" Version="8.0.16" />
    <!-- End of MySql PackageReference -->

    <!-- Newtonsoft.Json is added to each project -->
    <PackageReference Include="Newtonsoft.Json" Version="12.0.2" />
    <!-- End of Newtonsoft.json PackageReference -->
    
    <!-- Steeltoe ConnectorCore is added when any connector is selected -->
    <PackageReference Include="Steeltoe.CloudFoundry.ConnectorCore"  Version="2.4.3"/>
    <!-- End of default connector PackageReference -->
    
  </ItemGroup>
</Project>
```
### Using Startup.cs
The file [Startup.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/Startup.cs) is used to generate the Startup.cs file based on the selected dependencies. 

Some options include adding:

+ Required `using` statements
+ Services to add to the [IServiceCollection](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)
+ Application configuration to the [IApplicationBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.iapplicationbuilder)

### Using Program.cs
The file [Program.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-WebApi/Program.cs) is used configure your `WebHostBuilder` with your selected dependency.

Some options include adding:

+ Required `using` statements
+ Configuration to the [WebHostBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.hosting.webhostbuilder) 
