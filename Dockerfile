FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
# Désactiver ou rediriger le cache NuGet vers un dossier temporaire
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV NUGET_PACKAGES=/tmp/nuget-packages
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AkarangaBank.Web.csproj", "./"]
RUN dotnet restore "AkarangaBank.Web.csproj"
COPY . .
RUN dotnet build "AkarangaBank.Web.csproj" -c "$BUILD_CONFIGURATION" -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AkarangaBank.Web.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AkarangaBank.Web.dll"]
