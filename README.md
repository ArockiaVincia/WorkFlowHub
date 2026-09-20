# WorkFlowHub

A full-stack workflow and project management application built with ASP.NET Core, C#, Dapper, and SQL Server.

## Features

- **Project & Task Management**: Create and track projects, assign team members, and manage tasks across workflow stages.
- **Interactive Kanban Board**: Visual task board with drag-and-drop status transitions from To Do through to Done.
- **Role-Based Access Control (RBAC)**: Support for Manager, Team Lead, Developer, and Quality Analyst roles with page and action-level permissions.
- **Decoupled Architecture**: 
  - `EmployeeWorkFlowHub.Web`: MVC frontend (Bootstrap 5, jQuery) running on port 5000.
  - `EmployeeWorkFlowHub.Api`: RESTful backend Web API running on port 5001 with JWT authentication.
- **Data Access**: High-performance data operations using Dapper and SQL Server Stored Procedures.

## Tech Stack

- **Backend**: .NET 10 (C#), ASP.NET Core Web API, JWT Bearer Authentication
- **Data Access**: Dapper, Microsoft.Data.SqlClient, Microsoft SQL Server
- **Frontend**: ASP.NET Core MVC (Razor), Bootstrap 5, jQuery
- **Architecture**: 5-layer separation (Controller -> Service -> Repository -> Stored Procedures -> Database)

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (Local or Express instance)
- Visual Studio 2022 or VS Code

### Database Setup

1. Open SQL Server Management Studio (SSMS) or command line.
2. Execute the database script against your local SQL Server instance to create the database schema, stored procedures, and seed initial lookup data.

### Configuration

1. In `EmployeeWorkFlowHub.Api/appsettings.json`, set your connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=EmployeeWorkFlowDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

2. In `EmployeeWorkFlowHub.Web/appsettings.json`, set your API Base URL:
```json
"ApiSettings": {
  "BaseUrl": "http://localhost:5001"
}
```

### Running the Application

1. Open the solution file `EmployeeWorkFlowHub.sln` in Visual Studio.
2. Set both `EmployeeWorkFlowHub.Api` and `EmployeeWorkFlowHub.Web` to start simultaneously (or run from CLI):

```powershell
# Terminal 1 - Web API (Port 5001)
dotnet run --project EmployeeWorkFlowHub.Api

# Terminal 2 - Web MVC (Port 5000)
dotnet run --project EmployeeWorkFlowHub.Web
```

3. Navigate to `http://localhost:5000` in your browser.

## Modules

- **Dashboard**: High-level metrics for departments, active projects, employees, and tasks.
- **Departments**: Manage organization departments and view team allocations.
- **Employees**: Employee directory with unique employee codes (`EMP-1001` series) and role assignments.
- **Projects**: Project timelines, assigned Project Managers, and team member management.
- **Tasks & Kanban**: Task board with 8 workflow states including development, review, and QA verification.
