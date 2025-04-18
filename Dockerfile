FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy csproj files and restore dependencies
COPY *.sln .
COPY SnapnFix/*.csproj ./SnapnFix/
COPY SnapNFix.Domain/*.csproj ./SnapNFix.Domain/
COPY SnapNFix.Application/*.csproj ./SnapNFix.Application/
COPY SnapNFix.Infrastructure/*.csproj ./SnapNFix.Infrastructure/

RUN dotnet restore

# Copy everything else and build the project
COPY . .
WORKDIR /source/SnapnFix
RUN dotnet publish -c Release -o /app

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "SnapnFix.dll"]