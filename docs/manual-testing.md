# Manual Testing

Use this checklist to verify the Phase 1 flow locally.

## Start

```powershell
docker compose up --build -d
docker compose ps
```

Expected services:

```text
api
worker
postgres
rabbitmq
```

## Health Check

```powershell
Invoke-RestMethod -Method Get -Uri http://localhost:8080/health
```

Expected response:

```json
{
  "status": "ok"
}
```

## Submit Job

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

Expected initial status:

```text
Queued
```

## Check Status

```powershell
Start-Sleep -Seconds 2

Invoke-RestMethod -Method Get -Uri "http://localhost:8080/jobs/$($created.id)"
```

Expected final status:

```text
Succeeded
```

## Check Logs

```powershell
docker compose logs worker --tail=50
```

Look for:

```text
Processing job ...
Completed job ...
```

## Stop

```powershell
docker compose down
```
