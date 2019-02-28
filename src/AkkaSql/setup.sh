#!/usr/bin/env bash

# start SQL Server
/opt/mssql/bin/sqlservr

#wait for the SQL Server to come up
sleep 90s

# setup the tables
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ${SA_PASSWORD} -d master -i setup.sql

/bin/bash