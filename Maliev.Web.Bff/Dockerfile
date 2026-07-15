# syntax=docker/dockerfile:1.7

ARG dependency_restore_stage=restore-private
ARG shared_library_version
ARG messaging_contracts_version
ARG app_version
ARG commit_sha

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-base
ARG shared_library_version
ARG messaging_contracts_version
ARG app_version
ARG commit_sha
WORKDIR /src
RUN : "${shared_library_version:?shared_library_version build argument is required}" \
    && : "${messaging_contracts_version:?messaging_contracts_version build argument is required}" \
    && : "${app_version:?app_version build argument is required}" \
    && : "${commit_sha:?commit_sha build argument is required}"

COPY ["nuget.config", "."]
COPY ["NuGet.PRValidation.Config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Maliev.Web.Bff/Maliev.Web.Bff.csproj", "Maliev.Web.Bff/"]
COPY ["Maliev.Web.Client/Maliev.Web.Client.csproj", "Maliev.Web.Client/"]
COPY ["Maliev.Web.Shared/Maliev.Web.Shared.csproj", "Maliev.Web.Shared/"]

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
COPY [".ci-packages/", ".ci-packages/"]
RUN dotnet restore "Maliev.Web.Bff/Maliev.Web.Bff.csproj" \
      --configfile "NuGet.PRValidation.Config" \
      /p:GITHUB_ACTIONS=true \
      /p:SharedLibraryVersion="$shared_library_version" \
      /p:MessagingContractsVersion="$messaging_contracts_version"

FROM build-base AS restore-attested
RUN --mount=type=secret,id=ci_packages,required=true,target=/tmp/ci-packages.tar \
    mkdir -p .ci-packages \
    && tar -xf /tmp/ci-packages.tar -C .ci-packages \
    && dotnet restore "Maliev.Web.Bff/Maliev.Web.Bff.csproj" \
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
    /p:Version="$app_version" \
    /p:SourceRevisionId="$commit_sha" \
    /p:ContinuousIntegrationBuild=true \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
ARG app_version
ARG commit_sha
WORKDIR /app
COPY --chown=app:app --from=build /app/publish .

LABEL org.opencontainers.image.version="$app_version" \
      org.opencontainers.image.revision="$commit_sha" \
      org.opencontainers.image.source="https://github.com/MALIEV-Co-Ltd/Maliev.Web"

USER app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    BuildMetadata__Version="$app_version" \
    BuildMetadata__CommitSha="$commit_sha" \
    DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "Maliev.Web.Bff.dll"]
