-- ==========================================
-- Database Update Script (Demo Feedback)
-- ==========================================

USE [EMS_Database];
GO

-- 1. Drop Leave Management functionality
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Leaves]') AND type in (N'U'))
BEGIN
    TRUNCATE TABLE [dbo].[EMS_Leaves];
    DROP TABLE [dbo].[EMS_Leaves];
END;
GO

-- 2. Drop Department functionality
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Departments]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[EMS_Departments];
END;
GO

-- 3. Remove DepartmentName from Employees
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Employees]') AND name = N'DepartmentName')
BEGIN
    ALTER TABLE [dbo].[EMS_Employees] DROP COLUMN [DepartmentName];
END;
GO
