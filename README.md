# Mango Microservices

A full-stack e-commerce platform built with .NET microservices architecture, enabling scalable online shopping with modular services.

## Architecture Overview

```
                                    ┌─────────────────┐
                                    │   MVC Web UI    │
                                    │   (Port 7200)   │
                                    └────────┬────────┘
                                             │
                                    ┌────────▼────────┐
                                    │   YARP Gateway  │
                                    │   (Port 7100)   │
                                    └────────┬────────┘
                                             │
          ┌──────────────┬──────────────┬────┴────┬──────────────┬──────────────┐
          │              │              │         │              │              │
    ┌─────▼────┐  ┌─────▼────┐  ┌─────▼────┐ ┌─────▼────┐ ┌─────▼────┐ ┌─────▼────┐
    │ AuthAPI  │  │ProductAPI│  │ CartAPI │ │ OrderAPI │ │CouponAPI │ │ RewardAPI│
    │ :7001    │  │ :7002    │  │ :7003   │ │ :7004    │ │ :7005    │ │ :7006    │
    └─────┬────┘  └─────┬────┘  └────┬────┘ └─────┬────┘ └─────┬────┘ └─────┬────┘
          │              │              │            │            │            │
          └──────────────┴──────────────┴────────────┴────────────┴────────────┘
                                             │
                                    ┌────────▼────────┐
                                    │  Message Bus   │
                                    │   (RabbitMQ)   │
                                    └────────┬────────┘
                                             │
                              ┌──────────────┼──────────────┐
                              │              │              │
                        ┌─────▼────┐  ┌─────▼────┐  ┌─────▼────┐
                        │ EmailAPI │  │  Reward  │  │ Product  │
                        │ :7007    │  │   API    │  │   API    │
                        └──────────┘  └──────────┘  └──────────┘
```

## Services

| Service | Port | Technology | Description |
|---------|------|------------|-------------|
| [AuthAPI](src/Services/Auth/Mango.Services.AuthAPI) | 7001 | ASP.NET Core Web API | User authentication, JWT token generation, role management |
| [ProductAPI](src/Services/Product/Mango.Services.ProductAPI) | 7002 | ASP.NET Core Web API | Product catalog, categories, inventory management |
| [ShoppingCartAPI](src/Services/ShoppingCart/Mango.Services.ShoppingCartAPI) | 7003 | ASP.NET Core Web API | Shopping cart, coupon application |
| [OrderAPI](src/Services/Order/Mango.Services.OrderAPI) | 7004 | ASP.NET Core Web API | Order processing, payment integration |
| [CouponAPI](src/Services/Coupon/Mango.Services.CouponAPI) | 7005 | ASP.NET Core Web API | Discount coupons and promo codes |
| [RewardAPI](src/Services/Reward/Mango.Services.RewardAPI) | 7006 | ASP.NET Core Web API | Loyalty points, reward transactions |
| [EmailAPI](src/Services/Email/Mango.Services.EmailAPI) | 7007 | ASP.NET Core Web API | Email consumers (event-driven) |
| [Gateway](src/Gateway/Mango.GatewaySolution) | 7100 | YARP | API Gateway with routing, load balancing |
| [Web](src/Web/Mango.Web) | 7200 | ASP.NET Core MVC | Frontend web application |

## Technology Stack

- **Runtime**: .NET 10
- **Language**: C# 13
- **Database**: SQL Server 2022
- **ORM**: Entity Framework Core 10
- **API Gateway**: YARP 3.x
- **Message Broker**: RabbitMQ via MassTransit 8.x
- **Authentication**: JWT Tokens
- **Object Mapping**: Mapperly (compile-time)
- **HTTP Resilience**: Polly v8
- **Logging**: Serilog
- **Testing**: xUnit + Moq + FluentAssertions

## Domain Events

| Event | Publisher | Consumers |
|-------|-----------|-----------|
| `OrderPlaced` | OrderAPI | EmailAPI, RewardAPI |
| `PaymentCompleted` | OrderAPI | OrderAPI |
| `UserRegistered` | AuthAPI | EmailAPI |
| `CartCheckedOut` | ShoppingCartAPI | OrderAPI |
| `CouponApplied` | CouponAPI | ShoppingCartAPI |
| `PointsRedeemed` | RewardAPI | OrderAPI |
| `RewardPointsEarned` | RewardAPI | - |
| `InventoryUpdated` | ProductAPI | - |

## Project Structure

```
src/
├── BuildingBlocks/
│   ├── Mango.Contracts/      # Shared DTOs and events
│   ├── Mango.MessageBus/     # RabbitMQ messaging
│   └── Mango.SharedKernel/   # Shared utilities
├── Services/
│   ├── Auth/
│   ├── Product/
│   ├── ShoppingCart/
│   ├── Order/
│   ├── Coupon/
│   ├── Reward/
│   └── Email/
├── Gateway/
│   └── Mango.GatewaySolution/  # YARP reverse proxy
└── Web/
    └── Mango.Web/               # MVC frontend

tests/
├── UnitTests/
├── IntegrationTests/
└── E2ETests/
```

## Prerequisites

- .NET 10 SDK
- SQL Server 2022
- RabbitMQ 3.13+
- Visual Studio 2022 or VS Code

## Getting Started

### 1. Clone and Restore

```bash
git clone <repository-url>
cd MangoMicroServices
dotnet restore
```

### 2. Database Setup

Update connection strings in each service's `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=MangoAuth;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True"
}
```

Run EF Core migrations:

```bash
dotnet ef database update --project src/Services/Auth/Mango.Services.AuthAPI
dotnet ef database update --project src/Services/Product/Mango.Services.ProductAPI
# ... repeat for other services
```

### 3. Run Services

```bash
# Start all services (recommended: use multiple terminals)
dotnet run --project src/Services/Auth/Mango.Services.AuthAPI
dotnet run --project src/Services/Product/Mango.Services.ProductAPI
# ... start other services

# Or start Gateway and access via http://localhost:7100
dotnet run --project src/Gateway/Mango.GatewaySolution

# Start Web UI
dotnet run --project src/Web/Mango.Web
```

### 4. Run Tests

```bash
dotnet test
```

## Configuration

### Environment Variables

Each service supports these environment variables:

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment | Development |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | - |
| `RabbitMq__Host` | RabbitMQ host | localhost |
| `RabbitMq__Username` | RabbitMQ username | guest |
| `RabbitMq__Password` | RabbitMQ password | guest |

### Ports

| Service | Dev Port | Production Port |
|---------|---------|-----------------|
| AuthAPI | 7001 | 80 |
| ProductAPI | 7002 | 80 |
| ShoppingCartAPI | 7003 | 80 |
| OrderAPI | 7004 | 80 |
| CouponAPI | 7005 | 80 |
| RewardAPI | 7006 | 80 |
| EmailAPI | 7007 | 80 |
| Gateway | 7100 | 80 |
| Web | 7200 | 80 |

## API Response Format

All APIs return consistent envelope:

```json
{
  "isSuccess": true,
  "result": {},
  "statusCode": 200,
  "message": ""
}
```

## Security

- JWT Bearer authentication for protected endpoints
- Role-based authorization (Admin, User)
- SQL injection prevention via EF Core
- CORS configuration for frontend access
- Secure password hashing with ASP.NET Identity

## Observability

- **Health Endpoints**: `/health` and `/health/ready` for each service
- **Structured Logging**: Serilog with console and file sinks
- **Correlation IDs**: Propagated across service calls

## Deployment

### Docker

```bash
# Build Docker images
docker build -t mango-authapi -f src/Services/Auth/Mango.Services.AuthAPI/Dockerfile .
# ... build other services

# Run containers
docker run -d -p 7001:80 mango-authapi
```

### Kubernetes

See [deploy/k8s/](deploy/k8s/) for Kubernetes manifests.

### Helm

```bash
helm install mango ./deploy/helm/mango
```

## Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.
