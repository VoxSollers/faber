FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Api
COPY ["src/api/Faber.Api/Faber.Api.csproj", "src/api/Faber.Api/"]

# Aspire
COPY ["src/api/Aspire/Faber.ServiceDefaults/Faber.ServiceDefaults.csproj", "src/api/Aspire/Faber.ServiceDefaults/"]

# Auth
COPY ["src/api/Modules/Auth/Faber.Modules.Auth.Domain/Faber.Modules.Auth.Domain.csproj", "src/api/Modules/Auth/Faber.Modules.Auth.Domain/"]
COPY ["src/api/Modules/Auth/Faber.Modules.Auth.Application/Faber.Modules.Auth.Application.csproj", "src/api/Modules/Auth/Faber.Modules.Auth.Application/"]
COPY ["src/api/Modules/Auth/Faber.Modules.Auth.PublicApi/Faber.Modules.Auth.PublicApi.csproj", "src/api/Modules/Auth/Faber.Modules.Auth.PublicApi/"]

# Common
COPY ["src/api/Modules/Common/Faber.Modules.Common.PublicApi/Faber.Modules.Common.PublicApi.csproj", "src/api/Modules/Common/Faber.Modules.Common.PublicApi/"]

# Documents
COPY ["src/api/Modules/Documents/Faber.Modules.Documents.Application/Faber.Modules.Documents.Application.csproj", "src/api/Modules/Documents/Faber.Modules.Documents.Application/"]
COPY ["src/api/Modules/Documents/Faber.Modules.Documents.PublicApi/Faber.Modules.Documents.PublicApi.csproj", "src/api/Modules/Documents/Faber.Modules.Documents.PublicApi/"]

# Identity
COPY ["src/api/Modules/Identity/Faber.Modules.Identity.Domain/Faber.Modules.Identity.Domain.csproj", "src/api/Modules/Identity/Faber.Modules.Identity.Domain/"]
COPY ["src/api/Modules/Identity/Faber.Modules.Identity.Application/Faber.Modules.Identity.Application.csproj", "src/api/Modules/Identity/Faber.Modules.Identity.Application/"]
COPY ["src/api/Modules/Identity/Faber.Modules.Identity.Infrastructure/Faber.Modules.Identity.Infrastructure.csproj", "src/api/Modules/Identity/Faber.Modules.Identity.Infrastructure/"]
COPY ["src/api/Modules/Identity/Faber.Modules.Identity.PublicApi/Faber.Modules.Identity.PublicApi.csproj", "src/api/Modules/Identity/Faber.Modules.Identity.PublicApi/"]

# Notifications
COPY ["src/api/Modules/Notifications/Faber.Modules.Notifications.Application/Faber.Modules.Notifications.Application.csproj", "src/api/Modules/Notifications/Faber.Modules.Notifications.Application/"]
COPY ["src/api/Modules/Notifications/Faber.Modules.Notifications.PublicApi/Faber.Modules.Notifications.PublicApi.csproj", "src/api/Modules/Notifications/Faber.Modules.Notifications.PublicApi/"]

# Resumes
COPY ["src/api/Modules/Resumes/Faber.Modules.Resumes.Domain/Faber.Modules.Resumes.Domain.csproj", "src/api/Modules/Resumes/Faber.Modules.Resumes.Domain/"]
COPY ["src/api/Modules/Resumes/Faber.Modules.Resumes.Application/Faber.Modules.Resumes.Application.csproj", "src/api/Modules/Resumes/Faber.Modules.Resumes.Application/"]
COPY ["src/api/Modules/Resumes/Faber.Modules.Resumes.Infrastructure/Faber.Modules.Resumes.Infrastructure.csproj", "src/api/Modules/Resumes/Faber.Modules.Resumes.Infrastructure/"]
COPY ["src/api/Modules/Resumes/Faber.Modules.Resumes.PublicApi/Faber.Modules.Resumes.PublicApi.csproj", "src/api/Modules/Resumes/Faber.Modules.Resumes.PublicApi/"]

# Users
COPY ["src/api/Modules/Users/Faber.Modules.Users.Domain/Faber.Modules.Users.Domain.csproj", "src/api/Modules/Users/Faber.Modules.Users.Domain/"]
COPY ["src/api/Modules/Users/Faber.Modules.Users.Application/Faber.Modules.Users.Application.csproj", "src/api/Modules/Users/Faber.Modules.Users.Application/"]
COPY ["src/api/Modules/Users/Faber.Modules.Users.PublicApi/Faber.Modules.Users.PublicApi.csproj", "src/api/Modules/Users/Faber.Modules.Users.PublicApi/"]

# Vault
COPY ["src/api/Modules/Vault/Faber.Modules.Vault.Application/Faber.Modules.Vault.Application.csproj", "src/api/Modules/Vault/Faber.Modules.Vault.Application/"]
COPY ["src/api/Modules/Vault/Faber.Modules.Vault.PublicApi/Faber.Modules.Vault.PublicApi.csproj", "src/api/Modules/Vault/Faber.Modules.Vault.PublicApi/"]

RUN dotnet restore "src/api/Faber.Api/Faber.Api.csproj"
COPY . .
WORKDIR "/src/src/api/Faber.Api"
RUN dotnet build "Faber.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Install PowerShell
RUN wget -q "https://github.com/PowerShell/PowerShell/releases/download/v7.6.6/powershell_7.6.6-1.deb_amd64.deb" && \
    dpkg -i powershell_7.6.6-1.deb_amd64.deb || true && \
    apt-get update && \
    apt-get install -f -y && \
    rm powershell_7.6.6-1.deb_amd64.deb

# Install Playwright browsers using PowerShell script
RUN pwsh /app/build/playwright.ps1 install

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Faber.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    libnss3 \
    libnspr4 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libasound2 \
    libpango-1.0-0 \
    libcairo2 \
    libatspi2.0-0 \
    fonts-liberation \
    xvfb \
    xauth \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright

# Ensure proper permissions
RUN chmod -R 777 /app
RUN chmod -R 777 /root/.cache/ms-playwright
ENTRYPOINT ["dotnet", "Faber.Api.dll"]
