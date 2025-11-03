# Deployment Guide

This guide provides detailed instructions for deploying the CrackedScreenCare application.

## Quick Start with Docker Compose

The fastest way to deploy the full application stack:

```bash
# 1. Clone the repository
git clone https://github.com/cowboyjack911/pc-install-repairhub.git
cd pc-install-repairhub

# 2. Start all services
docker-compose up -d

# 3. Wait for services to be healthy (30 seconds typically)
docker-compose logs -f webapp

# 4. Access the application
# API: http://localhost:8080
# Swagger UI: http://localhost:8080/swagger
```

## Prerequisites

### For Docker Deployment
- Docker Desktop 4.0+ or Docker Engine 20.10+
- Docker Compose 2.0+
- 2GB free RAM
- 5GB free disk space

### For Local Development
- .NET 9 SDK
- PostgreSQL 16
- 4GB free RAM
- Visual Studio 2022 / VS Code / Rider (optional)

## Deployment Methods

### Method 1: Docker Compose (Production Ready)

**Advantages:**
- All dependencies bundled
- Consistent environment
- Easy rollback
- Production-ready configuration

**Steps:**

1. **Configure Environment Variables** (Optional)

Edit `docker-compose.yml` to customize:

```yaml
services:
  webapp:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=crackedscreencare;Username=postgres;Password=YOUR_SECURE_PASSWORD
```

2. **Start Services**

```bash
docker-compose up -d
```

3. **Verify Deployment**

```bash
# Check service status
docker-compose ps

# View logs
docker-compose logs -f webapp

# Test API
curl http://localhost:8080
```

4. **Apply Database Migrations**

The application will automatically create the database schema on first run. To manually apply migrations:

```bash
docker-compose exec webapp dotnet ef database update --project /app/CrackedScreenCare.Infrastructure.dll
```

5. **Create Admin User** (Coming soon)

```bash
# Will be available in future release
docker-compose exec webapp dotnet run -- create-admin admin@crackedscreencare.com
```

### Method 2: GitHub Container Registry (GHCR)

The CI/CD pipeline automatically builds and publishes Docker images to GHCR.

**Pull and Run:**

```bash
# Login to GHCR (requires GitHub personal access token)
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin

# Pull latest image
docker pull ghcr.io/cowboyjack911/pc-install-repairhub:latest

# Run with external PostgreSQL
docker run -d \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=your-db-host;Database=crackedscreencare;Username=postgres;Password=password" \
  ghcr.io/cowboyjack911/pc-install-repairhub:latest
```

### Method 3: Local Development

For development and testing:

1. **Install PostgreSQL**

```bash
# Ubuntu/Debian
sudo apt-get install postgresql-16

# macOS (Homebrew)
brew install postgresql@16

# Windows
# Download from https://www.postgresql.org/download/windows/
```

2. **Create Database**

```bash
sudo -u postgres psql
CREATE DATABASE crackedscreencare;
CREATE USER crackedscreen WITH PASSWORD 'devpassword';
GRANT ALL PRIVILEGES ON DATABASE crackedscreencare TO crackedscreen;
\q
```

3. **Update Connection String**

Edit `src/CrackedScreenCare.WebHost/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=crackedscreencare;Username=crackedscreen;Password=devpassword"
  }
}
```

4. **Apply Migrations**

```bash
cd src/CrackedScreenCare.WebHost
dotnet ef database update --project ../CrackedScreenCare.Infrastructure
```

5. **Run Application**

```bash
dotnet run
```

Application will be available at:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000
- Swagger: https://localhost:5001/swagger

## Database Management

### Creating Migrations

When you modify entity models, create a new migration:

```bash
dotnet ef migrations add MigrationName \
  --project src/CrackedScreenCare.Infrastructure \
  --startup-project src/CrackedScreenCare.WebHost
```

### Applying Migrations

```bash
# Apply all pending migrations
dotnet ef database update \
  --project src/CrackedScreenCare.Infrastructure \
  --startup-project src/CrackedScreenCare.WebHost

# Apply to specific migration
dotnet ef database update MigrationName \
  --project src/CrackedScreenCare.Infrastructure \
  --startup-project src/CrackedScreenCare.WebHost
```

### Rollback Migrations

```bash
# Rollback to previous migration
dotnet ef migrations remove \
  --project src/CrackedScreenCare.Infrastructure \
  --startup-project src/CrackedScreenCare.WebHost

# Rollback database to specific migration
dotnet ef database update PreviousMigrationName \
  --project src/CrackedScreenCare.Infrastructure \
  --startup-project src/CrackedScreenCare.WebHost
```

## Production Configuration

### Security Considerations

1. **Change Default Passwords**

Never use default passwords in production:

```yaml
# docker-compose.yml
environment:
  POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-secure_password_here}
```

2. **Use Environment Variables**

Store sensitive configuration in environment variables or secrets management:

```bash
export ConnectionStrings__DefaultConnection="Host=prod-db;Database=crackedscreencare;Username=app;Password=${DB_PASSWORD}"
```

3. **Enable HTTPS**

Configure SSL certificates in production:

```csharp
// Program.cs
builder.WebHost.UseKestrel(options =>
{
    options.Listen(IPAddress.Any, 443, listenOptions =>
    {
        listenOptions.UseHttps("certificate.pfx", "password");
    });
});
```

4. **Configure CORS** (if needed)

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", builder =>
    {
        builder.WithOrigins("https://crackedscreencare.com")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

### Performance Optimization

1. **Connection Pooling**

PostgreSQL connection pooling is configured by default in Npgsql. Adjust if needed:

```
Host=postgres;Database=crackedscreencare;Username=postgres;Password=postgres;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100
```

2. **Response Compression**

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});
```

3. **Response Caching**

```csharp
builder.Services.AddResponseCaching();
app.UseResponseCaching();
```

### Monitoring and Logging

1. **Structured Logging**

Application uses built-in .NET logging. Configure in appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

2. **Health Checks** (Coming soon)

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

app.MapHealthChecks("/health");
```

## Troubleshooting

### Database Connection Issues

**Problem:** Can't connect to PostgreSQL

**Solutions:**
1. Verify PostgreSQL is running: `docker-compose ps` or `sudo systemctl status postgresql`
2. Check connection string format
3. Verify network connectivity
4. Check PostgreSQL logs: `docker-compose logs postgres`

### Migration Issues

**Problem:** Migrations fail to apply

**Solutions:**
1. Ensure database exists
2. Verify user has sufficient permissions
3. Check for conflicting migrations
4. Review migration SQL: `dotnet ef migrations script`

### Port Already in Use

**Problem:** Port 8080 or 5432 already in use

**Solutions:**
1. Stop conflicting services
2. Change port in docker-compose.yml:
```yaml
ports:
  - "8090:8080"  # Change external port
```

### Docker Build Fails

**Problem:** Docker image build errors

**Solutions:**
1. Clear Docker build cache: `docker builder prune`
2. Ensure .dockerignore excludes obj/bin directories
3. Verify all project files are committed
4. Check Docker daemon logs

## Updating the Application

### Docker Compose

```bash
# Pull latest changes
git pull origin main

# Rebuild and restart
docker-compose down
docker-compose build --no-cache
docker-compose up -d

# Apply any new migrations
docker-compose exec webapp dotnet ef database update
```

### GitHub Container Registry

```bash
# Pull latest image
docker pull ghcr.io/cowboyjack911/pc-install-repairhub:latest

# Restart with new image
docker-compose up -d
```

## Backup and Restore

### Database Backup

```bash
# Using Docker Compose
docker-compose exec postgres pg_dump -U postgres crackedscreencare > backup.sql

# Using local PostgreSQL
pg_dump -U postgres crackedscreencare > backup.sql
```

### Database Restore

```bash
# Using Docker Compose
docker-compose exec -T postgres psql -U postgres crackedscreencare < backup.sql

# Using local PostgreSQL
psql -U postgres crackedscreencare < backup.sql
```

## Support

For issues and questions:
- GitHub Issues: https://github.com/cowboyjack911/pc-install-repairhub/issues
- Documentation: See README.md

## Next Steps

After deployment:
1. Configure user authentication
2. Set up backup schedules
3. Configure monitoring alerts
4. Review security settings
5. Test disaster recovery procedures
