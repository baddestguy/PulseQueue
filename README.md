# PulseQueue

PulseQueue is a distributed job processing platform built in .NET to demonstrate production-grade backend architecture: REST APIs, async messaging, durable job state, RabbitMQ workers, Dapper-based PostgreSQL persistence, Dockerized infrastructure, and a foundation for retries, idempotency, observability, and failure recovery.

## Phase 1 Scope

- `POST /jobs` submits a job.
- `GET /jobs/{id}` returns job status and metadata.
- PostgreSQL stores job state.
- RabbitMQ carries job messages.
- A worker consumes messages and marks jobs completed.
- Docker Compose runs the API, worker, PostgreSQL, and RabbitMQ.

## Architecture

```text
Client
  |
  v
PulseQueue.Api
  |
  +--> PostgreSQL jobs table
  |
  +--> RabbitMQ pulsequeue.jobs queue
          |
          v
      PulseQueue.Worker
          |
          v
      PostgreSQL jobs table
```

## Run Locally

```powershell
docker compose up --build -d
```

The API listens on `http://localhost:8080`.

RabbitMQ Management is available at `http://localhost:15672` with `guest` / `guest`.

Check that the containers are running:

```powershell
docker compose ps
```

Check API health:

```powershell
Invoke-RestMethod -Method Get -Uri http://localhost:8080/health
```

## Submit A Job

```powershell
$body = @{
  type = "SendEmail"
  payload = @{
    to = "customer@example.com"
    template = "InvoiceReminder"
  }
} | ConvertTo-Json

$created = Invoke-RestMethod -Method Post -Uri http://localhost:8080/jobs -ContentType "application/json" -Body $body

$created
```

The response includes the job id and starts in `Queued`. The worker should move it through `Processing` to `Succeeded`.

## Check Status

```powershell
Start-Sleep -Seconds 2

Invoke-RestMethod -Method Get -Uri "http://localhost:8080/jobs/$($created.id)"
```

The job should move from `Queued` to `Processing` to `Succeeded`.

## Inspect Logs

```powershell
docker compose logs worker --tail=50
docker compose logs api --tail=50
```

## Stop Locally

```powershell
docker compose down
```

## Project Structure

```text
src/
  PulseQueue.Api/
  PulseQueue.Worker/
  PulseQueue.Domain/
  PulseQueue.Infrastructure/
docs/
docker-compose.yml
```

The API uses controller/service classes. Data access uses Dapper repositories with explicit SQL.

## Roadmap

- Phase 2: retries, exponential backoff, failed reason, dead-letter queue, idempotency key, worker heartbeat.
- Phase 3: multiple workers, optimistic concurrency, Redis cache, scheduler distributed lock, rate limiting.
- Phase 4: Serilog, correlation IDs, tracing, metrics, Seq.
