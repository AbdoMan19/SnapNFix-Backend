FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy everything at once
COPY . .

# List directories to debug
RUN ls -la

# Restore as distinct layers
RUN dotnet restore "SnapNFix-Backend.sln"

# Build and publish the API project directly using its csproj file
RUN dotnet publish "SnapnFix.Api/SnapnFix.Api.csproj" -c Release -o /app

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "SnapnFix.Api.dll"]