FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["global.json", "."]
COPY ["src/ScopelyBattles.Worker/ScopelyBattles.Worker.csproj", "src/ScopelyBattles.Worker/"]
COPY ["src/ScopelyBattles.Shared/ScopelyBattles.Shared.csproj", "src/ScopelyBattles.Shared/"]

RUN dotnet restore "src/ScopelyBattles.Worker/ScopelyBattles.Worker.csproj"

COPY . .

RUN dotnet publish "src/ScopelyBattles.Worker/ScopelyBattles.Worker.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ScopelyBattles.Worker.dll"]
