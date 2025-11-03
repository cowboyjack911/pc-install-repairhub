# CrackedScreenCare - Modular Monolith PSA Platform

A comprehensive Professional Services Automation (PSA) platform built on ASP.NET Core 9, designed for repair businesses with support for Point of Sale (POS), PC Builder (CPQ), and repair workflow management.

## 🏗️ Architecture

This application implements a **Modular Monolith** ("Modulith") architecture pattern, providing the optimal balance between:
- **Development velocity** of a traditional monolith
- **Clean separation of concerns** found in microservices
- **Single deployable unit** for simplified operations

### Technology Stack

- **.NET 9** - Latest version of the .NET platform
- **ASP.NET Core** - Web application and API framework
- **Entity Framework Core** - ORM for data access
- **PostgreSQL** - Primary database with Npgsql provider
- **ASP.NET Core Identity** - Authentication and authorization
- **Docker & Docker Compose** - Containerization and orchestration

### Core Architecture Principles

1. **Asset-Centric Data Model**: Central focus on customer devices (assets) rather than just customers
2. **PostgreSQL Schema Separation**: Uses PostgreSQL schemas (identity, ticketing, inventory) for logical module separation
3. **Repository Pattern**: Enforces modular boundaries - modules interact through interfaces, not direct database access
4. **UUIDv7 Primary Keys**: Sequential UUID generation for index-friendly performance

## 📁 Project Structure

```
CrackedScreenCare/
├── src/
│   ├── CrackedScreenCare.Core/              # Shared domain entities and interfaces
│   │   ├── Entities/                        # Customer, Asset, RepairTicket
│   │   └── Interfaces/                      # IInventoryRepository, ITicketingRepository
│   ├── CrackedScreenCare.Infrastructure/    # EF Core, DbContext, repositories
│   │   ├── Data/                            # AppDbContext with PostgreSQL schemas
│   │   └── Repositories/                    # Repository implementations
│   ├── CrackedScreenCare.Modules.Ticketing/ # Repair ticket management module
│   ├── CrackedScreenCare.Modules.Inventory/ # Inventory management module
│   ├── CrackedScreenCare.Modules.PCBuilder/ # PC Builder CPQ module
│   ├── CrackedScreenCare.Modules.RepairWorkflow/ # Workflow engine integration
│   └── CrackedScreenCare.WebHost/           # Main ASP.NET Core application
├── Dockerfile                                # Docker container definition
├── docker-compose.yml                        # Multi-container orchestration
└── .github/workflows/                        # CI/CD pipeline
```

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for containerized deployment)
- [PostgreSQL 16](https://www.postgresql.org/download/) (if running locally without Docker)

### Installation Options

#### Option 1: Docker Compose (Recommended)

The simplest way to run the entire application stack:

```bash
# Clone the repository
git clone https://github.com/cowboyjack911/pc-install-repairhub.git
cd pc-install-repairhub

# Start all services (PostgreSQL + Application)
docker-compose up -d

# View logs
docker-compose logs -f webapp

# Access the application
# API: http://localhost:8080
# Swagger UI: http://localhost:8080/swagger
```

#### Option 2: Local Development

```bash
# Clone the repository
git clone https://github.com/cowboyjack911/pc-install-repairhub.git
cd pc-install-repairhub

# Restore dependencies
dotnet restore CrackedScreenCare.sln

# Update connection string in appsettings.json if needed
# Default: Host=localhost;Database=crackedscreencare;Username=postgres;Password=postgres

# Apply database migrations
cd src/CrackedScreenCare.WebHost
dotnet ef database update --project ../CrackedScreenCare.Infrastructure

# Run the application
dotnet run

# Access the application
# API: https://localhost:5001 or http://localhost:5000
# Swagger UI: https://localhost:5001/swagger
```

### Database Migrations

Create and apply migrations:

```bash
# Create a new migration
dotnet ef migrations add InitialCreate \
  --project src/CrackedScreenCare.Infrastructure \
  --startup-project src/CrackedScreenCare.WebHost

# Apply migrations to database
dotnet ef database update \
  --project src/CrackedScreenCare.Infrastructure \
  --startup-project src/CrackedScreenCare.WebHost
```

## 🗄️ Database Schema

The application uses an **asset-centric** data model optimized for repair businesses:

### Core Entities

- **Customer**: Stores customer information
  - One-to-Many relationship with Assets
  
- **Asset**: Represents a device owned by a customer (iPhone, Laptop, Custom PC)
  - Each asset maintains its own service history
  - One-to-Many relationship with RepairTickets

- **RepairTicket**: Tracks repair work for a specific asset
  - Includes status tracking (Created, InProgress, Completed, etc.)
  - Cost estimation and actual cost tracking

### PostgreSQL Schemas

- `identity`: ASP.NET Core Identity tables (users, roles, claims)
- `ticketing`: Customer, Asset, and RepairTicket tables
- `inventory`: (To be implemented) Product and stock management tables

## 🔧 Key Features

### 1. Modular Boundaries
Modules communicate through:
- Public interfaces defined in `Core` project
- Repository pattern for data access
- (Future) In-process messaging with MediatR

### 2. PostgreSQL Optimization
- **Npgsql provider** with connection pooling
- **Sequential UUIDv7** for primary keys (index-friendly)
- **Schema separation** for logical module boundaries

### 3. ASP.NET Core Identity
- User authentication for technicians and administrators
- PostgreSQL as the backing store
- Role-based authorization

### 4. Docker Support
- Multi-stage Dockerfile for optimized images
- Docker Compose for full stack deployment
- Health checks and automatic restart policies

## 🔄 CI/CD Pipeline

GitHub Actions workflow automatically:
1. Builds and tests the application on push/PR
2. Creates Docker images
3. Publishes to GitHub Container Registry
4. Tags images with branch name, SHA, and 'latest'

## 📦 Planned Features

- [ ] **PC Builder Module**: Configure-Price-Quote (CPQ) with Microsoft RulesEngine
- [ ] **Repair Workflow Module**: Integration with Elsa workflow engine
- [ ] **Inventory Module**: Full parts and stock management
- [ ] **POS Module**: Point of sale functionality
- [ ] **MediatR Integration**: For in-process module communication
- [ ] **API Documentation**: Comprehensive API documentation with examples
- [ ] **Unit Tests**: Full test coverage for all modules

## 🤝 Contributing

This is a custom PSA platform for CrackedScreenCare.com. Contributions, issues, and feature requests are welcome.

## 📄 License

[Add your license information here]

## 🔗 References

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
- [Modular Monolith Architecture](https://www.kamilgrzybek.com/design/modular-monolith-primer/)

