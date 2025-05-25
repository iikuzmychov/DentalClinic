#!/bin/bash

CONTAINER_NAME=dental-clinic-db
POSTGRES_PASSWORD="yourStrong(!)Password"
DB_NAME="DentalClinic"
POSTGRES_USER="postgres"

docker run --name $CONTAINER_NAME \
    -e POSTGRES_PASSWORD="$POSTGRES_PASSWORD" \
    -e POSTGRES_DB="$DB_NAME" \
    -p 5432:5432 \
    -d postgres:16

if [ $? -ne 0 ]; then
  exit 1
fi

echo -n "Waiting for PostgreSQL..."

until docker exec $CONTAINER_NAME pg_isready -U $POSTGRES_USER &> /dev/null
do
    echo -n "."
    sleep 1
done

echo -e "\nPostgreSQL is ready."