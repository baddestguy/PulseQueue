# Architecture

PulseQueue starts with a small distributed flow:

```text
Submit flow:

Client -> API -> PostgreSQL
             -> RabbitMQ Queue -> Worker -> PostgreSQL

Status flow:

Client -> API -> PostgreSQL -> API -> Client
```

The API owns client-facing HTTP concerns and persists the job before publishing a RabbitMQ message. Persisting first means the job has durable state even if the worker is offline.

The API is structured with controllers, services, and models. Persistence is handled through Dapper repositories in `PulseQueue.Infrastructure`, so the SQL for creating, reading, and updating jobs is explicit.

The worker consumes one RabbitMQ message at a time per instance, reads the job from PostgreSQL because the database is the source of truth, updates the job to `Processing`, performs the Phase 1 handler work, and then marks the job `Succeeded`.

Future phases will add retries, dead-letter handling, idempotency, heartbeats, and observability on this foundation.
