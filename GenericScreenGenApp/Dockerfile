# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files first for layer caching on restore
COPY GenericScreenGenSln.sln .
COPY GenericScreenGenApp/GenericScreenGenApp.csproj GenericScreenGenApp/
COPY GenericScreenGenFactoryLib/GenericScreenGenFactoryLib.csproj GenericScreenGenFactoryLib/
COPY GenericScreenGenImplementationsLib/GenericScreenGenImplementationsLib.csproj GenericScreenGenImplementationsLib/
COPY GenericScreenGenInterfacesLib/GenericScreenGenInterfacesLib.csproj GenericScreenGenInterfacesLib/
COPY GenericScreenGenUtilsLib/GenericScreenGenUtilsLib.csproj GenericScreenGenUtilsLib/
COPY MyDataStoreProviders/MyDataStoreProviders.csproj MyDataStoreProviders/
COPY MyTestConsoleApp/MyTestConsoleApp.csproj MyTestConsoleApp/

RUN dotnet restore GenericScreenGenApp/GenericScreenGenApp.csproj

# Copy source files
COPY GenericScreenGenApp/ GenericScreenGenApp/
COPY GenericScreenGenFactoryLib/ GenericScreenGenFactoryLib/
COPY GenericScreenGenImplementationsLib/ GenericScreenGenImplementationsLib/
COPY GenericScreenGenInterfacesLib/ GenericScreenGenInterfacesLib/
COPY GenericScreenGenUtilsLib/ GenericScreenGenUtilsLib/
COPY MyDataStoreProviders/ MyDataStoreProviders/

# Publish
RUN dotnet publish GenericScreenGenApp/GenericScreenGenApp.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5074

ENTRYPOINT ["dotnet", "GenericScreenGenApp.dll"]
