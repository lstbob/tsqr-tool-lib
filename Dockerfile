FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Build tsqr-common from source (avoids GitHub Packages auth)
COPY --from=common . ./tsqr-common
RUN rm -f ./tsqr-common/nuget.config && \
    dotnet pack ./tsqr-common/src/TSQR.Common/TSQR.Common.csproj -c Release -o /tmp/nupkg && \
    dotnet pack ./tsqr-common/src/TSQR.Common.WebApi/TSQR.Common.WebApi.csproj -c Release -o /tmp/nupkg

# Use a clean NuGet configuration inside the image. The repository config
# points at GitHub Packages, which requires a token that the build does not
# have; the TSQR.Common packages are restored from the locally packed feed instead.
RUN printf '%s\n' \
    '<?xml version="1.0" encoding="utf-8"?>' \
    '<configuration>' \
    '  <packageSources>' \
    '    <clear />' \
    '    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />' \
    '    <add key="local-common" value="/tmp/nupkg" />' \
    '  </packageSources>' \
    '</configuration>' > /src/nuget.config
COPY TSQR.ToolLibrary.sln .
COPY src/TSQR.ToolLibrary.WebApi/ ./src/TSQR.ToolLibrary.WebApi/
COPY src/TSQR.ToolLibrary.Application/ ./src/TSQR.ToolLibrary.Application/
COPY src/TSQR.ToolLibrary.Domain/ ./src/TSQR.ToolLibrary.Domain/
COPY src/TSQR.ToolLibrary.Infrastructure/ ./src/TSQR.ToolLibrary.Infrastructure/

RUN dotnet restore ./src/TSQR.ToolLibrary.WebApi/TSQR.ToolLibrary.WebApi.csproj
RUN dotnet publish ./src/TSQR.ToolLibrary.WebApi/TSQR.ToolLibrary.WebApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y curl --no-install-recommends && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TSQR.ToolLibrary.WebApi.dll"]
