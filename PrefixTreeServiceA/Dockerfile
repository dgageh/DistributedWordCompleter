# Stage 1: Build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["PrefixTreeServiceA/PrefixTreeServiceA.csproj", "PrefixTreeServiceA/"]
COPY ["TrieLibrary/TrieLibrary.csproj", "TrieLibrary/"]
RUN dotnet restore "PrefixTreeServiceA/PrefixTreeServiceA.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR /src/PrefixTreeServiceA
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish the app
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PrefixTreeServiceA.dll"]
