# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy solution file
COPY *.sln .

# Copy each project file to its respective directory
COPY SnapNFix.Api/*.csproj ./SnapNFix.Api/
COPY SnapNFix.Domain/*.csproj ./SnapNFix.Domain/
COPY SnapNFix.Application/*.csproj ./SnapNFix.Application/
COPY SnapNFix.Infrastructure/*.csproj ./SnapNFix.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app ./

# Set environment variables
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose the port
EXPOSE 10000

# Set the entry point
ENTRYPOINT ["dotnet", "SnapNFix.Api.dll"]