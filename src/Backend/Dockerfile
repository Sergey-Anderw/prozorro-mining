FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json nuget.config ./
COPY src/Backend/ProzorroMining.Api/ProzorroMining.Api.csproj src/Backend/ProzorroMining.Api/
COPY src/Backend/ProzorroMining.App/ProzorroMining.App.csproj src/Backend/ProzorroMining.App/
COPY src/Backend/ProzorroMining.Contracts/ProzorroMining.Contracts.csproj src/Backend/ProzorroMining.Contracts/
COPY src/Backend/ProzorroMining.DbMigrator/ProzorroMining.DbMigrator.csproj src/Backend/ProzorroMining.DbMigrator/
COPY src/Backend/ProzorroMining.Domain/ProzorroMining.Domain.csproj src/Backend/ProzorroMining.Domain/
COPY src/Backend/ProzorroMining.Infrastructure/ProzorroMining.Infrastructure.csproj src/Backend/ProzorroMining.Infrastructure/

RUN dotnet restore src/Backend/ProzorroMining.Api/ProzorroMining.Api.csproj
RUN dotnet restore src/Backend/ProzorroMining.DbMigrator/ProzorroMining.DbMigrator.csproj

COPY src ./src

RUN dotnet publish src/Backend/ProzorroMining.Api/ProzorroMining.Api.csproj -c Release -o /out/api /p:UseAppHost=false
RUN dotnet publish src/Backend/ProzorroMining.DbMigrator/ProzorroMining.DbMigrator.csproj -c Release -o /out/dbmigrator /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api
WORKDIR /app
COPY --from=build /out/api .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProzorroMining.Api.dll"]

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS dbmigrator
WORKDIR /app
COPY --from=build /out/dbmigrator .
ENTRYPOINT ["dotnet", "ProzorroMining.DbMigrator.dll"]
