# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY CrackedScreenCare.sln ./
COPY src/CrackedScreenCare.Core/CrackedScreenCare.Core.csproj src/CrackedScreenCare.Core/
COPY src/CrackedScreenCare.Infrastructure/CrackedScreenCare.Infrastructure.csproj src/CrackedScreenCare.Infrastructure/
COPY src/CrackedScreenCare.Modules.Ticketing/CrackedScreenCare.Modules.Ticketing.csproj src/CrackedScreenCare.Modules.Ticketing/
COPY src/CrackedScreenCare.Modules.Inventory/CrackedScreenCare.Modules.Inventory.csproj src/CrackedScreenCare.Modules.Inventory/
COPY src/CrackedScreenCare.Modules.PCBuilder/CrackedScreenCare.Modules.PCBuilder.csproj src/CrackedScreenCare.Modules.PCBuilder/
COPY src/CrackedScreenCare.Modules.RepairWorkflow/CrackedScreenCare.Modules.RepairWorkflow.csproj src/CrackedScreenCare.Modules.RepairWorkflow/
COPY src/CrackedScreenCare.WebHost/CrackedScreenCare.WebHost.csproj src/CrackedScreenCare.WebHost/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build the application
WORKDIR /src/src/CrackedScreenCare.WebHost
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CrackedScreenCare.WebHost.dll"]
