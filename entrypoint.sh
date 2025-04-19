#!/bin/bash
set -e

# Install EF Core tools if not already installed
dotnet tool install --global dotnet-ef || true
export PATH="$PATH:/root/.dotnet/tools"

# Create migration and update database
cd /app
dotnet ef migrations add DeploymentMigration --project ../source/SnapNFix.Infrastructure --startup-project ../source/SnapNFix.Api
dotnet ef database update --project ../source/SnapNFix.Infrastructure --startup-project ../source/SnapNFix.Api

# Start the application
exec dotnet SnapNFix.Api.dll