# CampusRaket System - Activity Requirements Analysis

This document analyzes how the **CampusRaket System** fulfills the exact requirements of your Information System (IS) project activity.

## 1. User Management Module Integration (Activity 5)
The system fully integrates user management concepts (from your Activity 5 module) through a dedicated user authentication and authorization flow:
- **`FrmLogin.cs` & `FrmPasswordRecovery.cs`**: Handles user authentication.
- **`FrmUserManagement.cs`**: Provides administrative features to manage users.
- **`UserAccountService.cs` & `AuthenticatedUser`**: The currently logged-in user is tracked across the system. This context is used to securely identify who is performing transactions and automatically fills out the signature placeholder when generating reports.

## 2. Three Primary Transactions
The system implements three distinct and primary transactions essential to a freelance job platform:
1. **Jobs Transaction (`FrmJobs.cs`)**: Allows users to create, review, and manage job postings for freelancers on campus.
2. **Proposals Transaction (`FrmProposals.cs`)**: Manages the submissions and statuses of proposals sent by freelancers bidding on active jobs.
3. **Payments Transaction (`FrmPayments.cs`)**: Records and processes financial transactions when clients pay freelancers for completed jobs.

## 3. Report Generation Module with Data Grid Control
The reporting module is centralized in **`FrmReportGenerator.cs`**:
- Users select the type of report they wish to generate from a dropdown (e.g., Clients Report, Jobs Report, Payments Report).
- Upon clicking **Preview**, the system queries the database and binds the resulting records directly into a **Data Grid control (`DataGridView` named `dgvReport`)**. This provides a clear, on-screen list of the data before exporting.

## 4. MS Excel Export Button & Template Requirements
Inside the Report Generator form, an **"Export Excel"** button triggers the **`ExcelReportExporter.cs`** service, which strictly adheres to the requested template format:

### A. Header with Company Name and Logo
- The first sheet (`Report`) generates a professional header.
- Cell `A1` displays the company name ("CampusRaket") in large, bold font.
- The system automatically loads and inserts a company logo (`campusraket-logo-placeholder.png`) into the top header section.

### B. Signature Placeholder
- At the bottom of the data grid in the Excel file, the system dynamically generates a signature block.
- It pulls the active user's credentials (`signedByUser.FullName` and `signedByUser.Email`) and creates a visual signature line, strictly meeting the "user that will sign the report generated" requirement.

### C. Sheet 2 containing a Graph
- The exporter creates a second distinct worksheet named **"Chart"**.
- It leverages a specialized summary query (e.g., grouping payments by month) via `ReportServices.cs`.
- Depending on the report type, it generates an `ExcelChart` (such as a `ClusteredColumn` for Jobs/Clients or `LineMarkers` for Payments) directly inside Sheet 2 to visually represent the data.

---

## Guide: Recording the Video of Your IS
Since CampusRaket System is a Windows Desktop Application (WinForms), you cannot record it using browser-based extensions. To record a video demonstrating your system's features as required, follow these steps:

**Using built-in Windows Tools:**
1. **Snipping Tool (Windows 11):** Open Snipping Tool, click the "Record" (video camera) icon, select your screen area, and hit Start.
2. **Xbox Game Bar (Windows 10/11):** Press `Win + G` on your keyboard, click the "Capture" widget, and press the Record button (or use shortcut `Win + Alt + R`).

**What to show in the video:**
1. **Login:** Show yourself logging in to demonstrate the User Management integration.
2. **Transactions:** Open the Jobs, Proposals, and Payments forms. Briefly add or edit a record to prove they are functional transactions.
3. **Reporting:** Open the Report Generator, select a report, and click "Preview" to show the Data Grid control.
4. **Export:** Click the "Export Excel" button, save the file, and then **open the generated Excel file** on camera to prove it contains the Header/Logo, the Signature block, and the Graph on Sheet 2.
