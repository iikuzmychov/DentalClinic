#!/bin/bash

CONTAINER_NAME=dental-clinic-db
SA_PASSWORD="yourStrong(!)Password"
DB_NAME="DentalClinic"

docker run --name $CONTAINER_NAME \
    -e "ACCEPT_EULA=Y" \
    -e "MSSQL_SA_PASSWORD=$SA_PASSWORD" \
    -p 1433:1433 \
    -d mcr.microsoft.com/mssql/server:2022-latest

if [ $? -ne 0 ]; then
  exit 1
fi

echo -n "Waiting for SQL Server..."

until docker exec $CONTAINER_NAME //opt/mssql-tools18/bin/sqlcmd \
    -S 127.0.0.1 -U sa -P "$SA_PASSWORD" -Q "SELECT 1" -C &> /dev/null
do
    echo -n "."
    sleep 1
done

echo -e "\nSQL Server is ready."

docker exec -i $CONTAINER_NAME //opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" -C \
    -Q "IF DB_ID('$DB_NAME') IS NULL CREATE DATABASE [$DB_NAME];"

echo "Database '$DB_NAME' was successfully created."
