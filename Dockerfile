# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy csproj files first to leverage Docker layer caching
COPY *.sln .
COPY */*.csproj ./
RUN for file in $(find . -name "*.csproj"); do mkdir -p ${file%/*}/ && mv $file ${file%/*}/; done

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Set timezone if needed
ENV TZ=UTC

# Add application insights if needed
# ENV APPLICATIONINSIGHTS_CONNECTION_STRING="your_connection_string"

# Set ASP.NET Core environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
# ENV ConnectionStrings__DefaultConnection="your_connection_string"

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy from build stage
COPY --from=build /app .
RUN chown -R appuser:appuser /app

# Configure healthcheck
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Set working directory permissions
RUN chmod -R 755 /app

# Set user
USER appuser

# Expose the port
EXPOSE 8080

# Add metadata
LABEL maintainer="Your Name <your.email@example.com>"
LABEL version="1.0"
LABEL description="SnapNFix API Service"

# Run the application
ENTRYPOINT ["dotnet", "SnapnFix.Api.dll"]