# AGENTS.md - Debug Mode

This file provides guidance to agents when working with code in this repository.

## Debug Rules (Non-Obvious)

- **Logging**: Serilog structured logging to Seq/ELK — never use Console.WriteLine
- **Health endpoints**: Every service exposes `/health` and `/health/ready` — check these first
- **Correlation IDs**: Propagated across all service calls — check request headers for trace correlation
- **Event failures**: RabbitMQ consumers log to Seq — check for "PaymentFailed", "OrderProcessingError" events
- **Database connections**: Each service owns its own schema — check connection string for correct database
- **Integration test failures**: Testcontainers spin up real SQL Server + RabbitMQ — ensure Docker is running
- **Polly retry logs**: Check for "Retry needed" in Serilog — indicates transient failures or downstream issues

## Common Debug Scenarios

- **Service won't start**: Check environment variables for connection strings (not in source code)
- **Message not processed**: Check RabbitMQ queue for unacknowledged messages
- **API returns 500**: Check Serilog for stack traces — response envelope hides details
- **Testcontainer failures**: Docker must be running; Windows containers not supported for SQL Server
