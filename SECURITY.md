# Security Configuration

## ⚠️ Important Security Notes

### Development vs Production

The default configuration includes **development-only** credentials that **MUST be changed** before production deployment.

### Default Credentials (DO NOT USE IN PRODUCTION)

**PostgreSQL:**
- Username: `postgres`
- Password: `postgres`
- Database: `crackedscreencare`

**Location in code:**
- `appsettings.json`: Line 3
- `Program.cs`: Lines 10-11
- `docker-compose.yml`: Lines 9-10

## Production Security Checklist

### 1. Database Credentials

**Option A: Environment Variables (Recommended)**

```bash
# Set environment variables
export DB_PASSWORD="your_secure_password_here"

# Run with environment variables
docker-compose up -d
```

Update `docker-compose.yml`:
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=postgres;Database=crackedscreencare;Username=postgres;Password=${DB_PASSWORD}
```

**Option B: Docker Secrets**

```yaml
version: '3.8'
services:
  webapp:
    secrets:
      - db_password
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=crackedscreencare;Username=postgres;Password=/run/secrets/db_password

secrets:
  db_password:
    external: true
```

**Option C: Configuration Files**

Create `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db-server;Database=crackedscreencare;Username=app_user;Password=REPLACE_WITH_SECURE_PASSWORD"
  }
}
```

### 2. ASP.NET Core Identity

Configure strong password requirements in `Program.cs`:

```csharp
builder.Services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12; // Increase from 8
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
```

### 3. HTTPS Configuration

**Development (Self-Signed Certificate):**
```bash
dotnet dev-certs https --trust
```

**Production:**

1. Obtain SSL certificate (Let's Encrypt, commercial CA, etc.)
2. Configure Kestrel in `Program.cs`:

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 443, listenOptions =>
    {
        listenOptions.UseHttps("/path/to/certificate.pfx", "certificate_password");
    });
});
```

### 4. CORS Policy

For production, restrict CORS to specific origins:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://crackedscreencare.com", "https://www.crackedscreencare.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// In middleware
app.UseCors("Production");
```

### 5. API Key Protection (Future)

When implementing APIs:

```csharp
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });
```

### 6. Rate Limiting

Add rate limiting to prevent abuse:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
    });
});

app.UseRateLimiter();
```

### 7. Security Headers

Add security headers middleware:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
    await next();
});
```

## Secret Management Solutions

### For Small Deployments
- Environment variables
- Configuration files with restricted permissions
- Docker secrets

### For Enterprise Deployments
- Azure Key Vault
- AWS Secrets Manager
- HashiCorp Vault
- Kubernetes Secrets

## Database Security

### PostgreSQL Best Practices

1. **Create Application-Specific User:**
```sql
CREATE USER crackedscreen_app WITH PASSWORD 'secure_password';
GRANT CONNECT ON DATABASE crackedscreencare TO crackedscreen_app;
GRANT USAGE ON SCHEMA ticketing TO crackedscreen_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA ticketing TO crackedscreen_app;
```

2. **Enable SSL Connections:**
```
Host=postgres;Database=crackedscreencare;Username=app;Password=pass;SSL Mode=Require
```

3. **Regular Backups:**
```bash
# Daily backup script
pg_dump -U postgres crackedscreencare > backup_$(date +%Y%m%d).sql
```

## Monitoring and Auditing

### Application Logging

Configure structured logging:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.AspNetCore.Authentication": "Information"
    }
  }
}
```

### Security Events to Log

- Failed login attempts
- User creation/deletion
- Password changes
- Permission changes
- Sensitive data access
- API authentication failures

## Vulnerability Scanning

### NuGet Package Security

```bash
# Check for vulnerable packages
dotnet list package --vulnerable

# Update packages
dotnet restore
```

### Container Security

```bash
# Scan Docker image for vulnerabilities
docker scout cve crackedscreencare:latest
```

## Incident Response

### In Case of Security Breach

1. **Immediate Actions:**
   - Disable compromised accounts
   - Rotate all passwords and keys
   - Review access logs
   - Identify scope of breach

2. **Investigation:**
   - Review application logs
   - Check database audit logs
   - Analyze network traffic

3. **Remediation:**
   - Patch vulnerabilities
   - Update security policies
   - Notify affected users if required

4. **Prevention:**
   - Implement additional monitoring
   - Update security procedures
   - Conduct security training

## Compliance

Ensure compliance with:
- GDPR (if serving EU customers)
- PCI DSS (if processing payments)
- HIPAA (if handling medical device data)
- SOC 2 (for enterprise customers)

## Regular Security Maintenance

- [ ] Monthly: Review and update dependencies
- [ ] Quarterly: Security audit and penetration testing
- [ ] Annually: Comprehensive security review
- [ ] As needed: Respond to CVE notifications

## Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [PostgreSQL Security](https://www.postgresql.org/docs/current/security.html)
- [Docker Security](https://docs.docker.com/engine/security/)

## Support

For security-related questions or to report vulnerabilities:
- Create a private security advisory on GitHub
- Contact: [Add security contact email]
