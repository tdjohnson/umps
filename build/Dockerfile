# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy remaining source code and publish the application
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published app from build stage
COPY --from=build /app/publish .

# Expose port 80
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "umps.dll"]