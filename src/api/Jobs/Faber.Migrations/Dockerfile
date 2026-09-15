FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/api/Jobs/Faber.Migrations/Faber.Migrations.csproj", "src/api/Jobs/Faber.Migrations/"]
COPY ["src/api/Aspire/Faber.ServiceDefaults/Faber.ServiceDefaults.csproj", "src/api/Aspire/Faber.ServiceDefaults/"]
COPY ["src/api/Modules/Resumes/Faber.Modules.Resumes.Infrastructure/Faber.Modules.Resumes.Infrastructure.csproj", "src/api/Modules/Resumes/Faber.Modules.Resumes.Infrastructure/"]
COPY ["src/api/Modules/Resumes/Faber.Modules.Resumes.Domain/Faber.Modules.Resumes.Domain.csproj", "src/api/Modules/Resumes/Faber.Modules.Resumes.Domain/"]
COPY ["src/api/Modules/Identity/Faber.Modules.Identity.Infrastructure/Faber.Modules.Identity.Infrastructure.csproj", "src/api/Modules/Identity/Faber.Modules.Identity.Infrastructure/"]
COPY ["src/api/Modules/Identity/Faber.Modules.Identity.Domain/Faber.Modules.Identity.Domain.csproj", "src/api/Modules/Identity/Faber.Modules.Identity.Domain/"]

RUN dotnet restore "src/api/Jobs/Faber.Migrations/Faber.Migrations.csproj"

COPY . .

WORKDIR "/src/src/api/Jobs/Faber.Migrations"
RUN dotnet publish "Faber.Migrations.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Faber.Migrations.dll"]
