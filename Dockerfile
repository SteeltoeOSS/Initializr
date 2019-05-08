FROM hananiel/dotnetcore2.2.101 
WORKDIR /app
COPY . ./

RUN dotnet restore

RUN dotnet publish InitializrApi.csproj -o out


#FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine AS runtime
#WORKDIR /app
#COPY --from=build /app/src/InitializrApi/out ./
COPY .templateengine /root/.templateengine/
RUN dotnet new console -o /root/test.zip
EXPOSE 8080
ENTRYPOINT ["dotnet", "out/InitializrApi.dll"]
