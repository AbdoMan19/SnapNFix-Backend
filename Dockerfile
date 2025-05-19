FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

COPY *.sln .

COPY SnapNFix.Api/*.csproj ./SnapNFix.Api/
COPY SnapNFix.Domain/*.csproj ./SnapNFix.Domain/
COPY SnapNFix.Application/*.csproj ./SnapNFix.Application/
COPY SnapNFix.Infrastructure/*.csproj ./SnapNFix.Infrastructure/

RUN dotnet restore

COPY . .

# Add EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Create migration script
RUN dotnet ef migrations add DockerMigration --project SnapNFix.Infrastructure --startup-project SnapNFix.Api

RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

ENV ConnectionStrings__DefaultConnection="Host=snapnfix-server.postgres.database.azure.com;Port=5432;Database=postgres;Username=wnrqzgrnzx;Password=123456snapnfix@;SSL Mode=Require;"

EXPOSE 10000

ENTRYPOINT ["dotnet", "SnapNFix.Api.dll"]

