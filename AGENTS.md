# AGENTS.md

This file provides guidance to agents when working with code in this repository.

## Build & Test Commands

- **Format code**: `dotnet format` (zero warnings required)
- **Run tests**: `dotnet test` (via xUnit)
- **Integration tests**: Uses Testcontainers for SQL Server + RabbitMQ (not mock databases)
- **Contract tests**: Pact.NET for all public APIs
- **Coverage gates**: ≥85% unit, ≥70% integration per service

## Project Structure

- **Architecture**: Clean Architecture per service: `Domain → Application → Infrastructure → Presentation`
- **Shared contracts**: `Mango.Services.{Name}` namespaces; shared DTOs/events in `Mango.Contracts`
- **Each service**: Owns its own SQL Server schema (no shared databases)

## Microservices (10 Services)

| Service | Responsibility | Key Events |
|---------|---------------|------------|
| AuthAPI | Users, Roles, Tokens | UserRegistered |
| ProductAPI | Products, Categories | InventoryUpdated |
| ShoppingCartAPI | Carts, CartItems | CartCheckedOut |
| OrderAPI | Orders, Status | OrderPlaced, PaymentCompleted |
| CouponAPI | Coupons, PromoCodes | CouponApplied |
| RewardAPI | Points, Transactions | PointsRedeemed, RewardPointsEarned |
| EmailAPI | Email Templates, Send | — |
| Gateway | Routing, Rate Limiting | — |
| MessageBus | Event Routing (infrastructure) | — |
| Web (MVC) | UI, User Sessions | — |

## Critical Patterns (Non-Obvious)

- **Object mapping**: Use [Mapperly](https:// Mapperly.app) (compile-time, zero-alloc) — NOT AutoMapper
- **API Gateway**: Use YARP (not Ocelot) — Microsoft-supported reverse proxy
- **Message Broker**: MassTransit with RabbitMQ — all domain events publish via `Mango.MessageBus`
- **Consumers MUST be idempotent** — same event processed twice produces no side effects
- **HTTP resilience**: Polly v8 required for all inter-service calls (retry with exponential backoff + jitter)

## API Response Format

All APIs MUST return consistent envelope:
```json
{ "isSuccess": true, "result": {}, "statusCode": 200, "message": "" }
```

## Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Namespace | `Mango.Services.{Name}` | `Mango.Services.OrderAPI` |
| Interface | `I` prefix | `IOrderRepository` |
| Async method | `Async` suffix | `GetOrderAsync()` |
| Event | Past tense | `OrderPlaced`, `RewardPointsEarned` |
| Command | Imperative | `PlaceOrder` |

## Configuration Requirements

- `<TreatWarningsAsErrors>` enabled in Release builds
- `<Nullable>enable</Nullable>` required
- Use records (immutable) for DTOs and events
- No `public` fields — properties only

## Observability

- **Logging**: Serilog → Seq/ELK (structured logging only, no Console.WriteLine)
- **Health endpoints**: Every service exposes `/health` and `/health/ready`
- **Correlation IDs**: Propagated across all service calls

## Dependencies

- Runtime: .NET 10
- ORM: Entity Framework Core 10
- Language: C# 13
- Database: SQL Server 2022
- API Gateway: YARP 3.x
- Message Broker: RabbitMQ 3.13+ via MassTransit 8.x
- Caching: Redis 7.x
- Testing: xUnit + FluentAssertions + Testcontainers

## Port Assignments

| Service | Dev Port |
|---------|----------|
| AuthAPI | 7001 |
| ProductAPI | 7002 |
| ShoppingCartAPI | 7003 |
| OrderAPI | 7004 |
| CouponAPI | 7005 |
| RewardAPI | 7006 |
| EmailAPI | 7007 |
| Gateway | 7100 |
| Web (MVC) | 7200 |
