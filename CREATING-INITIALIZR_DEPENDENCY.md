## Creating an Initializr Dependency

Initializr uses a .NET templating engine called [Stubble](https://github.com/StubbleOrg/Stubble), an implementation of [Mustache](http://mustache.github.com/). 

When adding a new 3rdparty library dependency, the templating allows a lot of flexibility.  There are many areas project that can be modified when your dependency has been selected and your project has been generated.

Typical files that are modified:

+ [Mustache.json](using-mustache%2E.json) - Describe the dependency parameters that the template engine will use to replace
+ [ReplaceMe.csproj](using-replaceme%2Ecsproj) - Adding `PackageReference` to `csproj` file
+ [Startup.cs](using-startup%2Ecs) - Setting your services up to use the dependency
+ [Program.cs](using-program%2Ecs) - Adding the `Using` statements and webhost configuration

Other files that can be modified:

+ Root (/) Directory
  - [Dockerfile](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/Dockerfile)
  - [app.config](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/app.config)
  - [appseetings.json](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/appsettings.json) 
+ Controllers
  - [ValuesController.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/Controllers/ValuesController.cs)
+ Models
  - [ErrorViewModel.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/Models/ErrorViewModel.cs)
  -  [InitializeContext.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/Models/InitializeContext.cs)
  -  [SampleData.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/Models/SampleData.cs)
  -  [TestContext.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/Models/TestContext.cs)
+ Properties
  - [launchSettings.json](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/Properties/launchSettings.json) 
  

### Using Mustache.json
The file [mustache.json](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/mustache.json) handles several areas related to the templating:

+ Describing the dependency under the `Params` section
+ Grouping of dependencies by using `CalculatedParams`
+ Setting conditional inclusions and exclusions
+ Specifying `Versions` for the dependencies

### Using ReplaceMe.csproj
The file [ReplaceMe.csproj](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/ReplaceMe.csproj) is used to add the `PackageReference`(s) for the dependency.

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
The file [Startup.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/Startup.cs) is used to generate the Startup.cs file based on the selected dependencies. 

Some options include adding:

+ Required `using` statements
+ Services to add to the [IServiceCollection](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)
+ Application configuration to the [IApplicationBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.iapplicationbuilder)

### Using Program.cs
The file [Program.cs](https://github.com/SteeltoeOSS/Initializr/blob/dev/src/templates/Mustache/3.x/Steeltoe-React/Program.cs) is used configure your `WebHostBuilder` with your selected dependency.

Some options include adding:

+ Required `using` statements
+ Configuration to the [WebHostBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.hosting.webhostbuilder) 
