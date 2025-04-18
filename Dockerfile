FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy everything at once
COPY . .

# Print all project files to see the exact paths
RUN find . -name "*.csproj"

# Restore as distinct layers
RUN dotnet restore

# Build and publish directly from the solution
# This approach builds all projects and publishes the startup project
RUN dotnet publish -c Release -o /app

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "SnapnFix.Api.dll"]