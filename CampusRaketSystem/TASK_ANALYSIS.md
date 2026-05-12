# CampusRaket Task Analysis

## What was completed here

- Verified that XAMPP is installed at `C:\xampp`.
- Verified that `Apache` and `MySQL` are already running.
- Verified that database `campusraketdb` already exists.
- Verified these tables/views are present:
  - `clients`
  - `freelancers`
  - `jobs`
  - `payments`
  - `proposals`
  - `v_activejobs`
  - `v_clientfinancialsummary`
  - `v_platformrevenue`
- Verified this stored procedure is present:
  - `sp_GetFreelancerProposals`
- Created a WinForms project at `Edp_Gui/CampusRaketSystem`.
- Added the `MySql.Data` NuGet package to the project.
- Replaced the starter form with:
  - `Program.cs`
  - `DbHelper.cs`
  - `FrmLogin.cs`
  - `FrmPasswordRecovery.cs`
  - `FrmAbout.cs`
  - `FrmDashboard.cs`
  - `FrmReportGenerator.cs`

## What could not be done exactly as written here

- I could not create a **Windows Forms App (.NET Framework)** from the terminal because the installed `dotnet` templates only support modern `.NET` (`net6.0` to `net10.0`) in this environment.
- Because of that, the project was scaffolded as a **WinForms app targeting `net8.0-windows`**, not classic `.NET Framework`.
- I did not use the Visual Studio Designer to drag/drop controls. The forms were built directly in C# code so the app can still compile and run.
- I could not perform the phpMyAdmin import step because no CampusRaket SQL file was available in this workspace, and the database was already present with the required objects.

## Manual follow-up if your teacher strictly requires `.NET Framework`

1. Open the folder in Visual Studio.
2. Create a new **Windows Forms App (.NET Framework)** project named `CampusRaketSystem`.
3. Copy the source files from this generated project into that Visual Studio project.
4. Reinstall `MySql.Data` through NuGet inside the `.NET Framework` project.
5. If desired, recreate the forms in the Designer for a more traditional WinForms submission.

## Manual follow-up if MySQL root uses a password

- Update the connection string in `DbHelper.cs`:
  - From: `pwd=;`
  - To: `pwd=yourpassword;`

## Optional upgrade for real login

- The current login and password recovery flow are demo-only and match the task instructions.
- If you want real database-backed login, create a `users` table and update the forms to query it.
