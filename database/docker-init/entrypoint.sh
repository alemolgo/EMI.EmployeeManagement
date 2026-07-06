#!/bin/bash
set -e

if [ "$1" = 'postgres' ]; then
  /usr/local/bin/docker-entrypoint.sh "$@" &
  ENTRYPOINT_PID=$!

  echo "Waiting for PostgreSQL to accept connections..."
  until pg_isready -U "$POSTGRES_USER" -d postgres >/dev/null 2>&1; do
    sleep 1
  done

  /docker-init/apply_schema.sh

  wait "$ENTRYPOINT_PID"
else
  exec /usr/local/bin/docker-entrypoint.sh "$@"
fi
