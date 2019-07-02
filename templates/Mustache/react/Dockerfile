FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["webfrontend.csproj", "."]
RUN dotnet restore "{{ProjectNameSpace}}.csproj"
COPY . .
RUN dotnet build "{{ProjectNameSpace}}.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "{{ProjectNameSpace}}.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "{{ProjectNameSpace}}.dll"]