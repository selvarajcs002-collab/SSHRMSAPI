-- =========================================================================================
-- DIAGNOSTIC QUERIES & STORED PROCEDURE: dbo.usp_Employee_Delete
-- Database: SSManagement_HRMS
-- Purpose: Permanently and atomically delete an employee and all dependent records.
-- =========================================================================================

-- =========================================================================================
-- DIAGNOSTIC QUERY 1: Inspect all tables having foreign keys referencing dbo.Employees
-- =========================================================================================
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_SCHEMA_NAME(fk.parent_object_id) AS ChildTableSchema,
    OBJECT_NAME(fk.parent_object_id) AS ChildTableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ChildColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ParentTableName,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ParentColumnName,
    fk.delete_referential_action_desc AS OnDeleteAction
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE fk.referenced_object_id = OBJECT_ID('dbo.Employees')
ORDER BY ChildTableName;

-- =========================================================================================
-- DIAGNOSTIC QUERY 2: Inspect all tables containing EmployeeId column in SSManagement_HRMS
-- =========================================================================================
SELECT 
    t.TABLE_SCHEMA AS SchemaName,
    t.TABLE_NAME AS TableName,
    c.COLUMN_NAME AS ColumnName,
    c.DATA_TYPE AS DataType,
    c.IS_NULLABLE AS IsNullable
FROM INFORMATION_SCHEMA.TABLES t
INNER JOIN INFORMATION_SCHEMA.COLUMNS c 
    ON t.TABLE_SCHEMA = c.TABLE_SCHEMA AND t.TABLE_NAME = c.TABLE_NAME
WHERE t.TABLE_TYPE = 'BASE TABLE'
  AND c.COLUMN_NAME = 'EmployeeId'
ORDER BY t.TABLE_NAME;
GO

-- =========================================================================================
-- STORED PROCEDURE: dbo.usp_Employee_Delete
-- Deletes child records in dependency order, then deletes parent dbo.Employees record.
-- Wrapped in an atomic SQL transaction with TRY/CATCH and THROW.
-- =========================================================================================
CREATE OR ALTER PROCEDURE dbo.usp_Employee_Delete
    @EmployeeId INT,
    @DeletedBy INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- 1. Validate @EmployeeId parameter
    IF @EmployeeId IS NULL OR @EmployeeId <= 0
    BEGIN
        SELECT 
            CAST(0 AS BIT) AS Success,
            'Invalid Employee ID.' AS Message,
            ISNULL(@EmployeeId, 0) AS EmployeeId,
            0 AS DeletedRows;
        RETURN;
    END

    -- 2. Verify whether employee exists
    IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE EmployeeId = @EmployeeId)
    BEGIN
        SELECT 
            CAST(0 AS BIT) AS Success,
            'Employee not found.' AS Message,
            @EmployeeId AS EmployeeId,
            0 AS DeletedRows;
        RETURN;
    END

    -- 3. Atomic transaction to delete dependent records then employee
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @EmployeeSalaryDetailsCount INT = 0;
        DECLARE @SalaryDetailsCount INT = 0;
        DECLARE @AttendanceCount INT = 0;
        DECLARE @EmployeeShiftsCount INT = 0;
        DECLARE @EmployeeDocumentsCount INT = 0;
        DECLARE @EmployeesCount INT = 0;
        DECLARE @TotalDeletedRows INT = 0;

        -- Step A: Delete from dbo.Employee_Salary_Details
        IF OBJECT_ID('dbo.Employee_Salary_Details', 'U') IS NOT NULL
        BEGIN
            DELETE FROM dbo.Employee_Salary_Details
            WHERE EmployeeId = @EmployeeId;
            SET @EmployeeSalaryDetailsCount = @@ROWCOUNT;
        END

        -- Step B: Delete from dbo.SalaryDetails
        IF OBJECT_ID('dbo.SalaryDetails', 'U') IS NOT NULL
        BEGIN
            DELETE FROM dbo.SalaryDetails
            WHERE EmployeeId = @EmployeeId;
            SET @SalaryDetailsCount = @@ROWCOUNT;
        END

        -- Step C: Delete from dbo.Attendance
        IF OBJECT_ID('dbo.Attendance', 'U') IS NOT NULL
        BEGIN
            DELETE FROM dbo.Attendance
            WHERE EmployeeId = @EmployeeId;
            SET @AttendanceCount = @@ROWCOUNT;
        END

        -- Step D: Delete from dbo.EmployeeShifts
        IF OBJECT_ID('dbo.EmployeeShifts', 'U') IS NOT NULL
        BEGIN
            DELETE FROM dbo.EmployeeShifts
            WHERE EmployeeId = @EmployeeId;
            SET @EmployeeShiftsCount = @@ROWCOUNT;
        END

        -- Step E: Delete from dbo.EmployeeDocuments
        IF OBJECT_ID('dbo.EmployeeDocuments', 'U') IS NOT NULL
        BEGIN
            DELETE FROM dbo.EmployeeDocuments
            WHERE EmployeeId = @EmployeeId;
            SET @EmployeeDocumentsCount = @@ROWCOUNT;
        END

        -- Step F: Finally delete the record from dbo.Employees
        DELETE FROM dbo.Employees
        WHERE EmployeeId = @EmployeeId;
        SET @EmployeesCount = @@ROWCOUNT;

        -- Step G: Audit log entry in HRMSActivityLog if present
        IF OBJECT_ID('dbo.HRMSActivityLog', 'U') IS NOT NULL
        BEGIN
            INSERT INTO dbo.HRMSActivityLog (UserId, Module, Action, EntityId, Description, IPAddress, CreatedDate)
            VALUES (@DeletedBy, 'Employee', 'DELETE', @EmployeeId, CONCAT('Employee ID ', @EmployeeId, ' permanently deleted along with all dependent records.'), NULL, GETDATE());
        END

        COMMIT TRANSACTION;

        SET @TotalDeletedRows = @EmployeeSalaryDetailsCount 
                              + @SalaryDetailsCount 
                              + @AttendanceCount 
                              + @EmployeeShiftsCount 
                              + @EmployeeDocumentsCount 
                              + @EmployeesCount;

        SELECT 
            CAST(1 AS BIT) AS Success,
            'Employee and all related data deleted successfully.' AS Message,
            @EmployeeId AS EmployeeId,
            @TotalDeletedRows AS DeletedRows;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        THROW;
    END CATCH
END
GO
