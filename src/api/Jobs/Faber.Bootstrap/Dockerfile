FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/api/Jobs/Faber.Bootstrap/Faber.Bootstrap.csproj", "src/api/Jobs/Faber.Bootstrap/"]

RUN dotnet restore "src/api/Jobs/Faber.Bootstrap/Faber.Bootstrap.csproj"

COPY . .

WORKDIR "/src/src/api/Jobs/Faber.Bootstrap"
RUN dotnet publish "Faber.Bootstrap.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Faber.Bootstrap.dll"]
