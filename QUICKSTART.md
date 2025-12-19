# Quick Start Guide

## Prerequisites
- Docker Desktop or Docker Engine 20.10+
- Docker Compose 2.0+
- Git

## Installation (3 Steps)

### 1. Clone the Repository
```bash
git clone https://github.com/cowboyjack911/pc-install-repairhub.git
cd pc-install-repairhub
```

### 2. Start the Application
```bash
docker-compose up -d
```

This will:
- Pull PostgreSQL 16 image
- Build the .NET application
- Start both services
- Create the database schema automatically

### 3. Access the Application

**Main Application:**
```
http://localhost:8080
```

**API Documentation (Swagger):**
```
http://localhost:8080/swagger
```

**Database:**
```
Host: localhost
Port: 5432
Database: crackedscreencare
Username: postgres
Password: postgres
```

## Verify Installation

Check service status:
```bash
docker-compose ps
```

View logs:
```bash
# All services
docker-compose logs -f

# Application only
docker-compose logs -f webapp

# Database only
docker-compose logs -f postgres
```

Test the API:
```bash
curl http://localhost:8080
```

Expected response:
```json
{
  "application": "CrackedScreenCare - Modular Monolith PSA Platform",
  "version": "1.0.0",
  "architecture": "ASP.NET Core 9 Modular Monolith",
  "database": "PostgreSQL with Npgsql",
  "features": [
    "Asset-Centric Repair Ticketing",
    "PostgreSQL with Schema Separation",
    "ASP.NET Core Identity",
    "Repository Pattern for Module Boundaries"
  ]
}
```

## Stopping the Application

```bash
# Stop services
docker-compose down

# Stop and remove volumes (data will be lost)
docker-compose down -v
```

## Troubleshooting

### Port Already in Use

If ports 8080 or 5432 are already in use:

1. Edit `docker-compose.yml`
2. Change the port mappings:
```yaml
services:
  postgres:
    ports:
      - "5433:5432"  # Changed from 5432:5432
  webapp:
    ports:
      - "8090:8080"  # Changed from 8080:8080
```
3. Restart: `docker-compose up -d`

### Docker Build Issues

If you encounter SSL/certificate issues during build:

**Option 1: Use Pre-built Image (when available)**
```bash
docker pull ghcr.io/cowboyjack911/pc-install-repairhub:latest
```

**Option 2: Build Locally**
```bash
# Ensure you have .NET 9 SDK installed
dotnet build CrackedScreenCare.sln
cd src/CrackedScreenCare.WebHost
dotnet run
```

**Option 3: Update Docker DNS**
Add to `docker-compose.yml`:
```yaml
services:
  webapp:
    build:
      context: .
      dockerfile: Dockerfile
      args:
        - BUILDKIT_INLINE_CACHE=1
    dns:
      - 8.8.8.8
      - 8.8.4.4
```

### Database Connection Fails

1. Check PostgreSQL is running:
```bash
docker-compose ps postgres
```

2. Check PostgreSQL logs:
```bash
docker-compose logs postgres
```

3. Verify connection string in application logs:
```bash
docker-compose logs webapp | grep -i "connection"
```

## Next Steps

- See [DEPLOYMENT.md](DEPLOYMENT.md) for production deployment
- See [ARCHITECTURE.md](ARCHITECTURE.md) for technical details
- See [README.md](README.md) for complete documentation

## Support

- GitHub Issues: https://github.com/cowboyjack911/pc-install-repairhub/issues
- Documentation: See README.md and other guides in the repository
