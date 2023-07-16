# Fountain.Migrations

An F# migrations library I'm testing out to power Fountain.


## Random Database Scripts

Create database
```sql
CREATE DATABASE fountain
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    CONNECTION LIMIT = -1;
```

Setting up database:
```sql
CREATE SCHEMA IF NOT EXISTS fount;

CREATE USER app WITH PASSWORD 'fountain_cli_365';

GRANT ALL PRIVILEGES ON DATABASE fountain TO app;

GRANT ALL ON SCHEMA fount TO app;
```

## Repack tool

```
dotnet pack .\Fountain.Migrations.fsproj /p:Version=0.0.1

dotnet tool install --global --add-source "./nupkg" "Fount.Cli"
```
