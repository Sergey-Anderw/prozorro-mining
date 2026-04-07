# ProzorroMining - Modular Monolith Architecture

ProzorroMining is a modern web application built with a **modular monolith** architecture pattern, combining the benefits of monolithic simplicity with module-oriented design principles.

## 🏗️ Architecture Overview

### Architectural Pattern: Modular Monolith

This project implements a **modular monolith** with **vertical slices** inside modules:

```
┌─────────────────────────────────────────────────┐
│                   ProzorroMining                │
│                                                 │
│  ┌─────────────┐  ┌─────────────┐              │
│  │   Module 1  │  │   Module 2  │  ...         │
│  │             │  │             │              │
│  │ ┌─────────┐ │  │ ┌─────────┐ │              │
│  │ │Vertical │ │  │ │Vertical │ │              │
│  │ │ Slice 1 │ │  │ │ Slice 1 │ │              │
│  │ └─────────┘ │  │ └─────────┘ │              │
│  │ ┌─────────┐ │  │ ┌─────────┐ │              │
│  │ │Vertical │ │  │ │Vertical │ │              │
│  │ │ Slice 2 │ │  │ │ Slice 2 │ │              │
│  │ └─────────┘ │  │ └─────────┘ │              │
│  └─────────────┘  └─────────────┘              │
└─────────────────────────────────────────────────┘
```

**Key Benefits:**
- **Modularity**: Clear separation of concerns into distinct modules
- **Vertical Slices**: Feature-complete slices with end-to-end functionality
- **Single Deployment**: One deployable unit vs. microservices complexity
- **Easier Testing**: Integration tests within modules without external services
- **Team Autonomy**: Teams work on independent modules without conflicts

### Dependency Flow

```
┌─────────────┐
│    API      │  HTTP entry point (ASP.NET Core Minimal API)
└──────┬──────┘
       │
┌──────▼──────────┐
│   Application   │  Use cases, orchestration
└──────┬──────────┘
       │
┌──────┼──────────┐
│      │          │
▼      ▼          ▼
Domain Contracts Infrastructure
```

**Strict Rules:**
- ❌ API depends only on App & Contracts
- ❌ App depends on Domain, Contracts, Infrastructure
- ❌ Infrastructure depends on Domain
- ❌ Domain has no dependencies (core of the system)
- ❌ No circular dependencies

## 📁 Project Structure

```
ProzorroMining/
├── src/
│   ├── Backend/
│   │   ├── ProzorroMining.Api              [ASP.NET Core Minimal API]
│   │   ├── ProzorroMining.App              [Application Services]
│   │   ├── ProzorroMining.Domain           [Business Logic & Entities]
│   │   ├── ProzorroMining.Infrastructure   [Data Access & External Services]
│   │   ├── ProzorroMining.Contracts        [DTOs & Shared Interfaces]
│   │   └── ProzorroMining.DbMigrator       [Database Migrations Tool]
│   │
│   └── Frontend/
│       └── prozorro-dashboard/             [React + TypeScript SPA]
│           ├── src/
│           │   ├── main.tsx
│           │   ├── App.tsx
│           │   └── index.css
│           ├── package.json
│           ├── tsconfig.json
│           ├── vite.config.ts
│           └── index.html
│
├── tests/
│   └── Backend/
│       ├── ProzorroMining.UnitTests        [Pure business logic tests]
│       ├── ProzorroMining.IntegrationTests [Full stack integration tests]
│       └── ProzorroMining.ArchitectureTests [Architecture & dependency rules]
│
├── global.json                             [.NET 8 LTS requirement]
├── Directory.Build.props                   [Shared project settings]
├── Directory.Packages.props                [Centralized NuGet versions]
├── .editorconfig                           [Code style enforcement]
├── ProzorroMining.sln                      [Solution file]
└── README.md                               [This file]
```

## 🔧 Backend Stack

### Technology Choices

| Component         | Technology      | Rationale                          |
|-------------------|-----------------|-------------------------------------|
| **Runtime**       | .NET 8          | LTS version, latest features       |
| **API Framework** | ASP.NET Core    | Minimal APIs for lean endpoints    |
| **ORM**           | Dapper          | Lightweight, efficient SQL access  |
| **Database**      | PostgreSQL      | Reliable, ACID-compliant           |
| **Testing**       | xUnit           | Modern .NET testing framework      |
| **Logging**       | Serilog         | Structured logging                 |
| **Validation**    | FluentValidation| Type-safe validation rules        |

### Why These Choices?

- **No Entity Framework**: Dapper provides fine-grained control over queries without ORM overhead
- **No MediatR**: Direct dependency injection for simpler, more transparent flow
- **No Generic Repository**: Explicit data access methods per feature
- **No Redis**: Start simple; add caching when bottlenecks are proven
- **No API Gateway**: Direct client-to-service communication for initial phase

## 📱 Frontend Stack

### Technology Choices

| Component      | Technology       | Rationale                          |
|----------------|------------------|------------------------------------|
| **Framework**  | React 18         | Declarative UI, large ecosystem    |
| **Language**   | TypeScript       | Type safety, better IDE support    |
| **Build Tool** | Vite             | Fast build times, ES modules       |
| **Styling**    | CSS              | Future: Tailwind or styled-components |

## 🧪 Testing Strategy

### Three-Layer Testing Pyramid

```
        ▲
       /|\
      / | \
     /  |  \    Architecture Tests
    /   |   \   (Dependency violations)
   /    |    \
  ┌─────┼─────┐
  │     |     │  Integration Tests
  │   Tests   │  (Full stack, Docker)
  └─────┼─────┘
        │
     ┌──┴──┐
     │Unit │  Unit Tests (Pure logic)
     └─────┘
```

### Test Projects

1. **UnitTests**
   - Pure business logic without external dependencies
   - Domain, value objects, helpers
   - Fast, deterministic, no I/O

2. **IntegrationTests**
   - Full feature flow from API to database
   - Uses TestContainers for PostgreSQL
   - Validates data persistence and retrieval

3. **ArchitectureTests**
   - Enforces dependency rules
   - Prevents accidental circular dependencies
   - Validates layering principles

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL 14+
- Node.js 18+ (for frontend)

### Backend Setup

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run API
dotnet run --project src/Backend/ProzorroMining.Api
```

### Frontend Setup

```bash
cd src/Frontend/prozorro-dashboard

# Install dependencies
npm install

# Run development server
npm run dev

# Build for production
npm run build
```

## 📦 Project References

### Backend Project Dependencies

```
ProzorroMining.Api
├── ProzorroMining.App
└── ProzorroMining.Contracts

ProzorroMining.App
├── ProzorroMining.Domain
├── ProzorroMining.Contracts
└── ProzorroMining.Infrastructure

ProzorroMining.Infrastructure
└── ProzorroMining.Domain

ProzorroMining.DbMigrator
└── ProzorroMining.Infrastructure

Test Projects
├── ProzorroMining.UnitTests → Domain, App, Contracts
├── ProzorroMining.IntegrationTests → Api, Infrastructure
└── ProzorroMining.ArchitectureTests → All Backend Projects
```

## 🏛️ Layer Responsibilities

### 🎯 API Layer (`ProzorroMining.Api`)
- HTTP endpoint mapping
- Request/response serialization
- Authentication & authorization setup
- OpenAPI/Swagger documentation
- **Should NOT contain**: Business logic, data access

### 🔄 Application Layer (`ProzorroMining.App`)
- Use case orchestration
- Input validation
- Business rule coordination
- Transaction management
- **Should NOT contain**: HTTP concerns, domain rules

### 💼 Domain Layer (`ProzorroMining.Domain`)
- Business entities
- Value objects
- Domain events
- Pure business logic
- **Should NOT contain**: Any external dependencies

### 📊 Infrastructure Layer (`ProzorroMining.Infrastructure`)
- Database queries (Dapper)
- PostgreSQL connections
- External service integrations
- **Should NOT contain**: Business logic, application flow

### 📋 Contracts Layer (`ProzorroMining.Contracts`)
- Request/Response DTOs
- Shared interfaces
- Exception types
- Constants
- **Purpose**: Cross-cutting communication

## 🔒 Architecture Constraints

To maintain module integrity:

1. **No Direct Database Imports in API**
   - All data access through Application layer

2. **Domain is Isolated**
   - Can be tested without Database, HTTP, or external services

3. **Vertical Slices Within Modules**
   - Each feature owns its complete vertical stack

4. **Future Patterns**
   - **Modules** can evolve into separate deployment units
   - **Vertical slices** can become event-driven boundaries
   - **Domain events** can trigger cross-module communication

## 🛠️ Configuration

### `global.json`
Enforces .NET 8 for all projects

### `Directory.Build.props`
Shared compilation settings:
- Nullable reference types enabled
- Latest C# language features
- Strict compiler warnings

### `Directory.Packages.props`
Centralized NuGet package versions for consistency

### `.editorconfig`
Code style enforcement across all projects

## 📝 Development Workflow

1. **Create vertical slice** within appropriate module
2. **Start with Domain** (business rules)
3. **Add Infrastructure** (data access)
4. **Implement Application** (orchestration)
5. **Expose via API** (endpoints)
6. **Test each layer** (unit → integration → architecture)

## 🔍 Next Steps

- [ ] Set up PostgreSQL database
- [ ] Create initial database schema
- [ ] Implement first feature (vertical slice)
- [ ] Configure CI/CD pipeline
- [ ] Add authentication/authorization
- [ ] Implement logging & monitoring
- [ ] Create API documentation
- [ ] Deploy to staging environment

## 📚 Resources

- [Clean Architecture by Robert Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Vertical Slices Architecture](https://jimmybogard.com/vertical-slice-architecture/)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Dapper Documentation](https://github.com/DapperLib/Dapper)

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
