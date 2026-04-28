# CinemaTicketBooking Development Docker Compose

Bộ compose này chạy môi trường development đầy đủ cho CinemaTicketBooking:

- WebServer ASP.NET Core (`CinemaTicketBooking.WebServer`)
- WebApp React/Vite (`CinemaTicketBooking.WebApp`)
- PostgreSQL 17
- Redis 7.4
- Prometheus + PostgreSQL exporter + Redis exporter
- cAdvisor cho container disk/network I/O metrics
- Loki
- Tempo
- Grafana với datasource và dashboard mẫu

Các biến observability đang khớp với cấu hình trong `src/Aspire.ServiceDefaults/Extensions.cs`:

- `LOKI_ENDPOINT=http://loki:3100` để Serilog đẩy log sang Loki.
- `TEMPO_OTLP_ENDPOINT=http://tempo:4317` để OpenTelemetry đẩy trace sang Tempo bằng OTLP gRPC.
- `/metrics` được expose bởi `MapPrometheusScrapingEndpoint()` để Prometheus scrape app metrics.

## Yeu Cau

- Docker Desktop đang chạy.
- Port mặc định còn trống: `5173`, `8080`, `8081`, `5432`, `6379`, `3000`, `9090`, `3100`, `3200`, `4317`, `9187`, `9121`.

## Chay Moi Truong Dev

Từ thư mục này:

```powershell
cd .\dockers\development
.\run.ps1 -Build
```

Lần đầu chạy, script sẽ tự tạo `.env` từ `.env.example`. Có thể sửa `.env` nếu muốn đổi port, mật khẩu PostgreSQL/Redis hoặc tài khoản Grafana.

## Chay Compose Bang Command Line

Nếu không dùng `run.ps1`, có thể chạy trực tiếp bằng Docker Compose CLI.

Từ thư mục `dockers/development`:

```powershell
cd .\dockers\development
Copy-Item .env.example .env
docker compose --project-name cinema-ticket-booking-dev --env-file .env -f docker-compose.yml up -d --build
```

Nếu đã có file `.env`, bỏ qua bước `Copy-Item`.

Xem trạng thái containers:

```powershell
docker compose --project-name cinema-ticket-booking-dev --env-file .env -f docker-compose.yml ps
```

Xem logs toàn bộ stack:

```powershell
docker compose --project-name cinema-ticket-booking-dev --env-file .env -f docker-compose.yml logs -f --tail 200
```

Xem logs một service cụ thể:

```powershell
docker compose --project-name cinema-ticket-booking-dev --env-file .env -f docker-compose.yml logs -f webserver
docker compose --project-name cinema-ticket-booking-dev --env-file .env -f docker-compose.yml logs -f webapp
```

Restart một service:

```powershell
docker compose --project-name cinema-ticket-booking-dev --env-file .env -f docker-compose.yml restart webserver
docker compose --project-name cinema-ticket-booking-dev --env-file .env -f docker-compose.yml restart webapp
```

Down nhưng giữ dữ liệu trong volumes:

```powershell
docker compose --project-name cinema-ticket-booking-dev --env-file .env -f docker-compose.yml down --remove-orphans
```

Down và dọn sạch toàn bộ volumes:

```powershell
docker compose --project-name cinema-ticket-booking-dev --env-file .env -f docker-compose.yml down --volumes --remove-orphans
```

## Chay Bang Visual Studio

Project Docker Compose cho Visual Studio nằm tại:

```text
dockers/development/docker-compose.dcproj
```

Project này đã được thêm vào `CinemaTicketBooking.slnx` trong solution folder `dockers/development`.

Cách chạy:

1. Mở `CinemaTicketBooking.slnx` bằng Visual Studio.
2. Chọn startup project là `docker-compose`.
3. Chạy bằng Docker Compose profile của Visual Studio.
4. Visual Studio sẽ chạy service chính `webapp` và mở <http://localhost:5173>.

Nếu cần đổi port hoặc password khi chạy bằng Visual Studio, tạo file `.env` trong `dockers/development` dựa trên `.env.example`. Docker Compose sẽ tự đọc `.env` cùng thư mục với compose file.

Các lệnh hữu ích:

```powershell
.\run.ps1
.\run.ps1 -Status
.\run.ps1 -Logs
```

## URL Mac Dinh

- WebServer: <http://localhost:8080>
- WebApp: <http://localhost:5173>
- Grafana: <http://localhost:3000> (`admin/admin` mặc định)
- Prometheus: <http://localhost:9090>
- Loki ready endpoint: <http://localhost:3100/ready>
- Tempo API: <http://localhost:3200>
- PostgreSQL exporter metrics: <http://localhost:9187/metrics>
- Redis exporter metrics: <http://localhost:9121/metrics>
- cAdvisor: <http://localhost:8081>

## Kiem Tra Monitoring

1. Mở Prometheus targets: <http://localhost:9090/targets>
2. Kiểm tra các job đang `UP`:
   - `cinematicketbooking-webserver`
   - `postgres-exporter`
   - `redis-exporter`
   - `cadvisor`
   - `prometheus`
3. Mở Grafana: <http://localhost:3000>
4. Vào folder dashboard `CinemaTicketBooking`.
5. Mở dashboard `CinemaTicketBooking Development Overview`.
6. Kiểm tra các panel chính:
   - Web requests/sec và p95 latency.
   - WebServer Exceptions/s.
   - WebServer CPU/RAM usage.
   - Docker Disk I/O và Network I/O qua cAdvisor.
   - PostgreSQL và Redis activity.

Nếu không thấy panel `Docker Disk I/O` và `Docker Network I/O`, Grafana có thể vẫn đang dùng dashboard đã provision trước đó trong volume. Restart Grafana để reload provisioning:

```powershell
docker compose --project-name cinema-ticket-booking-dev --env-file .env -f docker-compose.yml restart grafana
```

Nếu vẫn không thấy sau khi restart, dọn volume Grafana rồi chạy lại stack:

```powershell
docker compose --project-name cinema-ticket-booking-dev --env-file .env -f docker-compose.yml down --volumes --remove-orphans
docker compose --project-name cinema-ticket-booking-dev --env-file .env -f docker-compose.yml up -d --build
```

Dashboard mẫu được đặt tại:

```text
dockers/development/grafana/dashboards/cinema-overview.json
```

Datasource provisioning:

```text
dockers/development/grafana/provisioning/datasources/datasources.yml
```

## Metrics PostgreSQL Redis Va Container I/O

PostgreSQL metrics được thu thập qua `postgres-exporter`:

- Connections theo database.
- Transaction commit/rollback rate.
- Các metric mặc định từ `pg_stat_database`, locks, sessions và collector stats.

Redis metrics được thu thập qua `redis_exporter`:

- Commands processed rate.
- Connected clients.
- Memory usage.
- System metrics của Redis container.
- Key pattern `CinemaTicketBooking:*` để quan sát cache/keyspace của app.

Docker Disk/Network I/O được thu thập qua `cadvisor`:

- Disk read/write bytes per second từ `container_fs_sector_reads_total` và `container_fs_sector_writes_total`.
- Network receive/transmit bytes per second từ `container_network_receive_bytes_total` và `container_network_transmit_bytes_total`.
- Trên Docker Desktop/WSL, cAdvisor có thể chỉ trả aggregate/root I/O thay vì per-container I/O, nên dashboard hiển thị Docker host I/O aggregate để tránh `No data`.

## Logs Va Traces

Log app được gửi sang Loki qua `LOKI_ENDPOINT`. Trong Grafana Explore, chọn datasource `Loki` và thử query:

```logql
{deployment_environment="Development"}
```

Trace app được gửi sang Tempo qua `TEMPO_OTLP_ENDPOINT`. Trong Grafana Explore, chọn datasource `Tempo`, sau đó query trace sau khi tạo traffic vào WebServer.

Dashboard logs dùng LogQL để parse JSON log từ Loki và hiển thị dạng gọn:

```logql
{deployment_environment="Development"} !~ "/metrics|Executed DbCommand" | json | line_format "[{{.level}}] {{.Message}}"
```

Query này bỏ qua log scrape `/metrics` và log EF SQL command nhiều dòng để panel `Recent Development Logs (Formatted)` dễ đọc hơn.

## Down Va Don Dep Hoan Toan

Từ thư mục `dockers/development`:

```powershell
.\down-clean.ps1
```

Lệnh này xóa containers, network của compose và toàn bộ named volumes, bao gồm dữ liệu PostgreSQL, Redis, Prometheus, Loki, Tempo và Grafana.

Nếu muốn xóa thêm image local được build bởi compose:

```powershell
.\down-clean.ps1 -RemoveBuiltImages
```

## Cau Truc File

```text
dockers/development/
  docker-compose.yml
  .env.example
  run.ps1
  down-clean.ps1
  ../../src/CinemaTicketBooking.WebApp/Dockerfile
  prometheus/prometheus.yml
  loki/loki-config.yml
  tempo/tempo.yml
  grafana/provisioning/datasources/datasources.yml
  grafana/provisioning/dashboards/dashboards.yml
  grafana/dashboards/cinema-overview.json
```
