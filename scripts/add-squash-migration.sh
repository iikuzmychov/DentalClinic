#!/bin/bash

migration_name="$1"
verbose_parameter=""

if [ -z "$migration_name" ]; then
  migration_name="InitialMigration"
fi

if [ "$2" == "--verbose" ]; then
  verbose_parameter="--verbose"
fi

dotnet tool update --global dotnet-ef
rm -rf ../src/DentalClinic.Infrastructure/Migrations
dotnet ef migrations add "$migration_name" --project ../src/DentalClinic.Infrastructure --output-dir "Migrations" --context ApplicationDbContext $verbose_parameter