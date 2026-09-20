-- =======================================================================
-- Project: Employee Workflow Hub
-- Target: Microsoft SQL Server
-- Complete Database Script with Tables, Stored Procedures, and Seed Data
-- =======================================================================

-- 1. Drop existing database cleanly and recreate fresh
USE [master];
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'EmployeeWorkFlowDb')
BEGIN
    ALTER DATABASE [EmployeeWorkFlowDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [EmployeeWorkFlowDb];
END
GO

CREATE DATABASE [EmployeeWorkFlowDb];
GO

USE [EmployeeWorkFlowDb];
GO

-- 2. Create Departments Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Departments')
BEGIN
    CREATE TABLE [dbo].[Departments] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL UNIQUE
    );
END
GO

-- 3. Create Projects Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Projects')
BEGIN
    CREATE TABLE [dbo].[Projects] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(150) NOT NULL UNIQUE,
        [DepartmentId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Departments]([Id]),
        [ProjectManagerId] INT NULL,
        [TeamMembers] NVARCHAR(500) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'In Progress'
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Projects') AND name = 'ProjectManagerId')
BEGIN
    ALTER TABLE [dbo].[Projects] ADD [ProjectManagerId] INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Projects') AND name = 'TeamMembers')
BEGIN
    ALTER TABLE [dbo].[Projects] ADD [TeamMembers] NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Projects') AND name = 'StartDate')
BEGIN
    ALTER TABLE [dbo].[Projects] ADD [StartDate] DATETIME NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Projects') AND name = 'EndDate')
BEGIN
    ALTER TABLE [dbo].[Projects] ADD [EndDate] DATETIME NULL;
END
GO

-- 4. Create Employees Table (Unified Table for All Employees & Users)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Employees')
BEGIN
    CREATE TABLE [dbo].[Employees] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EmployeeCode] NVARCHAR(50) NOT NULL UNIQUE,
        [Username] NVARCHAR(50) NULL,
        [PasswordHash] NVARCHAR(256) NOT NULL CONSTRAINT DF_Employees_PasswordHash DEFAULT 'E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEC5CE84C5712534C',
        [Role] NVARCHAR(50) NOT NULL CONSTRAINT DF_Employees_Role DEFAULT 'Developer / Team Member',
        [FullName] NVARCHAR(150) NOT NULL,
        [Email] NVARCHAR(150) NOT NULL UNIQUE,
        [DepartmentId] INT NULL FOREIGN KEY REFERENCES [dbo].[Departments]([Id]),
        [Designation] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 5. Create Tasks Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Tasks')
BEGIN
    CREATE TABLE [dbo].[Tasks] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [ProjectId] INT NULL FOREIGN KEY REFERENCES [dbo].[Projects]([Id]),
        [EmployeeId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Employees]([Id]),
        [Priority] NVARCHAR(50) NOT NULL DEFAULT 'Medium',
        [DueDate] DATETIME NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'To Do'
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tasks') AND name = 'ProjectId')
BEGIN
    ALTER TABLE [dbo].[Tasks] ADD [ProjectId] INT NULL FOREIGN KEY REFERENCES [dbo].[Projects]([Id]);
END
GO

-- =======================================================================
-- 6. STORED PROCEDURE: sp_User_Authenticate
-- =======================================================================
IF OBJECT_ID('dbo.sp_User_Authenticate', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_User_Authenticate;
GO

CREATE PROCEDURE [dbo].[sp_User_Authenticate]
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        e.[Id],
        COALESCE(e.[Username], e.[EmployeeCode]) AS [Username],
        e.[PasswordHash],
        e.[Role],
        e.[FullName],
        e.[IsActive],
        e.[Id] AS [EmployeeId]
    FROM [dbo].[Employees] e
    WHERE LOWER(TRIM(e.[Username])) = LOWER(TRIM(@Username))
       OR LOWER(TRIM(e.[FullName])) = LOWER(TRIM(@Username))
       OR LOWER(REPLACE(TRIM(e.[FullName]), ' ', '')) = LOWER(REPLACE(TRIM(@Username), ' ', ''))
       OR LOWER(TRIM(e.[EmployeeCode])) = LOWER(TRIM(@Username))
       OR LOWER(TRIM(e.[Email])) = LOWER(TRIM(@Username));
END
GO

-- =======================================================================
-- 8. STORED PROCEDURE: sp_Department_CRUD
-- =======================================================================
IF OBJECT_ID('dbo.sp_Department_CRUD', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Department_CRUD;
GO

CREATE PROCEDURE [dbo].[sp_Department_CRUD]
    @ActionId INT = 1,
    @Id INT = NULL,
    @Name NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- 1: SELECT_ALL
    IF @ActionId = 1
    BEGIN
        SELECT 
            d.[Id], 
            d.[Name],
            COUNT(DISTINCT e.[Id]) AS [EmployeeCount],
            COUNT(DISTINCT p.[Id]) AS [ProjectCount]
        FROM [dbo].[Departments] d
        LEFT JOIN [dbo].[Employees] e ON d.[Id] = e.[DepartmentId]
        LEFT JOIN [dbo].[Projects] p ON d.[Id] = p.[DepartmentId]
        GROUP BY d.[Id], d.[Name]
        ORDER BY d.[Id] ASC;
    END

    -- 2: SELECT_BY_ID
    ELSE IF @ActionId = 2
    BEGIN
        SELECT 
            d.[Id], 
            d.[Name],
            COUNT(DISTINCT e.[Id]) AS [EmployeeCount],
            COUNT(DISTINCT p.[Id]) AS [ProjectCount]
        FROM [dbo].[Departments] d
        LEFT JOIN [dbo].[Employees] e ON d.[Id] = e.[DepartmentId]
        LEFT JOIN [dbo].[Projects] p ON d.[Id] = p.[DepartmentId]
        WHERE d.[Id] = @Id
        GROUP BY d.[Id], d.[Name];
    END

    -- 3: INSERT
    ELSE IF @ActionId = 3
    BEGIN
        IF EXISTS (SELECT 1 FROM [dbo].[Departments] WHERE LOWER(TRIM([Name])) = LOWER(TRIM(@Name)))
        BEGIN
            RAISERROR('A department with the same name already exists.', 16, 1);
            RETURN;
        END

        INSERT INTO [dbo].[Departments] ([Name])
        VALUES (TRIM(@Name));

        SELECT SCOPE_IDENTITY() AS [NewId];
    END

    -- 4: UPDATE
    ELSE IF @ActionId = 4
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Departments] WHERE [Id] = @Id)
        BEGIN
            RAISERROR('Department not found.', 16, 1);
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM [dbo].[Departments] WHERE LOWER(TRIM([Name])) = LOWER(TRIM(@Name)) AND [Id] <> @Id)
        BEGIN
            RAISERROR('Another department with this name already exists.', 16, 1);
            RETURN;
        END

        UPDATE [dbo].[Departments]
        SET [Name] = TRIM(@Name)
        WHERE [Id] = @Id;

        SELECT @@ROWCOUNT AS [RowsAffected];
    END

    -- 5: DELETE
    ELSE IF @ActionId = 5
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Departments] WHERE [Id] = @Id)
        BEGIN
            RAISERROR('Department not found.', 16, 1);
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [DepartmentId] = @Id)
        BEGIN
            RAISERROR('Cannot delete department because employees are currently assigned to it.', 16, 1);
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM [dbo].[Projects] WHERE [DepartmentId] = @Id)
        BEGIN
            RAISERROR('Cannot delete department because projects are currently assigned to it.', 16, 1);
            RETURN;
        END

        DELETE FROM [dbo].[Departments]
        WHERE [Id] = @Id;

        SELECT @@ROWCOUNT AS [RowsAffected];
    END
END
GO

-- =======================================================================
-- 7. STORED PROCEDURE: sp_Employee_CRUD
-- =======================================================================
IF OBJECT_ID('dbo.sp_Employee_CRUD', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Employee_CRUD;
GO

CREATE PROCEDURE [dbo].[sp_Employee_CRUD]
    @ActionId INT = 1,
    @Id INT = NULL,
    @EmployeeCode NVARCHAR(50) = NULL,
    @FullName NVARCHAR(150) = NULL,
    @Email NVARCHAR(150) = NULL,
    @DepartmentId INT = NULL,
    @Designation NVARCHAR(100) = NULL,
    @IsActive BIT = 1,
    @Role NVARCHAR(50) = NULL,
    @Username NVARCHAR(50) = NULL,
    @PasswordHash NVARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- 1: SELECT_ALL
    IF @ActionId = 1
    BEGIN
        SELECT 
            e.[Id],
            e.[EmployeeCode],
            e.[FullName],
            e.[Email],
            e.[DepartmentId],
            d.[Name] AS [DepartmentName],
            e.[Designation],
            e.[IsActive],
            e.[Role],
            COALESCE(e.[Username], e.[EmployeeCode]) AS [Username]
        FROM [dbo].[Employees] e
        LEFT JOIN [dbo].[Departments] d ON e.[DepartmentId] = d.[Id]
        ORDER BY e.[Id] ASC;
    END

    -- 2: SELECT_BY_ID
    ELSE IF @ActionId = 2
    BEGIN
        SELECT 
            e.[Id],
            e.[EmployeeCode],
            e.[FullName],
            e.[Email],
            e.[DepartmentId],
            d.[Name] AS [DepartmentName],
            e.[Designation],
            e.[IsActive],
            e.[Role],
            COALESCE(e.[Username], e.[EmployeeCode]) AS [Username]
        FROM [dbo].[Employees] e
        LEFT JOIN [dbo].[Departments] d ON e.[DepartmentId] = d.[Id]
        WHERE e.[Id] = @Id;
    END

    -- 3: INSERT
    ELSE IF @ActionId = 3
    BEGIN
        -- Validate duplicate EmployeeCode
        IF EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE LOWER(TRIM([EmployeeCode])) = LOWER(TRIM(@EmployeeCode)))
        BEGIN
            RAISERROR('Employee Code already exists. Please enter a unique Employee Code.', 16, 1);
            RETURN;
        END

        -- Validate duplicate Email
        IF EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE LOWER(TRIM([Email])) = LOWER(TRIM(@Email)))
        BEGIN
            RAISERROR('Email address already registered. Please enter a unique Email.', 16, 1);
            RETURN;
        END

        -- Validate Department exists if provided
        IF @DepartmentId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Departments] WHERE [Id] = @DepartmentId)
        BEGIN
            RAISERROR('Selected Department does not exist.', 16, 1);
            RETURN;
        END

        SET @Username = COALESCE(NULLIF(TRIM(@Username), ''), TRIM(@EmployeeCode));
        SET @PasswordHash = COALESCE(@PasswordHash, 'E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEC5CE84C5712534C');
        SET @Role = COALESCE(NULLIF(TRIM(@Role), ''), 'Developer / Team Member');

        INSERT INTO [dbo].[Employees] (
            [EmployeeCode], [Username], [PasswordHash], [Role], [FullName], [Email], [DepartmentId], [Designation], [IsActive]
        )
        VALUES (
            TRIM(@EmployeeCode), @Username, @PasswordHash, @Role, TRIM(@FullName), LOWER(TRIM(@Email)), @DepartmentId, TRIM(@Designation), @IsActive
        );

        SELECT SCOPE_IDENTITY() AS [NewId];
    END

    -- 4: UPDATE
    ELSE IF @ActionId = 4
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [Id] = @Id)
        BEGIN
            RAISERROR('Employee record not found.', 16, 1);
            RETURN;
        END

        -- Check duplicate EmployeeCode on another record
        IF EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE LOWER(TRIM([EmployeeCode])) = LOWER(TRIM(@EmployeeCode)) AND [Id] <> @Id)
        BEGIN
            RAISERROR('Another employee already has this Employee Code.', 16, 1);
            RETURN;
        END

        -- Check duplicate Email on another record
        IF EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE LOWER(TRIM([Email])) = LOWER(TRIM(@Email)) AND [Id] <> @Id)
        BEGIN
            RAISERROR('Another employee already has this Email address.', 16, 1);
            RETURN;
        END

        IF @DepartmentId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Departments] WHERE [Id] = @DepartmentId)
        BEGIN
            RAISERROR('Selected Department does not exist.', 16, 1);
            RETURN;
        END

        UPDATE [dbo].[Employees]
        SET [EmployeeCode] = TRIM(@EmployeeCode),
            [FullName] = TRIM(@FullName),
            [Email] = LOWER(TRIM(@Email)),
            [DepartmentId] = @DepartmentId,
            [Designation] = TRIM(@Designation),
            [Role] = COALESCE(NULLIF(TRIM(@Role), ''), [Role]),
            [Username] = COALESCE(NULLIF(TRIM(@Username), ''), [Username]),
            [PasswordHash] = CASE WHEN @PasswordHash IS NOT NULL AND LEN(@PasswordHash) > 0 THEN @PasswordHash ELSE [PasswordHash] END,
            [IsActive] = @IsActive
        WHERE [Id] = @Id;

        SELECT @@ROWCOUNT AS [RowsAffected];
    END

    -- 5: DELETE
    ELSE IF @ActionId = 5
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [Id] = @Id)
        BEGIN
            RAISERROR('Employee record not found.', 16, 1);
            RETURN;
        END

        -- Check if any task is assigned to this employee
        IF EXISTS (SELECT 1 FROM [dbo].[Tasks] WHERE [EmployeeId] = @Id)
        BEGIN
            RAISERROR('Cannot delete employee because active tasks are assigned to them.', 16, 1);
            RETURN;
        END

        DELETE FROM [dbo].[Employees]
        WHERE [Id] = @Id;

        SELECT @@ROWCOUNT AS [RowsAffected];
    END
END
GO

-- =======================================================================
-- 8. STORED PROCEDURE: sp_Project_CRUD
-- =======================================================================
IF OBJECT_ID('dbo.sp_Project_CRUD', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Project_CRUD;
GO

CREATE PROCEDURE [dbo].[sp_Project_CRUD]
    @ActionId INT = 1,
    @Id INT = NULL,
    @Name NVARCHAR(150) = NULL,
    @DepartmentId INT = NULL,
    @ProjectManagerId INT = NULL,
    @TeamMembers NVARCHAR(500) = NULL,
    @Status NVARCHAR(50) = 'In Progress',
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- 1: SELECT_ALL
    IF @ActionId = 1
    BEGIN
        SELECT 
            p.[Id],
            p.[Name],
            p.[DepartmentId],
            d.[Name] AS [DepartmentName],
            p.[ProjectManagerId],
            pm.[FullName] AS [ProjectManagerName],
            p.[TeamMembers],
            p.[Status],
            p.[StartDate],
            p.[EndDate]
        FROM [dbo].[Projects] p
        INNER JOIN [dbo].[Departments] d ON p.[DepartmentId] = d.[Id]
        LEFT JOIN [dbo].[Employees] pm ON p.[ProjectManagerId] = pm.[Id]
        ORDER BY p.[Id] ASC;
    END

    -- 2: SELECT_BY_ID
    ELSE IF @ActionId = 2
    BEGIN
        SELECT 
            p.[Id],
            p.[Name],
            p.[DepartmentId],
            d.[Name] AS [DepartmentName],
            p.[ProjectManagerId],
            pm.[FullName] AS [ProjectManagerName],
            p.[TeamMembers],
            p.[Status],
            p.[StartDate],
            p.[EndDate]
        FROM [dbo].[Projects] p
        INNER JOIN [dbo].[Departments] d ON p.[DepartmentId] = d.[Id]
        LEFT JOIN [dbo].[Employees] pm ON p.[ProjectManagerId] = pm.[Id]
        WHERE p.[Id] = @Id;
    END

    -- 3: SELECT_BY_MANAGER (Team Lead filtered projects)
    ELSE IF @ActionId = 3
    BEGIN
        SELECT 
            p.[Id],
            p.[Name],
            p.[DepartmentId],
            d.[Name] AS [DepartmentName],
            p.[ProjectManagerId],
            pm.[FullName] AS [ProjectManagerName],
            p.[TeamMembers],
            p.[Status],
            p.[StartDate],
            p.[EndDate]
        FROM [dbo].[Projects] p
        INNER JOIN [dbo].[Departments] d ON p.[DepartmentId] = d.[Id]
        LEFT JOIN [dbo].[Employees] pm ON p.[ProjectManagerId] = pm.[Id]
        WHERE p.[ProjectManagerId] = @ProjectManagerId
        ORDER BY p.[Id] ASC;
    END

    -- 4: INSERT
    ELSE IF @ActionId = 4
    BEGIN
        IF EXISTS (SELECT 1 FROM [dbo].[Projects] WHERE LOWER(TRIM([Name])) = LOWER(TRIM(@Name)))
        BEGIN
            RAISERROR('A project with this name already exists.', 16, 1);
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [dbo].[Departments] WHERE [Id] = @DepartmentId)
        BEGIN
            RAISERROR('Selected Department does not exist.', 16, 1);
            RETURN;
        END

        IF @ProjectManagerId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [Id] = @ProjectManagerId)
        BEGIN
            RAISERROR('Selected Project Manager does not exist.', 16, 1);
            RETURN;
        END

        INSERT INTO [dbo].[Projects] ([Name], [DepartmentId], [ProjectManagerId], [TeamMembers], [Status], [StartDate], [EndDate])
        VALUES (TRIM(@Name), @DepartmentId, @ProjectManagerId, TRIM(@TeamMembers), TRIM(@Status), @StartDate, @EndDate);

        SELECT SCOPE_IDENTITY() AS [NewId];
    END

    -- 5: UPDATE
    ELSE IF @ActionId = 5
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Projects] WHERE [Id] = @Id)
        BEGIN
            RAISERROR('Project not found.', 16, 1);
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM [dbo].[Projects] WHERE LOWER(TRIM([Name])) = LOWER(TRIM(@Name)) AND [Id] <> @Id)
        BEGIN
            RAISERROR('Another project with this name already exists.', 16, 1);
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [dbo].[Departments] WHERE [Id] = @DepartmentId)
        BEGIN
            RAISERROR('Selected Department does not exist.', 16, 1);
            RETURN;
        END

        IF @ProjectManagerId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [Id] = @ProjectManagerId)
        BEGIN
            RAISERROR('Selected Project Manager does not exist.', 16, 1);
            RETURN;
        END

        UPDATE [dbo].[Projects]
        SET [Name] = TRIM(@Name),
            [DepartmentId] = @DepartmentId,
            [ProjectManagerId] = @ProjectManagerId,
            [TeamMembers] = TRIM(@TeamMembers),
            [Status] = TRIM(@Status),
            [StartDate] = @StartDate,
            [EndDate] = @EndDate
        WHERE [Id] = @Id;

        SELECT @@ROWCOUNT AS [RowsAffected];
    END

    -- 6: DELETE
    ELSE IF @ActionId = 6
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Projects] WHERE [Id] = @Id)
        BEGIN
            RAISERROR('Project not found.', 16, 1);
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM [dbo].[Tasks] WHERE [ProjectId] = @Id)
        BEGIN
            RAISERROR('Cannot delete project because tasks are assigned to it.', 16, 1);
            RETURN;
        END

        DELETE FROM [dbo].[Projects]
        WHERE [Id] = @Id;

        SELECT @@ROWCOUNT AS [RowsAffected];
    END
END
GO

-- =======================================================================
-- 9. STORED PROCEDURE: sp_Task_CRUD
-- =======================================================================
IF OBJECT_ID('dbo.sp_Task_CRUD', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Task_CRUD;
GO

CREATE PROCEDURE [dbo].[sp_Task_CRUD]
    @ActionId INT = 1,
    @Id INT = NULL,
    @Title NVARCHAR(200) = NULL,
    @Description NVARCHAR(1000) = NULL,
    @ProjectId INT = NULL,
    @EmployeeId INT = NULL,
    @Priority NVARCHAR(50) = 'Medium',
    @DueDate DATETIME = NULL,
    @Status NVARCHAR(50) = 'To Do'
AS
BEGIN
    SET NOCOUNT ON;

    -- 1: SELECT_ALL
    IF @ActionId = 1
    BEGIN
        SELECT 
            t.[Id],
            t.[Title],
            t.[Description],
            t.[ProjectId],
            p.[Name] AS [ProjectName],
            t.[EmployeeId],
            e.[FullName] AS [EmployeeName],
            t.[Priority],
            t.[DueDate],
            t.[Status]
        FROM [dbo].[Tasks] t
        INNER JOIN [dbo].[Employees] e ON t.[EmployeeId] = e.[Id]
        LEFT JOIN [dbo].[Projects] p ON t.[ProjectId] = p.[Id]
        ORDER BY t.[Id] ASC;
    END

    -- 2: SELECT_BY_ID
    ELSE IF @ActionId = 2
    BEGIN
        SELECT 
            t.[Id],
            t.[Title],
            t.[Description],
            t.[ProjectId],
            p.[Name] AS [ProjectName],
            t.[EmployeeId],
            e.[FullName] AS [EmployeeName],
            t.[Priority],
            t.[DueDate],
            t.[Status]
        FROM [dbo].[Tasks] t
        INNER JOIN [dbo].[Employees] e ON t.[EmployeeId] = e.[Id]
        LEFT JOIN [dbo].[Projects] p ON t.[ProjectId] = p.[Id]
        WHERE t.[Id] = @Id;
    END

    -- 3: SELECT_BY_PROJECT_LEAD (Team Lead sees tasks belonging to their projects)
    ELSE IF @ActionId = 3
    BEGIN
        SELECT 
            t.[Id],
            t.[Title],
            t.[Description],
            t.[ProjectId],
            p.[Name] AS [ProjectName],
            t.[EmployeeId],
            e.[FullName] AS [EmployeeName],
            t.[Priority],
            t.[DueDate],
            t.[Status]
        FROM [dbo].[Tasks] t
        INNER JOIN [dbo].[Employees] e ON t.[EmployeeId] = e.[Id]
        INNER JOIN [dbo].[Projects] p ON t.[ProjectId] = p.[Id]
        WHERE p.[ProjectManagerId] = @EmployeeId
        ORDER BY t.[Id] ASC;
    END

    -- 4: SELECT_BY_EMPLOYEE (Team Member sees only their assigned tasks)
    ELSE IF @ActionId = 4
    BEGIN
        SELECT 
            t.[Id],
            t.[Title],
            t.[Description],
            t.[ProjectId],
            p.[Name] AS [ProjectName],
            t.[EmployeeId],
            e.[FullName] AS [EmployeeName],
            t.[Priority],
            t.[DueDate],
            t.[Status]
        FROM [dbo].[Tasks] t
        INNER JOIN [dbo].[Employees] e ON t.[EmployeeId] = e.[Id]
        LEFT JOIN [dbo].[Projects] p ON t.[ProjectId] = p.[Id]
        WHERE t.[EmployeeId] = @EmployeeId
        ORDER BY t.[Id] ASC;
    END

    -- 5: SELECT_FOR_QC (QC role sees tasks that reached Ready For QC and subsequent verification stages)
    ELSE IF @ActionId = 5
    BEGIN
        SELECT 
            t.[Id],
            t.[Title],
            t.[Description],
            t.[ProjectId],
            p.[Name] AS [ProjectName],
            t.[EmployeeId],
            e.[FullName] AS [EmployeeName],
            t.[Priority],
            t.[DueDate],
            t.[Status]
        FROM [dbo].[Tasks] t
        INNER JOIN [dbo].[Employees] e ON t.[EmployeeId] = e.[Id]
        LEFT JOIN [dbo].[Projects] p ON t.[ProjectId] = p.[Id]
        WHERE t.[Status] IN ('Ready For QC', 'Under QC / To Be Verify', 'QC Failed', 'Done')
        ORDER BY t.[Id] ASC;
    END

    -- 6: INSERT
    ELSE IF @ActionId = 6
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [Id] = @EmployeeId)
        BEGIN
            RAISERROR('Selected Employee does not exist.', 16, 1);
            RETURN;
        END

        IF @ProjectId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Projects] WHERE [Id] = @ProjectId)
        BEGIN
            RAISERROR('Selected Project does not exist.', 16, 1);
            RETURN;
        END

        INSERT INTO [dbo].[Tasks] ([Title], [Description], [ProjectId], [EmployeeId], [Priority], [DueDate], [Status])
        VALUES (TRIM(@Title), @Description, @ProjectId, @EmployeeId, TRIM(@Priority), @DueDate, TRIM(@Status));

        SELECT SCOPE_IDENTITY() AS [NewId];
    END

    -- 7: UPDATE
    ELSE IF @ActionId = 7
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Tasks] WHERE [Id] = @Id)
        BEGIN
            RAISERROR('Task record not found.', 16, 1);
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [dbo].[Employees] WHERE [Id] = @EmployeeId)
        BEGIN
            RAISERROR('Selected Employee does not exist.', 16, 1);
            RETURN;
        END

        IF @ProjectId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Projects] WHERE [Id] = @ProjectId)
        BEGIN
            RAISERROR('Selected Project does not exist.', 16, 1);
            RETURN;
        END

        UPDATE [dbo].[Tasks]
        SET [Title] = TRIM(@Title),
            [Description] = @Description,
            [ProjectId] = @ProjectId,
            [EmployeeId] = @EmployeeId,
            [Priority] = TRIM(@Priority),
            [DueDate] = @DueDate,
            [Status] = TRIM(@Status)
        WHERE [Id] = @Id;

        SELECT @@ROWCOUNT AS [RowsAffected];
    END

    -- 8: DELETE
    ELSE IF @ActionId = 8
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Tasks] WHERE [Id] = @Id)
        BEGIN
            RAISERROR('Task record not found.', 16, 1);
            RETURN;
        END

        DELETE FROM [dbo].[Tasks]
        WHERE [Id] = @Id;

        SELECT @@ROWCOUNT AS [RowsAffected];
    END
END
GO

-- =======================================================================
-- 10. Initial Seed Data
-- =======================================================================
-- Departments
IF NOT EXISTS (SELECT 1 FROM [dbo].[Departments])
BEGIN
    INSERT INTO [dbo].[Departments] ([Name]) VALUES 
    (N'Information Technology'),
    (N'Human Resources'),
    (N'Finance'),
    (N'Operations');
END
GO

-- 11. Initial Seed Data
-- =======================================================================

-- Seed Departments
IF NOT EXISTS (SELECT 1 FROM [dbo].[Departments])
BEGIN
    SET IDENTITY_INSERT [dbo].[Departments] ON;
    INSERT INTO [dbo].[Departments] ([Id], [Name]) VALUES
    (1, N'Engineering'),
    (2, N'Quality Assurance'),
    (3, N'Product Management'),
    (4, N'Human Resources'),
    (5, N'IT Operations');
    SET IDENTITY_INSERT [dbo].[Departments] OFF;
END
GO

-- Seed Employees (Unified table with credentials)
-- Standard Password for demo accounts: Password@123
-- SHA-256 Hash: FF7BD97B1A7789DDD2775122FD6817F3173672DA9F802CEEC57F284325BF589F
IF NOT EXISTS (SELECT 1 FROM [dbo].[Employees])
BEGIN
    DECLARE @DefaultHash NVARCHAR(256) = N'FF7BD97B1A7789DDD2775122FD6817F3173672DA9F802CEEC57F284325BF589F';

    SET IDENTITY_INSERT [dbo].[Employees] ON;
    INSERT INTO [dbo].[Employees] 
        ([Id], [EmployeeCode], [FullName], [Email], [DepartmentId], [Designation], [IsActive], [Role], [PasswordHash], [Username], [CreatedAt]) 
    VALUES
    (1, N'EMP-1001', N'Sarah Connor', N'sarah.connor@workflowhub.com', 1, N'Engineering Director', 1, N'Manager', @DefaultHash, N'manager', GETUTCDATE()),
    (2, N'EMP-1002', N'Alex Morgan', N'alex.morgan@workflowhub.com', 3, N'Senior Product Manager', 1, N'Manager', @DefaultHash, N'alex', GETUTCDATE()),
    (3, N'EMP-1003', N'John Doe', N'john.doe@workflowhub.com', 1, N'Lead Software Architect', 1, N'Team Lead / Project Lead', @DefaultHash, N'lead', GETUTCDATE()),
    (4, N'EMP-1004', N'Jane Smith', N'jane.smith@workflowhub.com', 1, N'Full Stack Team Lead', 1, N'Team Lead / Project Lead', @DefaultHash, N'jane', GETUTCDATE()),
    (5, N'EMP-1005', N'Bob Miller', N'bob.miller@workflowhub.com', 1, N'Senior Backend Engineer', 1, N'Developer / Team Member', @DefaultHash, N'member', GETUTCDATE()),
    (6, N'EMP-1006', N'Vincy Arockia', N'vincy@workflowhub.com', 1, N'Senior Software Developer', 1, N'Developer / Team Member', @DefaultHash, N'VIncy', GETUTCDATE()),
    (7, N'EMP-1007', N'David Chen', N'david.chen@workflowhub.com', 1, N'Frontend UI Engineer', 1, N'Developer / Team Member', @DefaultHash, N'david', GETUTCDATE()),
    (8, N'EMP-1008', N'Alice Cooper', N'alice.cooper@workflowhub.com', 2, N'Senior QA Automation Specialist', 1, N'Quality Analyst / QC', @DefaultHash, N'qc', GETUTCDATE()),
    (9, N'EMP-1009', N'System Administrator', N'admin@workflowhub.com', 5, N'Infrastructure & Security Administrator', 1, N'Manager', @DefaultHash, N'admin', GETUTCDATE());
    SET IDENTITY_INSERT [dbo].[Employees] OFF;
END
GO

-- Seed Projects (With Start Date, End Date, PM, and Team Members)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Projects])
BEGIN
    SET IDENTITY_INSERT [dbo].[Projects] ON;
    INSERT INTO [dbo].[Projects]
        ([Id], [Name], [DepartmentId], [ProjectManagerId], [TeamMembers], [Status], [StartDate], [EndDate])
    VALUES
    (1, N'Enterprise Workflow Hub', 1, 3, 
     N'John Doe, Bob Miller, Vincy Arockia, David Chen, Alice Cooper', N'In Progress', 
     '2026-08-01', '2026-12-31'),

    (2, N'Customer Self-Service Portal 2.0', 1, 4, 
     N'Jane Smith, Vincy Arockia, Bob Miller', N'Planning', 
     '2026-09-15', '2027-03-31'),

    (3, N'Automated Quality & Compliance Suite', 2, 1, 
     N'Alice Cooper, John Doe', N'In Progress', 
     '2026-07-01', '2026-11-30');
    SET IDENTITY_INSERT [dbo].[Projects] OFF;
END
GO

-- Seed Tasks Across 8-Stage Kanban Workflow
IF NOT EXISTS (SELECT 1 FROM [dbo].[Tasks])
BEGIN
    SET IDENTITY_INSERT [dbo].[Tasks] ON;
    INSERT INTO [dbo].[Tasks]
        ([Id], [Title], [Description], [ProjectId], [EmployeeId], [Priority], [DueDate], [Status])
    VALUES
    (1, N'System Architecture & API Design', 
     N'Design micro-service boundaries, DTO schemas, and OpenAPI v1 specifications.', 
     1, 3, N'High', '2026-08-20', N'Done'),

    (2, N'JWT Authentication & Claims-Based RBAC', 
     N'Implement secure HMAC-SHA256 JWT generation with role-based claim authorization.', 
     1, 6, N'Critical', '2026-09-10', N'Done'),

    (3, N'Single-Table Database Consolidation', 
     N'Merge Users table into Employees table, update stored procedures and Dapper repositories.', 
     1, 6, N'High', '2026-09-25', N'In Progress'),

    (4, N'8-Stage Interactive Kanban Board UI', 
     N'Build drag-and-drop workflow with HTML5 drag events, state validations, and dynamic badge counts.', 
     1, 5, N'High', '2026-09-22', N'Today Task'),

    (5, N'Automated E2E Regression Test Suite', 
     N'Develop PowerShell & xUnit test suite validating all API endpoints and role permission matrices.', 
     3, 8, N'Medium', '2026-09-28', N'Ready For QC'),

    (6, N'Security Audit & Vulnerability Assessment', 
     N'Perform OWASP Top 10 security scanning, SQL injection prevention audit, and token expiration checks.', 
     3, 8, N'High', '2026-10-05', N'Under QC / To Be Verify'),

    (7, N'Customer Portal UX Wireframes & Prototype', 
     N'Create high-fidelity Bootstrap 5 responsive UI prototypes with accessibility compliance.', 
     2, 7, N'Medium', '2026-10-15', N'To Do'),

    (8, N'Payment Gateway Webhook Architecture', 
     N'Design resilient idempotent webhook receiver for asynchronous payment processing.', 
     2, 4, N'High', '2026-10-10', N'Under Review');
    SET IDENTITY_INSERT [dbo].[Tasks] OFF;
END
GO

-- =======================================================================
-- 9. Create LookupMaster Table and Seed Data
-- =======================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'LookupMaster')
BEGIN
    CREATE TABLE [dbo].[LookupMaster] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [LookupType] NVARCHAR(50) NOT NULL,
        [LookupCode] NVARCHAR(50) NOT NULL,
        [LookupValue] NVARCHAR(100) NOT NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[LookupMaster] WHERE [LookupType] = 'TaskStatus')
BEGIN
    INSERT INTO [dbo].[LookupMaster] ([LookupType], [LookupCode], [LookupValue], [DisplayOrder], [IsActive]) VALUES
    ('TaskStatus', 'To Do', 'To Do', 1, 1),
    ('TaskStatus', 'Today Task', 'Today Task', 2, 1),
    ('TaskStatus', 'In Progress', 'In Progress', 3, 1),
    ('TaskStatus', 'Under Review', 'Under Review', 4, 1),
    ('TaskStatus', 'Ready For QC', 'Ready For QC', 5, 1),
    ('TaskStatus', 'Under QC / To Be Verify', 'Under QC / To Be Verify', 6, 1),
    ('TaskStatus', 'QC Failed', 'QC Failed', 7, 1),
    ('TaskStatus', 'Done', 'Done', 8, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[LookupMaster] WHERE [LookupType] = 'TaskPriority')
BEGIN
    INSERT INTO [dbo].[LookupMaster] ([LookupType], [LookupCode], [LookupValue], [DisplayOrder], [IsActive]) VALUES
    ('TaskPriority', 'High', 'High', 1, 1),
    ('TaskPriority', 'Medium', 'Medium', 2, 1),
    ('TaskPriority', 'Low', 'Low', 3, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[LookupMaster] WHERE [LookupType] = 'ProjectStatus')
BEGIN
    INSERT INTO [dbo].[LookupMaster] ([LookupType], [LookupCode], [LookupValue], [DisplayOrder], [IsActive]) VALUES
    ('ProjectStatus', 'Planning', 'Planning', 1, 1),
    ('ProjectStatus', 'In Progress', 'In Progress', 2, 1),
    ('ProjectStatus', 'Completed', 'Completed', 3, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[LookupMaster] WHERE [LookupType] = 'UserRole')
BEGIN
    INSERT INTO [dbo].[LookupMaster] ([LookupType], [LookupCode], [LookupValue], [DisplayOrder], [IsActive]) VALUES
    ('UserRole', 'Manager', 'Manager', 1, 1),
    ('UserRole', 'TeamLead', 'Team Lead / Project Lead', 2, 1),
    ('UserRole', 'Developer', 'Developer / Team Member', 3, 1),
    ('UserRole', 'QC', 'Quality Analyst / QC', 4, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[LookupMaster] WHERE [LookupType] = 'EmployeeStatus')
BEGIN
    INSERT INTO [dbo].[LookupMaster] ([LookupType], [LookupCode], [LookupValue], [DisplayOrder], [IsActive]) VALUES
    ('EmployeeStatus', 'Active', 'Active', 1, 1),
    ('EmployeeStatus', 'Inactive', 'Inactive', 2, 1);
END
GO

-- =======================================================================
-- 10. Stored Procedure: sp_Lookup_CRUD
-- =======================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Lookup_CRUD]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_Lookup_CRUD];
GO

CREATE PROCEDURE [dbo].[sp_Lookup_CRUD]
    @ActionId INT,
    @LookupType NVARCHAR(50) = NULL,
    @LookupCode NVARCHAR(50) = NULL,
    @LookupValue NVARCHAR(100) = NULL,
    @DisplayOrder INT = NULL,
    @IsActive BIT = NULL,
    @Id INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- 1: SELECT_ALL
    IF @ActionId = 1
    BEGIN
        SELECT [Id], [LookupType], [LookupCode], [LookupValue], [DisplayOrder], [IsActive]
        FROM [dbo].[LookupMaster]
        WHERE [IsActive] = 1
        ORDER BY [LookupType], [DisplayOrder];
    END

    -- 2: SELECT_BY_TYPE
    ELSE IF @ActionId = 2
    BEGIN
        SELECT [Id], [LookupType], [LookupCode], [LookupValue], [DisplayOrder], [IsActive]
        FROM [dbo].[LookupMaster]
        WHERE [LookupType] = @LookupType AND [IsActive] = 1
        ORDER BY [DisplayOrder];
    END

    -- 3: INSERT
    ELSE IF @ActionId = 3
    BEGIN
        INSERT INTO [dbo].[LookupMaster] ([LookupType], [LookupCode], [LookupValue], [DisplayOrder], [IsActive])
        VALUES (@LookupType, @LookupCode, @LookupValue, ISNULL(@DisplayOrder, 0), ISNULL(@IsActive, 1));
        SELECT SCOPE_IDENTITY() AS [Id];
    END

    -- 4: UPDATE
    ELSE IF @ActionId = 4
    BEGIN
        UPDATE [dbo].[LookupMaster]
        SET [LookupType] = ISNULL(@LookupType, [LookupType]),
            [LookupCode] = ISNULL(@LookupCode, [LookupCode]),
            [LookupValue] = ISNULL(@LookupValue, [LookupValue]),
            [DisplayOrder] = ISNULL(@DisplayOrder, [DisplayOrder]),
            [IsActive] = ISNULL(@IsActive, [IsActive])
        WHERE [Id] = @Id;
        SELECT @@ROWCOUNT AS [RowsAffected];
    END

    -- 5: DELETE
    ELSE IF @ActionId = 5
    BEGIN
        DELETE FROM [dbo].[LookupMaster]
        WHERE [Id] = @Id;
        SELECT @@ROWCOUNT AS [RowsAffected];
    END
END
GO



