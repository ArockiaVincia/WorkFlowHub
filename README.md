# WorkFlowHub

WorkFlowHub is an enterprise role-based workflow and project management web application developed using **C#**, **ASP.NET Core MVC**, **RESTful Web API**, **Dapper**, and **Microsoft SQL Server**.

The solution demonstrates a clean decoupled architecture separating a lightweight presentation client (`EmployeeWorkFlowHub.Web`) from a backend API service (`EmployeeWorkFlowHub.Api`), secured with JWT Bearer authentication and role-based authorization.

---

## Architecture Overview

The solution is divided into two distinct projects:

1. **`EmployeeWorkFlowHub.Web` (Frontend MVC Client - Port 5000)**:
   - Built with ASP.NET Core MVC (Razor), Bootstrap 5, and jQuery AJAX.
   - Communicates with the backend exclusively through HTTP REST API endpoints using an authenticated fetch wrapper (`authFetch`).
   - Contains zero direct database references or connection strings.

2. **`EmployeeWorkFlowHub.Api` (Backend RESTful Web API - Port 5001)**:
   - Standalone ASP.NET Core Web API with JWT Bearer authentication and role-based access control.
   - Implements a strict 5-layer architecture:
     `Controller` ➔ `Service Layer` ➔ `Repository Layer` ➔ `Stored Procedures` ➔ `SQL Server Database`
   - High-performance data access using **Dapper** micro-ORM and parameterized Stored Procedures.

---

## Features

- **Role-Based Access Control (RBAC)**:
  - **Manager / Admin**: Complete CRUD permissions across all 4 modules (Departments, Employees, Projects, Tasks).
  - **Team Lead**: Access to Projects and Tasks pages; can create, edit, delete, and manage tasks for projects assigned to them.
  - **Developer**: Access to own assigned tasks only; can advance tasks across workflow stages (create/delete buttons hidden).
  - **Quality Analyst (QC)**: Dedicated verification pipeline from *Ready For QC* through *Done*.
- **Interactive 8-Stage Kanban Board**: Drag-and-drop task workflow with transition rules dynamically loaded from the database (`LookupMaster`).
- **Sequential Employee Codes**: Auto-generated sequential identifiers (`EMP-1001`, `EMP-1002`, etc.) handled by database stored procedures.
- **Flexible Multi-Identifier Login**: Authenticate using Username, Employee Code, Full Name, or Email.

---

## Technology Stack

- **Backend**: C#, ASP.NET Core Web API, JWT Bearer Tokens
- **Data Access**: Dapper, Microsoft.Data.SqlClient, Microsoft SQL Server
- **Frontend**: ASP.NET Core MVC, Razor, Bootstrap 5, jQuery
- **Security**: SHA-256 password hashing, claims-based authorization, CORS protection

---

## Prerequisites

- **.NET SDK** (.NET 10.0 or .NET 8.0+)
- **Microsoft SQL Server** (Local instance or SQL Server Express)
- **Visual Studio 2022** (v17.10+) or **Visual Studio Code**

---

## Setup & Installation

### 1. Database Setup

Open SQL Server Management Studio (SSMS) or use `sqlcmd` to execute the database script located in the project root:

```powershell
sqlcmd -S . -i DatabaseScript.sql
```

The script automatically:
- Creates the `EmployeeWorkFlowDb` database.
- Creates all required tables: `Departments`, `Employees`, `Projects`, `Tasks`, and `LookupMaster`.
- Creates all CRUD and authentication Stored Procedures.
- Seeds initial lookups, departments, verified employee accounts, projects, and tasks.

### 2. Configuration

Verify connection details in both projects:

* **`EmployeeWorkFlowHub.Api/appsettings.json`**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=EmployeeWorkFlowDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

* **`EmployeeWorkFlowHub.Web/appsettings.json`**:
```json
"ApiSettings": {
  "BaseUrl": "http://localhost:5001"
}
```

---

## Running the Application

### Option A: Using Visual Studio
1. Open `EmployeeWorkFlowHub.sln`.
2. Right-click the solution in Solution Explorer ➔ **Configure Startup Projects...**
3. Select **Multiple startup projects**:
   - `EmployeeWorkFlowHub.Api` ➔ **Start**
   - `EmployeeWorkFlowHub.Web` ➔ **Start**
4. Press **F5** (or **Ctrl + F5**) to launch both applications.

### Option B: Using .NET CLI

**Terminal 1 — Web API (Port 5001):**
```powershell
dotnet run --project EmployeeWorkFlowHub.Api
```

**Terminal 2 — Web MVC Client (Port 5000):**
```powershell
dotnet run --project EmployeeWorkFlowHub.Web
```

Open your browser at: **`http://localhost:5000`**

---

## Test Login Credentials

All test accounts share the same default password: **`Password@123`**

| Role | Username | Employee Code | Full Name | Access Level |
|---|---|---|---|---|
| **Manager** | `manager` | `EMP-1001` | Sarah Connor | Full CRUD on Departments, Employees, Projects, Tasks |
| **Team Lead** | `lead` | `EMP-1003` | John Doe | Project management & task allocation for led projects |
| **Developer** | `VIncy` | `EMP-1006` | Vincy Arockia | View assigned tasks & move stages on Kanban |
| **QA / QC** | `qc` | `EMP-1008` | Alice Cooper | QC stage verification & sign-off pipeline |
| **Admin** | `admin` | `EMP-1009` | System Administrator | Full administrative system access |

> Note: You can log in using either the **Username** (e.g. `manager`), **Employee Code** (e.g. `EMP-1001`), **Full Name**, or **Email**.
