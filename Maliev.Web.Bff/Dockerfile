# syntax=docker/dockerfile:1.7

ARG dependency_restore_stage=restore-private
ARG shared_library_version
ARG messaging_contracts_version

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-base
ARG shared_library_version
ARG messaging_contracts_version
WORKDIR /src
RUN : "${shared_library_version:?shared_library_version build argument is required}" \
    && : "${messaging_contracts_version:?messaging_contracts_version build argument is required}"

COPY ["nuget.config", "."]
COPY ["NuGet.PRValidation.Config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Maliev.Web.Bff/Maliev.Web.Bff.csproj", "Maliev.Web.Bff/"]
COPY ["Maliev.Web.Client/Maliev.Web.Client.csproj", "Maliev.Web.Client/"]
COPY ["Maliev.Web.Shared/Maliev.Web.Shared.csproj", "Maliev.Web.Shared/"]
COPY [".ci-packages/", ".ci-packages/"]

FROM build-base AS restore-private
RUN --mount=type=secret,id=nuget_username,required=true \
    --mount=type=secret,id=nuget_password,required=true \
    NUGET_USERNAME="$(cat /run/secrets/nuget_username)" \
    NUGET_PASSWORD="$(cat /run/secrets/nuget_password)" \
    dotnet restore "Maliev.Web.Bff/Maliev.Web.Bff.csproj" \
      /p:GITHUB_ACTIONS=true \
      /p:SharedLibraryVersion="$shared_library_version" \
      /p:MessagingContractsVersion="$messaging_contracts_version"

FROM build-base AS restore-local
RUN dotnet restore "Maliev.Web.Bff/Maliev.Web.Bff.csproj" \
      --configfile "NuGet.PRValidation.Config" \
      /p:GITHUB_ACTIONS=true \
      /p:SharedLibraryVersion="$shared_library_version" \
      /p:MessagingContractsVersion="$messaging_contracts_version"

FROM ${dependency_restore_stage} AS build
COPY . .
RUN dotnet publish "Maliev.Web.Bff/Maliev.Web.Bff.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:GITHUB_ACTIONS=true \
    /p:SharedLibraryVersion="$shared_library_version" \
    /p:MessagingContractsVersion="$messaging_contracts_version" \
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
