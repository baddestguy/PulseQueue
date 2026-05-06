# System Design Notes

PulseQueue models an internal async processing platform:

- The database is the source of truth for job status.
- RabbitMQ is the delivery mechanism for work.
- Workers are horizontally scalable consumers.
- The API exposes a small client contract and keeps the submit path fast.
- Dapper repositories keep SQL explicit and make persistence behavior easy to inspect.

Initial statuses:

```text
Queued
Processing
Succeeded
Failed
DeadLettered
Cancelled
```

Phase 1 implements `Queued`, `Processing`, and `Succeeded`.

Core submit sequence:

```text
1. Client submits POST /jobs.
2. API stores the job as Queued.
3. API publishes JobQueuedMessage to RabbitMQ.
4. Worker consumes the message.
5. Worker reads the full job from PostgreSQL.
6. Worker marks the job Processing, then Succeeded.
7. Client reads status with GET /jobs/{id}.
```
