# Use the .NET SDK image to build the library
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./", "TrieLibrary/"]
RUN dotnet build "./TrieLibrary/TrieLibrary.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=build /app/build .