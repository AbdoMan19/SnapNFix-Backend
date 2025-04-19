#!/bin/bash

# Wait for the database to be ready (optional)
sleep 5

# Apply database migrations
dotnet SnapNFix.Api.dll --migrate

# Start the application
exec dotnet SnapNFix.Api.dll