
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

COPY *.sln .

COPY SnapNFix.Api/*.csproj ./SnapNFix.Api/
COPY SnapNFix.Domain/*.csproj ./SnapNFix.Domain/
COPY SnapNFix.Application/*.csproj ./SnapNFix.Application/
COPY SnapNFix.Infrastructure/*.csproj ./SnapNFix.Infrastructure/

RUN dotnet restore

COPY . .

RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

ENV ConnectionStrings__DefaultConnection="Host=dpg-d01e5sadbo4c738mad7g-a;Port=5432;Database=snapnfix_db;Username=snapnfix_db_user;Password=of1ADH97CszGA6B4rwUGWrCKZAha3wPX;SSL Mode=Require;"

EXPOSE 10000

ENTRYPOINT ["dotnet", "SnapNFix.Api.dll"]