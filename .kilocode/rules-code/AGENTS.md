# AGENTS.md - Code Mode

This file provides guidance to agents when working with code in this repository.

## Code-Specific Rules (Non-Obvious)

- **Object mapping**: ALWAYS use Mapperly (compile-time, zero-alloc) — NOT AutoMapper or manual mapping
- **API Gateway**: Use YARP (Microsoft-supported) — NOT Ocelot
- **Message publishing**: Use `Mango.MessageBus` for all RabbitMQ publishing
- **Consumers MUST be idempotent** — design event handlers to handle duplicate events gracefully
- **HTTP calls**: MUST use Polly v8 with exponential backoff + jitter (no raw HttpClient)

## Naming Enforcement

- Namespaces: `Mango.Services.{Name}` (e.g., `Mango.Services.OrderAPI`)
- Interfaces: `I` prefix (e.g., `IOrderRepository`)
- Async methods: `Async` suffix (e.g., `GetOrderAsync()`)
- Events: Past tense (e.g., `OrderPlaced`, `RewardPointsEarned`)
- Commands: Imperative (e.g., `PlaceOrder`)

## Required Patterns

- All DTOs/events: Use `record` (immutable)
- No `public` fields — properties only
- `<Nullable>enable</Nullable>` in all projects
- API responses: Always wrap in `{ isSuccess, result, statusCode, message }`

## Domain Events

| Event | Publisher | Consumers |
|-------|-----------|------------|
| OrderPlaced | OrderAPI | EmailAPI, RewardAPI, ProductAPI |
| PaymentCompleted | OrderAPI | OrderAPI |
| UserRegistered | AuthAPI | EmailAPI |
| CartCheckedOut | ShoppingCartAPI | OrderAPI |
| CouponApplied | CouponAPI | ShoppingCartAPI |
| PointsRedeemed | RewardAPI | OrderAPI |
| RewardPointsEarned | RewardAPI | — |
| InventoryUpdated | ProductAPI | — |

## Testing Requirements

- Unit tests: xUnit + Moq + FluentAssertions (≥85% coverage)
- Integration tests: Testcontainers (real SQL Server + RabbitMQ, not mocks)
- Contract tests: Pact.NET for all public APIs
- No `Thread.Sleep` — use Polly retry policies
