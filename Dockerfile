FROM selenium/standalone-chrome:87.0 AS base

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build

ARG BUILD_NUMBER
ARG NUGET_TOKEN
ARG NUGET_URL
RUN echo "Build Version: $BUILD_NUMBER"

WORKDIR /src
COPY . .
RUN dotnet nuget add source "$NUGET_URL" --name gitlab --username gitlab-ci-token --password $NUGET_TOKEN --store-password-in-clear-text
RUN dotnet restore ./src/Riveet.Prerender/Riveet.Prerender.csproj
RUN dotnet build ./src/Riveet.Prerender/Riveet.Prerender.csproj /p:VersionNumber=$BUILD_NUMBER -c Release -o /app

FROM build AS publish

ARG BUILD_NUMBER
RUN echo "Publishing Version: $BUILD_NUMBER"

WORKDIR /src
RUN dotnet publish ./src/Riveet.Prerender/Riveet.Prerender.csproj /p:VersionNumber=$BUILD_NUMBER -c Release -r linux-x64 -o /app

FROM base AS final
WORKDIR /app

COPY --from=publish /app .

ENTRYPOINT ["./Riveet.Prerender"]
