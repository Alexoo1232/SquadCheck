# SquadCheck

SquadCheck is an ASP.NET Core MVC application for managing sports groups, practice sessions, and attendance.

## What The Program Can Do

- Create sports groups or teams.
- Add members to a group with the role Member or Coach.
- Register and log in with ASP.NET Core Identity.
- Use Admin, Coach, and Member roles.
- Let members join groups with a join code or invite link.
- Create practice sessions for a group.
- Create recurring practice sessions.
- Edit or cancel practice sessions.
- Mark your own attendance as Going, Maybe, or Not going.
- Add an optional attendance comment.
- View attendance summaries for each practice session.
- View a mobile-friendly dashboard with the next practice.
- Send email reminders when SMTP settings and reminders are enabled.
- Store users, groups, practices, attendance, and reminders in PostgreSQL.

## Requirements

- .NET SDK installed.
- PostgreSQL installed locally, or access to a PostgreSQL database.
- Visual Studio or another editor that can run ASP.NET Core projects.

## How To Run

1. Create a local PostgreSQL database, for example:

   ```text
   squadcheck_dev
   ```

2. Copy `appsettings.Development.sample.json` to `appsettings.Development.json`.
3. Update the local connection string:

   ```json
   "ConnectionStrings": {
     "SquadCheckConnection": "Host=localhost;Port=5432;Database=squadcheck_dev;Username=postgres;Password=<local-password>"
   }
   ```

4. Apply the EF Core migrations:

   ```powershell
   dotnet ef database update
   ```

5. Open the project folder in Visual Studio.
6. Open the solution file:

   ```text
   SquadCheck.slnx
   ```

7. Press `F5` or click the green run button.
8. The application will open in the browser.

You can also run it from the terminal:

```powershell
dotnet run
```

Then open the local URL shown in the terminal.

## Deployment

For Railway deployment, see [RAILWAY_DEPLOYMENT.md](RAILWAY_DEPLOYMENT.md).

## Basic Usage

1. Register or log in.
2. If you want a seeded admin account in local development, set the values in `appsettings.Development.json`.
3. Create or promote a Coach account.
4. As Admin or Coach, create a group.
5. Share the group join code or invite link with members.
6. Create one practice or recurring practices for the group.
7. As a Member, open the dashboard and mark your own attendance.
8. As a Coach, open practice details to view attendance summaries.

## Data Storage

The application stores its data in PostgreSQL using Entity Framework Core migrations.

For local development, configure:

```text
ConnectionStrings:SquadCheckConnection
```

For Railway deployment, configure the same setting as an environment variable:

```text
ConnectionStrings__SquadCheckConnection
```
