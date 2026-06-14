FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["global.json", "."]
COPY ["src/ScopelyBattles.Api/ScopelyBattles.Api.csproj", "src/ScopelyBattles.Api/"]
COPY ["src/ScopelyBattles.Shared/ScopelyBattles.Shared.csproj", "src/ScopelyBattles.Shared/"]

RUN dotnet restore "src/ScopelyBattles.Api/ScopelyBattles.Api.csproj"

COPY . .

RUN dotnet publish "src/ScopelyBattles.Api/ScopelyBattles.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ScopelyBattles.Api.dll"]
