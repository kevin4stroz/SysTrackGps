#!/bin/sh

# Arrancar Redis en modo daemon temporal para que los scripts puedan conectarse
redis-server /usr/local/etc/redis/redis.conf --daemonize yes

# Esperar a que Redis esté listo
REDIS_CLI="redis-cli -a C3JbTQR3zB82zogUKN"

until $REDIS_CLI ping | grep -q PONG; do
  echo "Esperando a Redis..."
  sleep 1
done

# Ejecutar scripts de inicialización
for f in /docker-entrypoint-initdb.d/*.sh; do
  [ -f "$f" ] && sh "$f"
done

# Detener el daemon
$REDIS_CLI shutdown

while pgrep -x redis-server > /dev/null; do
    echo "Esperando a que Redis se detenga..."
    sleep 1
done
# Finalmente, levantar Redis en foreground (modo normal)
exec redis-server /usr/local/etc/redis/redis.conf
