# Steeltoe Initializr 

Master: ![image](https://dev.azure.com/SteeltoeOSS/Steeltoe/_apis/build/status/SteeltoeOSS.initializr?branchName=master)
Dev: ![image](https://dev.azure.com/SteeltoeOSS/Steeltoe/_apis/build/status/SteeltoeOSS.initializr?branchName=dev)

Steeltoe Initializr provides an extensible API to generate quickstart projects. It provides a simple web UI to configure the project to generate and endpoints that you can use via plain HTTP.

Steeltoe Initializr also exposes an endpoint that serves its metadata in a well-known
format to allow third-party clients to provide the necessary assistance.

# How to use
## Web
You can see a demo of app running on [Pivotal Web Services](https://startsteeltoe.cfapps.io).

The Web UI allows you to quickly generate a CSharp project with your choice of dependencies

 ![image](https://media.giphy.com/media/IdP0OiDeK0dTLIW1Qe/giphy.gif)


## Curl
```
curl https://startsteeltoe.cfapps.io/starter.zip -d dependencies=actuators,cloudfoundry -o myProject.zip

or

curl https://startsteeltoe.cfapps.io/starter.zip -d dependencies=actuators,cloudfoundry -d templateShortName=react -d projectName=MyCompany.MySample -o myProject.zip
```

To get a list of dependencies:
```
curl https://start.steeltoe.io/api/templates/dependencies
```

To get a list of valid templates:
```
curl https://startsteeeltoe.cfapps.io/api/templates/templates
```
## Dotnet templates
Install the Steeloe Templates 
```
dotnet new -i steeltoe.templates::2.2.1 --nuget-source https://www.myget.org/F/steeltoedev/api/v3/index.json
```
Generate project
```
dotnet new Steeltoe-WebApi --Actuators --CloudFoundry
```
# Developing 

Clone and cd into repo and :
``` dotnet build
    dotnet test 
    cd src 
    dotnet run
```

