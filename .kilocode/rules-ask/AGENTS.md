# AGENTS.md - Ask Mode

This file provides guidance to agents when working with code in this repository.

## Documentation Context (Non-Obvious)

- **Architecture**: Clean Architecture — Domain → Application → Infrastructure → Presentation per service
- **Shared contracts**: DTOs and events in `Mango.Contracts` package
- **Service ownership**: Each microservice owns its data (separate SQL Server schema)
- **API Gateway**: YARP-based — routes all external traffic; services not directly exposed
- **Event naming**: Domain events follow past tense (OrderPlaced, PaymentProcessed, RewardPointsEarned)
- **Response envelope**: All APIs return `{ isSuccess, result, statusCode, message }` — not raw data

## Technology Stack

- **Runtime**: .NET 10 with C# 13
- **Database**: SQL Server 2022 with Entity Framework Core 10
- **Message Broker**: RabbitMQ via MassTransit
- **API Gateway**: YARP (not Ocelot)
- **Object Mapping**: Mapperly (compile-time, not reflection-based)
- **Resilience**: Polly v8 for all HTTP calls between services

## Service Responsibility Map

| Service | Owns | Publishes | Consumes |
|---------|------|-----------|----------|
| AuthAPI | Users, Roles, Tokens | UserRegistered | — |
| ProductAPI | Products, Categories | InventoryUpdated | OrderPlaced |
| ShoppingCartAPI | Carts, CartItems | CartCheckedOut | — |
| OrderAPI | Orders, Status | OrderPlaced, PaymentCompleted | CartCheckedOut |
| CouponAPI | Coupons, PromoCodes | CouponApplied | — |
| RewardAPI | Points, Transactions | PointsRedeemed, RewardPointsEarned | OrderPlaced |
| EmailAPI | Email Templates, Send | — | OrderPlaced, UserRegistered |

## Key Boundaries

- Services communicate asynchronously via RabbitMQ by default
- Synchronous calls only through API Gateway (YARP)
- JWT authentication for both user and service-to-service calls
- Each service has independent deployment lifecycle
