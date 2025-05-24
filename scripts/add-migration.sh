#!/bin/bash

migration_name="$1"
verbose_parameter=""

if [ -z "$migration_name" ]; then
  echo "Migration name is not provided."
  exit 1
fi

if [ "$2" == "--verbose" ]; then
  verbose_parameter="--verbose"
fi

dotnet tool update --global dotnet-ef
dotnet ef migrations add "$migration_name" --project ../src/DentalClinic.Infrastructure --output-dir "Migrations" --context ApplicationDbContext $verbose_parameter