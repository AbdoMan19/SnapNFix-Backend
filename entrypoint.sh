#!/bin/bash

# Wait for the database to be ready (optional)
sleep 5

# Apply database migrations
dotnet ef database update --project SnapNFix.Infrastructure --startup-project SnapNFix.Api

# Start the application
exec dotnet SnapNFix.Api.dll