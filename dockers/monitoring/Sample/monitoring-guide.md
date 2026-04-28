# Monitoring Guide (Prometheus + Loki + Jaeger + Tempo + Grafana)

## 1) Prerequisites

- Docker Desktop is installed and running.
- .NET 10 SDK is installed.
- Project runs through Aspire AppHost (`src/Aspire.AppHost`).
- Ports are free: `3000`, `3100`, `3200`, `4317`, `4318`, `4319`, `9090`, `16686`, exporter ports `9121`, `9187`, and service ports `5000-5003`.

## 2) Start monitoring stack

From project root:

```powershell
.\monitoring\run-all.ps1
```

Useful commands:

```powershell
.\monitoring\run-all.ps1 -Status
.\monitoring\run-all.ps1 -Logs
.\monitoring\run-all.ps1 -Down
```

Default URLs:

- Grafana: <http://localhost:3000> (`admin/admin`)
- Prometheus: <http://localhost:9090>
- Jaeger UI: <http://localhost:16686>
- Tempo API: <http://localhost:3200>
- Loki ready endpoint: <http://localhost:3100/ready>
- Postgres exporter metrics: <http://localhost:9187/metrics>
- Redis exporter metrics: <http://localhost:9121/metrics>

## 3) Start the microservices with Aspire

Run AppHost:

```powershell
dotnet run --project .\src\Aspire.AppHost\Aspire.AppHost.csproj
```

AppHost injects shared observability settings into APIGateway and all services:

- Keep default Aspire OTLP env vars untouched so Aspire Dashboard can receive traces/metrics.
- Extra Jaeger trace export endpoint: `JAEGER_OTLP_ENDPOINT=http://localhost:4317`
- Extra Tempo trace export endpoint: `TEMPO_OTLP_ENDPOINT=http://localhost:4319`
- Loki endpoint for logs: `http://localhost:3100`
- Fixed service URLs for Prometheus scrape:
  - APIGateway: `http://localhost:5000/metrics`
  - Catalog API: `http://localhost:5001/metrics`
  - Inventory API: `http://localhost:5002/metrics`
  - Order API: `http://localhost:5003/metrics`
- Fixed infra endpoints for exporter/plugin scrape:
  - PostgreSQL host port: `55432` (used by `postgres-exporter`)
  - Redis TLS host port: `56379` (service connection)
  - Redis non-TLS host port: `6380` (used by `redis-exporter`)
  - RabbitMQ management: `15672`
  - RabbitMQ Prometheus plugin: `15692/metrics`

## 4) Quick verification checklist

1. **Metrics (Prometheus)**
   - Open <http://localhost:9090/targets>
   - Verify all jobs are `UP`: `apigateway`, `catalog-api`, `inventory-api`, `order-api`, `postgres-exporter`, `rabbitmq`, `redis-exporter`

2. **Tracing (Jaeger)**
   - Open <http://localhost:16686/search>
   - Trigger some HTTP requests to services.
   - Verify traces appear for each service.

3. **Tracing (Tempo for Grafana Traces tab)**
   - Open Grafana Explore and select **Tempo** datasource.
   - Search traces by service name.
   - Verify traces are returned.

4. **Logging (Loki)**
   - Open Grafana Explore.
   - Select **Loki** datasource.
   - Run query: `{app="eshop-microservices"}`
   - Verify logs are returned from service names.

5. **Dashboard (Grafana)**
   - Open dashboard folder **EShop**
   - Open dashboard **EShop Monitoring Overview**
   - Verify:
     - `Targets Up (App + Infra)` đạt `7`
     - `HTTP Request Rate` chart has data
     - `Recent Service Logs` shows entries
     - Infra panels have data:
       - PostgreSQL connections / transactions
       - RabbitMQ queue depth / publish-ack / consumers
       - Redis ops / hit ratio / memory / clients

## 5) Troubleshooting

### Prometheus targets are DOWN

- Ensure services are actually running on ports `5000-5003`.
- Verify `/metrics` endpoint manually:
  - <http://localhost:5000/metrics>
  - <http://localhost:5001/metrics>
  - <http://localhost:5002/metrics>
  - <http://localhost:5003/metrics>
- Check Docker can resolve `host.docker.internal` (Docker Desktop required).

### Infra metrics targets are DOWN

- Verify exporter containers are running:
  ```powershell
  docker compose -f .\monitoring\docker-compose.yml ps
  ```
- Verify exporter endpoints:
  - <http://localhost:9187/metrics>
  - <http://localhost:9121/metrics>
- Verify RabbitMQ plugin metrics endpoint:
  - <http://localhost:15692/metrics>
- If RabbitMQ metrics is DOWN, restart AppHost so RabbitMQ resource is recreated with metrics endpoint mapping.

### No traces in Jaeger

- Confirm Jaeger container is running and port `4317` is exposed.
- Verify service env vars in AppHost include:
  - `JAEGER_OTLP_ENDPOINT=http://localhost:4317`
- Generate traffic after startup; traces appear only when requests happen.

### Grafana Traces page shows empty datasource

- Traces Drilldown relies on **Tempo** datasource, not Jaeger datasource.
- Confirm Tempo container is running on port `3200` and OTLP gRPC receiver on `4319`.
- Verify AppHost sets `TEMPO_OTLP_ENDPOINT=http://localhost:4319`.

### No logs in Loki

- Confirm Loki ready endpoint is healthy: <http://localhost:3100/ready>
- Verify AppHost sets `LOKI_ENDPOINT=http://localhost:3100`.
- Confirm service startup logs do not show sink connection errors.

### Grafana has no datasources or dashboard

- Restart Grafana container:
  ```powershell
  docker compose -f .\monitoring\docker-compose.yml restart grafana
  ```
- Check provisioning files exist:
  - `monitoring/grafana/provisioning/datasources/datasources.yml`
  - `monitoring/grafana/provisioning/dashboards/dashboards.yml`
  - `monitoring/grafana/dashboards/eshop-overview.json`

## 6) Operational notes

- This setup is for development/local observability.
- Data is persisted in named Docker volumes (`prometheus_data`, `loki_data`, `tempo_data`, `grafana_data`).
- If you need a clean reset:

```powershell
docker compose -f .\monitoring\docker-compose.yml down -v
```
