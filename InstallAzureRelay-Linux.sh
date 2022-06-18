#!/usr/bin/env sh
set -eu

drop_path="$1"
connection_string="$2"
relay_name="$3"
bind_address="$4"
port="$5"

# Install package
apt-get install $drop_path

# Modify config file
config_path=/etc/azbridge/azbridge_config.svc.yml
install -D -m 644 /dev/null "config_path"
cat <<EOF > $config_path
LocalForward:
- RelayName: $relay_name
  ConnectionString: $connection_string
  BindAddress: $bind_address
  BindPort: $port
EOF

# Create and start azbridge systemd service
service_name=azbridge.service
service_path=/etc/systemd/system/$service_name
install -D -m 644 /dev/null "$service_path"
cat <<EOF > "$service_path"
[Unit]
Description=Azure Relay Bridge
Before=vsts-provisioner.service
After=network.target

[Service]
ExecStart=/usr/share/azbridge/azbridge -f /etc/azbridge/azbridge_config.svc.yml
Restart=on-failure
RestartSec=15

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable "$service_name"
systemctl start "$service_name"
