# Tradeoffs

## Raw SQL Initializer Instead Of Migrations

Phase 1 uses a small raw SQL database initializer so Docker Compose can bring up a working system with minimal ceremony. This is convenient for the first milestone but is not a production migration strategy.

Planned improvement: add a proper migration tool once the Phase 2 schema introduces retry counts, failure details, dead-letter metadata, and worker heartbeat fields.

## Dapper Instead Of EF Core

PulseQueue uses Dapper which means the repository contains the actual SQL for inserts, selects, and updates.

The tradeoff is that Dapper does not provide change tracking or schema migrations. The upside is that the queries are easy to inspect and explain.

## Publish After Database Commit

The API saves the job first and publishes the RabbitMQ message second. This avoids messages pointing to jobs that do not exist, but it leaves a possible gap if RabbitMQ publish fails after the database commit.

Planned improvement: add an outbox table so publishing becomes reliable and recoverable.
