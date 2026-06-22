# Railway Deployment Guide

This repository is set up for Railway with:

- a `Dockerfile` in the project root
- ASP.NET Core binding to the Railway `PORT`
- PostgreSQL through `ConnectionStrings__SquadCheckConnection`

Railway requires a `Dockerfile` for ASP.NET Core apps because Railpack does not yet support .NET.

## What You Need In The Repo

Already added:

- [Dockerfile](Dockerfile)
- [.dockerignore](.dockerignore)
- runtime `PORT` handling in [Program.cs](Program.cs)

## What To Create In Railway

Create one Railway project with two services:

1. PostgreSQL database service
2. Web app service from this GitHub repo

## Step 1: Create The Railway Project

In Railway:

```text
New Project -> Empty Project
```

Then add services inside that project.

## Step 2: Add PostgreSQL

In Railway:

```text
New -> Database -> PostgreSQL
```

Give it a name such as:

```text
squadcheck-db
```

Railway will create connection variables for the database service, including:

- `PGHOST`
- `PGPORT`
- `PGUSER`
- `PGPASSWORD`
- `PGDATABASE`
- `DATABASE_URL`

## Step 3: Add The Web Service

In Railway:

```text
New -> GitHub Repo
```

Then select the `SquadCheck` repository and deploy it.

Railway will detect the `Dockerfile` in the project root and use it to build the app.

## Step 4: Set Web App Variables

Open the web service and go to:

```text
Variables
```

Add these variables one by one.

```text
ConnectionStrings__SquadCheckConnection
Host=YOUR_RAILWAY_PG_HOST;Port=5432;Database=YOUR_RAILWAY_DB_NAME;Username=YOUR_RAILWAY_PG_USER;Password=YOUR_RAILWAY_PG_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;
```

You can copy the values from the PostgreSQL service variables.

```text
App__BaseUrl
https://YOUR_RAILWAY_PUBLIC_DOMAIN
```

This is the public domain Railway gives your web service after you generate one.

```text
EmailSettings__Host
smtp.gmail.com
```

```text
EmailSettings__Port
587
```

```text
EmailSettings__Username
YOUR_GMAIL_ADDRESS
```

```text
EmailSettings__Password
YOUR_GMAIL_APP_PASSWORD
```

```text
EmailSettings__FromEmail
YOUR_GMAIL_ADDRESS
```

```text
EmailSettings__FromName
SquadCheck
```

```text
EmailSettings__EnableSsl
true
```

```text
ReminderSettings__Enabled
true
```

```text
ReminderSettings__CheckIntervalMinutes
15
```

```text
ReminderSettings__PracticeReminderHoursBefore
24
```

```text
ReminderSettings__MissingResponseHoursBefore
12
```

```text
ASPNETCORE_ENVIRONMENT
Production
```

Optional startup migration flag:

```text
Database__ApplyMigrationsOnStartup
false
```

Leave this `false` unless you intentionally want the app to apply migrations when it starts.

## Step 5: Add The Public Domain

In the web service:

```text
Settings -> Networking -> Generate Domain
```

Copy the generated Railway URL into:

```text
App__BaseUrl
```

## Step 6: Apply The Database Migrations

Run the migration script from your local repo against the Railway PostgreSQL connection string.

Example:

```powershell
.\scripts\apply-azure-migrations.ps1 -ConnectionString "Host=YOUR_RAILWAY_PG_HOST;Port=5432;Database=YOUR_RAILWAY_DB_NAME;Username=YOUR_RAILWAY_PG_USER;Password=YOUR_RAILWAY_PG_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;"
```

The script sets the connection string for that command only, runs `dotnet ef database update`, and then clears the variable from the current shell.

## Step 7: Verify The App

After deploy, test:

- register and log in
- admin login and role management
- create a group
- join a group with a join code
- create a practice
- create recurring practices
- mark attendance from the dashboard
- confirm email reminders are not duplicated

## What The Code Already Does

- Reads the PostgreSQL connection string from configuration.
- Reads email and reminder settings from configuration.
- Binds to Railway's injected `PORT` when `PORT` is present.
- Uses EF Core migrations for schema changes.
- Sends reminder emails when enabled.

## What You Must Do In Railway

- Create the project and services.
- Copy the database variables into the web service connection string.
- Add the public domain to `App__BaseUrl`.
- Add SMTP credentials.
- Apply migrations to the Railway database.
- Generate the public domain for the web service.
