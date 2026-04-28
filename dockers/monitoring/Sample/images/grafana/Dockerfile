FROM grafana/grafana:11.6.11

ENV GF_INSTALL_PLUGINS="https://storage.googleapis.com/integration-artifacts/grafana-exploretraces-app/grafana-exploretraces-app-latest.zip;grafana-traces-app,https://storage.googleapis.com/integration-artifacts/grafana-lokiexplore-app/release/main/any/grafana-lokiexplore-app-main.zip;grafana-lokiexplore-app"

COPY grafana/provisioning/ /etc/grafana/provisioning/
COPY grafana/dashboards/ /var/lib/grafana/dashboards/
