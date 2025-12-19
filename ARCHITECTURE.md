# Architecture Documentation

## Overview

CrackedScreenCare is a Professional Services Automation (PSA) platform built using the **Modular Monolith** architectural pattern on ASP.NET Core 9. This document details the architectural decisions, patterns, and technical implementation.

## Architectural Pattern: Modular Monolith

### Definition

A Modular Monolith (or "Modulith") is a software architecture where:
- The application is deployed as a **single unit** (monolith)
- Internally divided into **logically independent, loosely-coupled modules**
- Each module has clear boundaries and responsibilities
- Modules communicate through well-defined interfaces

### Why Modular Monolith?

**Advantages over Traditional Monolith:**
- Clear separation of concerns
- Easier to understand and maintain
- Reduced coupling between domains
- Can evolve to microservices if needed

**Advantages over Microservices:**
- No operational complexity (service discovery, API gateways, distributed tracing)
- No distributed transactions
- Simplified deployment
- Lower infrastructure costs
- Better performance (in-process communication)

### When to Use Modular Monolith

✅ **Good fit for:**
- Mid-sized applications with distinct business domains
- Teams starting greenfield projects
- Applications needing clear boundaries but not full microservices
- Cost-sensitive deployments
- Rapid development and iteration

❌ **Not ideal for:**
- Massive scale requiring independent scaling per service
- Distributed teams needing complete autonomy
- Services with vastly different technology requirements

## System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   CrackedScreenCare                      │
│                  (Single Deployment Unit)                │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │  Ticketing   │  │  Inventory   │  │  PCBuilder   │ │
│  │    Module    │  │    Module    │  │    Module    │ │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘ │
│         │                 │                 │          │
│         └─────────────────┴─────────────────┘          │
│                          │                              │
│         ┌────────────────▼────────────────┐            │
│         │    Repository Interfaces        │            │
│         │    (ITicketingRepository,       │            │
│         │     IInventoryRepository)       │            │
│         └────────────────┬────────────────┘            │
│                          │                              │
│         ┌────────────────▼────────────────┐            │
│         │   Infrastructure Layer          │            │
│         │   (EF Core, DbContext,          │            │
│         │    Repository Implementations)  │            │
│         └────────────────┬────────────────┘            │
│                          │                              │
│         ┌────────────────▼────────────────┐            │
│         │      PostgreSQL Database        │            │
│         │  (Schema-separated: identity,   │            │
│         │   ticketing, inventory, etc.)   │            │
│         └─────────────────────────────────┘            │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### Module Structure

```
CrackedScreenCare/
├── Core/                    # Shared kernel
│   ├── Entities/           # Domain entities
│   └── Interfaces/         # Module contracts
├── Infrastructure/          # Technical infrastructure
│   ├── Data/               # DbContext, configuration
│   └── Repositories/       # Repository implementations
├── Modules/
│   ├── Ticketing/          # Repair ticket management
│   ├── Inventory/          # Parts and stock management
│   ├── PCBuilder/          # Configure-Price-Quote (CPQ)
│   └── RepairWorkflow/     # Workflow orchestration
└── WebHost/                # Entry point, composition
```

## Core Architectural Principles

### 1. Domain-Driven Design (DDD)

**Bounded Contexts:**
Each module represents a bounded context with its own domain model.

**Entities:**
- `Customer`: Customer information
- `Asset`: Device owned by customer
- `RepairTicket`: Service work for an asset

**Asset-Centric Model:**
Unlike traditional POS systems, we model around assets (devices) rather than transactions:
```
Customer (1) ──> (N) Assets (1) ──> (N) RepairTickets
```

### 2. Repository Pattern

**Purpose:**
- Enforce module boundaries
- Abstract data access
- Prevent cross-module database queries

**Implementation:**
```csharp
// Core/Interfaces/IInventoryRepository.cs
public interface IInventoryRepository
{
    Task<int> GetStockLevelAsync(Guid productId);
    Task<bool> ReserveStockAsync(Guid productId, int quantity);
}

// Infrastructure/Repositories/InventoryRepository.cs
public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _context;
    // Implementation details hidden
}
```

**Why Not Just DbContext?**
Direct DbContext access allows:
```csharp
// BAD: Cross-module query violates boundaries
var result = _context.Products
    .Join(_context.RepairTickets, ...)
    .Where(...);
```

Repositories prevent this by enforcing interface contracts.

### 3. PostgreSQL Schema Separation

**Logical Module Separation:**
```sql
-- Identity module
CREATE SCHEMA identity;
CREATE TABLE identity."AspNetUsers" (...);

-- Ticketing module
CREATE SCHEMA ticketing;
CREATE TABLE ticketing.customers (...);
CREATE TABLE ticketing.assets (...);
CREATE TABLE ticketing.repair_tickets (...);

-- Inventory module
CREATE SCHEMA inventory;
CREATE TABLE inventory.products (...);
CREATE TABLE inventory.stock_levels (...);
```

**Benefits:**
- Clear module ownership
- Prevents naming collisions
- Supports future microservices migration
- Better database organization

### 4. Sequential UUID (UUIDv7) Primary Keys

**Traditional UUID (v4) Issues:**
```
Random UUIDs: 3f4e9b2c-7a8d-4f1e-9c5b-2d8e7f3a6b1c
               ↓
           Random distribution causes B-tree index fragmentation
```

**UUIDv7 Solution:**
```
Sequential UUIDs: 018b-c5a0-0000-7000-9abc-def123456789
                  ↑↑↑↑ ↑↑↑↑
                  Timestamp prefix = sequential insertion
```

**Configuration:**
```csharp
entity.Property(e => e.Id)
    .HasDefaultValueSql("gen_random_uuid()");
```

Npgsql 9.0+ automatically generates UUIDv7 when using `gen_random_uuid()`.

## Technology Stack Details

### .NET 9
- **Target Framework:** net9.0
- **Runtime:** CoreCLR
- **Language:** C# 13

### ASP.NET Core 9
- **Web Framework:** Minimal APIs + Controllers
- **Middleware:** Authentication, Authorization, CORS
- **Hosting:** Kestrel web server

### Entity Framework Core 9
- **ORM:** EF Core with Code-First approach
- **Provider:** Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4
- **Features:**
  - Migrations
  - Change tracking
  - LINQ queries
  - Connection pooling

### PostgreSQL 16
- **Version:** 16.x (Alpine Docker image)
- **Features Used:**
  - Schemas for module separation
  - gen_random_uuid() for UUIDv7
  - Advanced indexing
  - JSONB for flexible data (future)

### ASP.NET Core Identity
- **Authentication:** Cookie-based
- **Storage:** PostgreSQL via EF Core
- **Identity Tables:** In `identity` schema
- **Features:**
  - User management
  - Role-based authorization
  - Password hashing (PBKDF2)
  - Token providers

### Docker
- **Base Image:** mcr.microsoft.com/dotnet/aspnet:9.0
- **Build Image:** mcr.microsoft.com/dotnet/sdk:9.0
- **Database Image:** postgres:16-alpine

## Data Model

### Core Entities

#### Customer
```csharp
public class Customer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public virtual ICollection<Asset> Assets { get; set; }
}
```

#### Asset
```csharp
public class Asset
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string DeviceType { get; set; }  // iPhone, Laptop, Custom PC
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string SerialNumber { get; set; }
    public virtual Customer Customer { get; set; }
    public virtual ICollection<RepairTicket> RepairTickets { get; set; }
}
```

#### RepairTicket
```csharp
public class RepairTicket
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public RepairStatus Status { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public virtual Asset Asset { get; set; }
}
```

### Entity Relationships

```
Customer
    ├── Assets[]
    │   ├── RepairTickets[]
    │   │   ├── Status: Created
    │   │   ├── Status: InProgress
    │   │   └── Status: Completed
    │   └── ...
    └── ...
```

## Module Communication

### Current: Repository Interfaces

Modules communicate through repository interfaces:

```csharp
// Ticketing module needs inventory check
public class TicketService
{
    private readonly IInventoryRepository _inventory;
    
    public async Task CreateTicket(...)
    {
        var hasStock = await _inventory.GetStockLevelAsync(partId) > 0;
        if (hasStock)
        {
            await _inventory.ReserveStockAsync(partId, 1);
        }
    }
}
```

### Future: MediatR (In-Process Messaging)

Plan to implement MediatR for event-driven communication:

```csharp
// Publish event
await _mediator.Publish(new TicketCompletedEvent(ticketId));

// Handle in inventory module
public class TicketCompletedHandler : INotificationHandler<TicketCompletedEvent>
{
    public Task Handle(TicketCompletedEvent evt, ...)
    {
        // Release reserved stock
    }
}
```

## Performance Considerations

### Connection Pooling

Npgsql connection pooling is enabled by default:

```
Host=postgres;Database=crackedscreencare;Username=postgres;Password=postgres;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100
```

### Query Optimization

EF Core 9 improvements:
- Compiled queries for repeated queries
- No-tracking queries for read-only data
- Explicit loading vs. eager loading

```csharp
// Read-only, no tracking
var customers = await _context.Customers
    .AsNoTracking()
    .ToListAsync();
```

### Indexing Strategy

```sql
-- Primary keys are automatically indexed (UUIDv7)
-- Add indexes for common queries
CREATE INDEX idx_assets_customer_id ON ticketing.assets(customer_id);
CREATE INDEX idx_tickets_asset_id ON ticketing.repair_tickets(asset_id);
CREATE INDEX idx_tickets_status ON ticketing.repair_tickets(status);
```

## Security

### Authentication
- ASP.NET Core Identity with cookie authentication
- Password requirements enforced
- Account lockout on failed attempts

### Authorization
- Role-based access control
- Future: Policy-based authorization

### SQL Injection Protection
- Parameterized queries via EF Core
- No raw SQL unless necessary

### Secrets Management
- Environment variables for sensitive data
- No secrets in source code
- Docker secrets support (future)

## Scalability

### Current State
Single instance, suitable for:
- Small to medium repair shops
- Up to 1000 concurrent users
- Moderate transaction volume

### Future Scaling Options

**Vertical Scaling:**
- Increase server resources (CPU, RAM)
- Optimize database queries
- Add caching (Redis)

**Horizontal Scaling:**
- Load balancer (nginx/HAProxy)
- Multiple application instances
- Shared PostgreSQL instance

**Microservices Migration:**
The modular structure enables eventual migration:
1. Extract module to separate service
2. Replace repository with HTTP client
3. Deploy independently
4. Update load balancer routing

## Deployment

### Docker Compose (Single Server)
```yaml
services:
  postgres:
    image: postgres:16-alpine
  webapp:
    build: .
    depends_on:
      - postgres
```

### Kubernetes (Future)
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: crackedscreencare
spec:
  replicas: 3
  ...
```

## Testing Strategy

### Unit Tests (Planned)
- Test business logic in isolation
- Mock repository interfaces
- Fast execution

### Integration Tests (Planned)
- Test with real database (TestContainers)
- Verify module interactions
- Test database migrations

### API Tests (Planned)
- Test HTTP endpoints
- Verify response formats
- Test authentication/authorization

## Monitoring and Observability

### Logging
- Structured logging with Serilog (planned)
- Log levels: Debug, Information, Warning, Error
- Log sinks: Console, File, Application Insights (future)

### Metrics (Planned)
- Request/response times
- Database query performance
- Error rates
- Business metrics (tickets created, etc.)

### Health Checks (Planned)
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddNpgSql(connectionString);
```

## Future Enhancements

### Planned Features
1. **PC Builder CPQ Module**
   - Microsoft RulesEngine integration
   - Component compatibility validation
   - Real-time pricing

2. **Repair Workflow Module**
   - Elsa workflow engine
   - Visual workflow designer
   - Long-running processes

3. **MediatR Integration**
   - Event-driven module communication
   - Command/Query separation (CQRS)

4. **API Documentation**
   - OpenAPI/Swagger enhancements
   - API versioning
   - Rate limiting

5. **Background Jobs**
   - Hangfire integration
   - Scheduled tasks
   - Recurring jobs

## References

- [Modular Monolith Architecture](https://www.kamilgrzybek.com/design/modular-monolith-primer/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
