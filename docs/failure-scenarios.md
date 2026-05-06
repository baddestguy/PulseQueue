# Failure Scenarios

Phase 1 intentionally keeps failure behavior simple.

```text
Worker fails
  |
  v
RabbitMQ requeues unacknowledged message
  |
  v
Another worker can consume it
```

Planned Phase 2 behavior:

```text
Worker fails
  |
  v
Retry with backoff
  |
  v
Max attempts reached
  |
  v
Dead-letter queue
  |
  v
Job marked DeadLettered
```
