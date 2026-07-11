# syntax=docker/dockerfile:1.7

ARG dependency_restore_stage=restore-private

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-base
WORKDIR /src

COPY ["nuget.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Maliev.Web.Bff/Maliev.Web.Bff.csproj", "Maliev.Web.Bff/"]
COPY ["Maliev.Web.Client/Maliev.Web.Client.csproj", "Maliev.Web.Client/"]
COPY ["Maliev.Web.Shared/Maliev.Web.Shared.csproj", "Maliev.Web.Shared/"]
COPY [".ci-packages/", "/ci-packages/"]

FROM build-base AS restore-private
RUN --mount=type=secret,id=nuget_username,required=true \
    --mount=type=secret,id=nuget_password,required=true \
    NUGET_USERNAME="$(cat /run/secrets/nuget_username)" \
    NUGET_PASSWORD="$(cat /run/secrets/nuget_password)" \
    dotnet restore "Maliev.Web.Bff/Maliev.Web.Bff.csproj" \
      /p:GITHUB_ACTIONS=true \
      /p:SharedLibraryVersion="1.0.81-alpha"

FROM build-base AS restore-local
RUN dotnet restore "Maliev.Web.Bff/Maliev.Web.Bff.csproj" \
      --source "https://api.nuget.org/v3/index.json" \
      --source "/ci-packages" \
      /p:GITHUB_ACTIONS=true \
      /p:SharedLibraryVersion="1.0.81-alpha"

FROM ${dependency_restore_stage} AS build
COPY . .
RUN dotnet publish "Maliev.Web.Bff/Maliev.Web.Bff.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:GITHUB_ACTIONS=true \
    /p:SharedLibraryVersion="1.0.81-alpha" \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --chown=app:app --from=build /app/publish .

USER app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "Maliev.Web.Bff.dll"]
