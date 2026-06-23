# Railway Deployment Guide

SquadCheck is set up to run on Railway with Docker, PostgreSQL, and ASP.NET Core configuration variables.

## What Is Already In The Repo

- `Dockerfile`
- `.dockerignore`
- Railway `PORT` binding in `Program.cs`
- PostgreSQL connection string support through `ConnectionStrings__SquadCheckConnection`

## Railway Setup

Create one Railway project with:

1. A PostgreSQL service
2. A web service connected to this repository

Set the web service variables with values from your Railway PostgreSQL service and your own public app URL. Keep SMTP credentials and reminder settings in Railway variables, not in source control.

Use a normal Npgsql connection string for `ConnectionStrings__SquadCheckConnection`. Do not paste `DATABASE_URL` directly.

When deploying from your own machine, run `dotnet ef database update` against Railway's external PostgreSQL TCP connection. The deployed web service itself should use the Railway-internal host.

## What To Verify

- the app starts without database errors
- Identity tables exist in PostgreSQL
- login and role-based access work
- groups, join codes, practices, and attendance work
- reminders only send when enabled
