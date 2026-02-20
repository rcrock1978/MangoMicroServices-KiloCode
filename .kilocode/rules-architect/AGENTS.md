# AGENTS.md - Architect Mode

This file provides guidance to agents when working with code in this repository.

## Architectural Constraints (Non-Obvious)

- **Service autonomy**: Each service owns its data — separate SQL Server schema, no shared databases
- **Event-driven by default**: Domain events (OrderPlaced, RewardPointsEarned) via RabbitMQ; synchronous calls only when necessary
- **Consumer idempotency**: All event consumers MUST handle duplicate events gracefully
- **API Gateway pattern**: YARP as single entry point — backend services not directly exposed to internet
- **No circular dependencies**: Services cannot depend on each other; use events for coordination

## Design Principles

- **Clean Architecture**: Each service implements Domain → Application → Infrastructure → Presentation layers
- **Shared contracts**: DTOs and events in `Mango.Contracts` package (separate from service code)
- **JWT authentication**: Both user auth and service-to-service communication
- **Resilience**: Polly v8 with exponential backoff + jitter for all HTTP inter-service calls

## Performance Targets

- API response p95: < 200ms
- API response p99: < 500ms
- Message processing: < 100ms per event
- Cold start (container): < 3 seconds

## Observability Requirements

- Serilog → Seq/ELK for structured logging
- OpenTelemetry for metrics and distributed tracing
- Health endpoints: `/health` and `/health/ready` per service
- Correlation IDs propagated across all service calls

## Deployment

- Each service: multi-stage Dockerfile + Helm chart
- Docker Compose for local development (SQL Server + RabbitMQ + Redis)
- Kubernetes with Horizontal Pod Autoscaler
- Trunk-based development with short-lived feature branches

## Solution Structure

```
Mango_Microservices/
├── Mango.sln
├── Directory.Build.props
├── docker-compose.yml
├── src/
│   ├── BuildingBlocks/
│   │   ├── Mango.Contracts/      # Shared DTOs, Events, Interfaces
│   │   ├── Mango.MessageBus/    # MassTransit configuration
│   │   └── Mango.SharedKernel/  # Cross-cutting: logging, resilience, health
│   ├── Gateway/
│   │   └── Mango.GatewaySolution/  # YARP routes in yarp.json
│   ├── Services/
│   │   ├── Auth/Mango.Services.AuthAPI/
│   │   ├── Product/Mango.Services.ProductAPI/
│   │   ├── ShoppingCart/Mango.Services.ShoppingCartAPI/
│   │   ├── Order/Mango.Services.OrderAPI/
│   │   ├── Coupon/Mango.Services.CouponAPI/
│   │   ├── Reward/Mango.Services.RewardAPI/
│   │   └── Email/Mango.Services.EmailAPI/
│   └── Web/Mango.Web/
└── tests/
    ├── UnitTests/
    ├── IntegrationTests/
    └── E2ETests/
```

## Database Per Service

| Service | Database Name |
|---------|---------------|
| AuthAPI | MangoAuth |
| ProductAPI | MangoProduct |
| ShoppingCartAPI | MangoCart |
| OrderAPI | MangoOrder |
| CouponAPI | MangoCoupon |
| RewardAPI | MangoReward |
| EmailAPI | MangoEmail |
