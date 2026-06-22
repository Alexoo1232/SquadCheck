# SquadCheck

SquadCheck is a simple ASP.NET Core MVC application for managing sports groups, practice sessions, and attendance.

## What The Program Can Do

- Create sports groups or teams.
- Add members to a group with the role Member or Coach.
- Create practice sessions for a group.
- Edit or cancel practice sessions.
- Mark attendance for group members as Going, Maybe, or Not going.
- Add an optional attendance comment.
- View attendance summaries for each practice session.
- Save and load data from a local JSON file.

## Requirements

- .NET SDK installed.
- Visual Studio or another editor that can run ASP.NET Core projects.

## How To Run

1. Open the project folder in Visual Studio.
2. Open the solution file:

   ```text
   SquadCheck.slnx
   ```

3. Press `F5` or click the green run button.
4. The application will open in the browser.

You can also run it from the terminal:

```powershell
dotnet run
```

Then open the local URL shown in the terminal.

## Deployment

For Railway deployment, see [RAILWAY_DEPLOYMENT.md](RAILWAY_DEPLOYMENT.md).

## Basic Usage

1. Go to the Groups page.
2. Create a group.
3. Open the group and add members.
4. Create a practice session for the group.
5. Open the practice session.
6. Choose a member and mark their attendance.
7. Open the attendance summary to see the results.

## Data Storage

The application stores its data in:

```text
App_Data/squadCheckData.json
```

This file keeps saved users, groups, practice sessions, and attendance records between application restarts.

Have fun managing your sports groups with SquadCheck!
