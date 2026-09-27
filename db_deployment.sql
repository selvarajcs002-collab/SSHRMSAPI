-- ========================================================
-- DATABASE CREATION
-- ========================================================
IF NOT EXISTS (SELECT name FROM master.sys.databases WHERE name = N'SSManagement_HRMS')
BEGIN
    CREATE DATABASE [SSManagement_HRMS];
END
GO

USE [SSManagement_HRMS];
GO

-- ========================================================
-- TABLE TYPES (For Bulk Save)
-- ========================================================
IF TYPE_ID(N'dbo.EmployeeShiftAssignmentType') IS NULL
BEGIN
    CREATE TYPE dbo.EmployeeShiftAssignmentType AS TABLE(
        EmployeeId INT,
        Shift NVARCHAR(50)
    );
END
GO

IF TYPE_ID(N'dbo.AttendanceSaveType') IS NULL
BEGIN
    CREATE TYPE dbo.AttendanceSaveType AS TABLE(
        EmployeeId INT,
        AttendanceDate DATE,
        Shift NVARCHAR(50),
        Status NVARCHAR(50),
        Remarks NVARCHAR(250)
    );
END
GO

-- ========================================================
-- TABLES
-- ========================================================
-- Drop foreign key tables first to avoid constraint errors
IF OBJECT_ID('dbo.HRMSActivityLog', 'U') IS NOT NULL DROP TABLE dbo.HRMSActivityLog;
IF OBJECT_ID('dbo.SalaryDetails', 'U') IS NOT NULL DROP TABLE dbo.SalaryDetails;
IF OBJECT_ID('dbo.Attendance', 'U') IS NOT NULL DROP TABLE dbo.Attendance;
IF OBJECT_ID('dbo.EmployeeShifts', 'U') IS NOT NULL DROP TABLE dbo.EmployeeShifts;
IF OBJECT_ID('dbo.EmployeeDocuments', 'U') IS NOT NULL DROP TABLE dbo.EmployeeDocuments;
IF OBJECT_ID('dbo.Employees', 'U') IS NOT NULL DROP TABLE dbo.Employees;

IF OBJECT_ID('dbo.Employees', 'U') IS NULL
BEGIN

    CREATE TABLE dbo.Employees (
        EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeCode VARCHAR(50) NOT NULL UNIQUE,
        Name NVARCHAR(150) NOT NULL,
        Designation NVARCHAR(100) NOT NULL,
        City NVARCHAR(100),
        PerDaySalary DECIMAL(18,2) NOT NULL CHECK (PerDaySalary >= 0),
        JoinedYear INT NOT NULL CHECK (JoinedYear >= 1900 AND JoinedYear <= 2100),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy INT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME2 NULL
    );

    CREATE INDEX IX_Employees_EmployeeCode ON dbo.Employees(EmployeeCode);
    CREATE INDEX IX_Employees_Name ON dbo.Employees(Name);
    CREATE INDEX IX_Employees_City ON dbo.Employees(City);
    CREATE INDEX IX_Employees_Designation ON dbo.Employees(Designation);
    CREATE INDEX IX_Employees_IsActive ON dbo.Employees(IsActive);
END
GO

IF OBJECT_ID('dbo.EmployeeDocuments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeDocuments (
        DocumentId INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeId INT NOT NULL FOREIGN KEY REFERENCES dbo.Employees(EmployeeId),
        DocumentName NVARCHAR(255) NOT NULL,
        OriginalFileName NVARCHAR(255) NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        ContentType NVARCHAR(100),
        FileSize BIGINT,
        UploadedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UploadedBy INT NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
    CREATE INDEX IX_EmployeeDocuments_EmployeeId ON dbo.EmployeeDocuments(EmployeeId);
END
GO

IF OBJECT_ID('dbo.EmployeeShifts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeShifts (
        EmployeeShiftId INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeId INT NOT NULL FOREIGN KEY REFERENCES dbo.Employees(EmployeeId),
        Shift NVARCHAR(50) NOT NULL,
        EffectiveFrom DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
        EffectiveTo DATE NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy INT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME2 NULL
    );
    CREATE INDEX IX_EmployeeShifts_EmployeeId ON dbo.EmployeeShifts(EmployeeId);
    CREATE INDEX IX_EmployeeShifts_IsActive ON dbo.EmployeeShifts(IsActive);
END
GO

IF OBJECT_ID('dbo.Attendance', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Attendance (
        AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeId INT NOT NULL FOREIGN KEY REFERENCES dbo.Employees(EmployeeId),
        AttendanceDate DATE NOT NULL,
        Shift NVARCHAR(50) NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        Remarks NVARCHAR(250),
        CreatedBy INT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT UQ_Attendance_Employee_Date UNIQUE (EmployeeId, AttendanceDate)
    );
    CREATE INDEX IX_Attendance_EmployeeId ON dbo.Attendance(EmployeeId);
    CREATE INDEX IX_Attendance_Date ON dbo.Attendance(AttendanceDate);
END
GO

IF OBJECT_ID('dbo.SalaryDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalaryDetails (
        SalaryId INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeId INT NOT NULL FOREIGN KEY REFERENCES dbo.Employees(EmployeeId),
        SalaryMonth NVARCHAR(50) NOT NULL,
        FromDate DATE NOT NULL,
        ToDate DATE NOT NULL,
        PresentDays DECIMAL(10,2) NOT NULL DEFAULT 0,
        AbsentDays DECIMAL(10,2) NOT NULL DEFAULT 0,
        HalfDays DECIMAL(10,2) NOT NULL DEFAULT 0,
        PerDaySalary DECIMAL(18,2) NOT NULL DEFAULT 0,
        SalaryForPresentDays DECIMAL(18,2) NOT NULL DEFAULT 0,
        Incentives DECIMAL(18,2) NOT NULL DEFAULT 0,
        Advance DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
        IsPaid BIT NOT NULL DEFAULT 0,
        IsPrinted BIT NOT NULL DEFAULT 0,
        PaidDate DATETIME2 NULL,
        PrintedDate DATETIME2 NULL,
        CreatedBy INT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT UQ_SalaryDetails_Employee_Month UNIQUE (EmployeeId, SalaryMonth)
    );
    CREATE INDEX IX_SalaryDetails_EmployeeId ON dbo.SalaryDetails(EmployeeId);
    CREATE INDEX IX_SalaryDetails_SalaryMonth ON dbo.SalaryDetails(SalaryMonth);
END
GO

IF OBJECT_ID('dbo.HRMSActivityLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HRMSActivityLog (
        ActivityLogId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NULL,
        Module NVARCHAR(100) NOT NULL,
        Action NVARCHAR(100) NOT NULL,
        EntityId INT NULL,
        Description NVARCHAR(500),
        IPAddress NVARCHAR(50),
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
    );
    CREATE INDEX IX_ActivityLog_Module ON dbo.HRMSActivityLog(Module);
END
GO

-- ========================================================
-- STORED PROCEDURES (EMPLOYEE)
-- ========================================================
CREATE OR ALTER PROCEDURE dbo.usp_Employee_GetList
    @Search NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn VARCHAR(50) = 'Name',
    @SortDirection VARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    WITH FilteredEmployees AS (
        SELECT 
            e.EmployeeId,
            e.EmployeeCode,
            e.Name,
            e.Designation,
            e.City,
            e.PerDaySalary,
            e.JoinedYear,
            e.IsActive,
            (SELECT TOP 1 DocumentId FROM dbo.EmployeeDocuments ed WHERE ed.EmployeeId = e.EmployeeId AND ed.IsActive = 1 ORDER BY ed.UploadedDate DESC) AS DocumentId,
            (SELECT TOP 1 DocumentName FROM dbo.EmployeeDocuments ed WHERE ed.EmployeeId = e.EmployeeId AND ed.IsActive = 1 ORDER BY ed.UploadedDate DESC) AS DocumentName
        FROM dbo.Employees e
        WHERE e.IsActive = 1
        AND (@Search IS NULL OR e.Name LIKE '%' + @Search + '%' OR e.City LIKE '%' + @Search + '%' OR CAST(e.JoinedYear AS VARCHAR) LIKE '%' + @Search + '%')
    ),
    CountedEmployees AS (
        SELECT COUNT(1) AS TotalCount FROM FilteredEmployees
    )
    SELECT 
        f.*,
        c.TotalCount
    FROM FilteredEmployees f
    CROSS JOIN CountedEmployees c
    ORDER BY 
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Name' THEN f.Name END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Name' THEN f.Name END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Designation' THEN f.Designation END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Designation' THEN f.Designation END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'City' THEN f.City END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'City' THEN f.City END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'JoinedYear' THEN f.JoinedYear END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'JoinedYear' THEN f.JoinedYear END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'PerDaySalary' THEN f.PerDaySalary END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'PerDaySalary' THEN f.PerDaySalary END DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employee_GetById
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.Employees WHERE EmployeeId = @EmployeeId AND IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employee_Insert
    @EmployeeCode VARCHAR(50),
    @Name NVARCHAR(150),
    @Designation NVARCHAR(100),
    @City NVARCHAR(100),
    @PerDaySalary DECIMAL(18,2),
    @JoinedYear INT,
    @CreatedBy INT = NULL,
    @EmployeeId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Employees (EmployeeCode, Name, Designation, City, PerDaySalary, JoinedYear, IsActive, CreatedBy, CreatedDate)
    VALUES (@EmployeeCode, @Name, @Designation, @City, @PerDaySalary, @JoinedYear, 1, @CreatedBy, GETDATE());
    
    SET @EmployeeId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employee_Update
    @EmployeeId INT,
    @Name NVARCHAR(150),
    @Designation NVARCHAR(100),
    @City NVARCHAR(100),
    @PerDaySalary DECIMAL(18,2),
    @JoinedYear INT,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Employees 
    SET Name = @Name,
        Designation = @Designation,
        City = @City,
        PerDaySalary = @PerDaySalary,
        JoinedYear = @JoinedYear,
        UpdatedBy = @UpdatedBy,
        UpdatedDate = GETDATE()
    WHERE EmployeeId = @EmployeeId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employee_Delete
    @EmployeeId INT,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Employees SET IsActive = 0, UpdatedBy = @UpdatedBy, UpdatedDate = GETDATE() WHERE EmployeeId = @EmployeeId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employee_Duplicate
    @EmployeeId INT,
    @NewEmployeeCode VARCHAR(50),
    @CreatedBy INT = NULL,
    @NewEmployeeId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Employees (EmployeeCode, Name, Designation, City, PerDaySalary, JoinedYear, IsActive, CreatedBy, CreatedDate)
    SELECT @NewEmployeeCode, Name, Designation, City, PerDaySalary, JoinedYear, 1, @CreatedBy, GETDATE()
    FROM dbo.Employees WHERE EmployeeId = @EmployeeId;
    
    SET @NewEmployeeId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employee_SaveDocument
    @EmployeeId INT,
    @DocumentName NVARCHAR(255),
    @OriginalFileName NVARCHAR(255),
    @FilePath NVARCHAR(500),
    @ContentType NVARCHAR(100),
    @FileSize BIGINT,
    @UploadedBy INT = NULL,
    @DocumentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Deactivate old documents
    UPDATE dbo.EmployeeDocuments SET IsActive = 0 WHERE EmployeeId = @EmployeeId;
    
    INSERT INTO dbo.EmployeeDocuments (EmployeeId, DocumentName, OriginalFileName, FilePath, ContentType, FileSize, UploadedBy, UploadedDate, IsActive)
    VALUES (@EmployeeId, @DocumentName, @OriginalFileName, @FilePath, @ContentType, @FileSize, @UploadedBy, GETDATE(), 1);
    
    SET @DocumentId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Employee_GetDocument
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM dbo.EmployeeDocuments WHERE EmployeeId = @EmployeeId AND IsActive = 1 ORDER BY UploadedDate DESC;
END
GO

-- ========================================================
-- STORED PROCEDURES (SHIFT)
-- ========================================================
CREATE OR ALTER PROCEDURE dbo.usp_Shift_GetEmployees
    @Search NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn VARCHAR(50) = 'Name',
    @SortDirection VARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    WITH ShiftData AS (
        SELECT 
            e.EmployeeId,
            e.EmployeeCode,
            e.Name,
            e.Designation,
            es.Shift
        FROM dbo.Employees e
        LEFT JOIN dbo.EmployeeShifts es ON e.EmployeeId = es.EmployeeId AND es.IsActive = 1
        WHERE e.IsActive = 1
        AND (@Search IS NULL OR e.Name LIKE '%' + @Search + '%' OR e.EmployeeCode LIKE '%' + @Search + '%')
    ),
    CountedData AS (
        SELECT COUNT(1) AS TotalCount FROM ShiftData
    )
    SELECT 
        s.*,
        c.TotalCount
    FROM ShiftData s
    CROSS JOIN CountedData c
    ORDER BY 
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Name' THEN s.Name END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Name' THEN s.Name END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Designation' THEN s.Designation END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Designation' THEN s.Designation END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Shift' THEN s.Shift END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Shift' THEN s.Shift END DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Shift_Save
    @Assignments dbo.EmployeeShiftAssignmentType READONLY,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Deactivate existing shifts for these employees that differ from new ones
        UPDATE es
        SET es.IsActive = 0, es.EffectiveTo = CAST(GETDATE() AS DATE), es.UpdatedBy = @UpdatedBy, es.UpdatedDate = GETDATE()
        FROM dbo.EmployeeShifts es
        INNER JOIN @Assignments a ON es.EmployeeId = a.EmployeeId
        WHERE es.IsActive = 1 AND es.Shift != a.Shift;
        
        -- Insert new active shifts if they don't have an active one matching the requested shift
        INSERT INTO dbo.EmployeeShifts (EmployeeId, Shift, EffectiveFrom, IsActive, CreatedBy, CreatedDate)
        SELECT a.EmployeeId, a.Shift, CAST(GETDATE() AS DATE), 1, @UpdatedBy, GETDATE()
        FROM @Assignments a
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.EmployeeShifts es 
            WHERE es.EmployeeId = a.EmployeeId AND es.IsActive = 1 AND es.Shift = a.Shift
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Shift_GetSummary
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ISNULL(es.Shift, 'Unassigned') AS ShiftName,
        COUNT(e.EmployeeId) AS EmployeeCount
    FROM dbo.Employees e
    LEFT JOIN dbo.EmployeeShifts es ON e.EmployeeId = es.EmployeeId AND es.IsActive = 1
    WHERE e.IsActive = 1
    GROUP BY ISNULL(es.Shift, 'Unassigned');
END
GO

-- ========================================================
-- STORED PROCEDURES (ATTENDANCE)
-- ========================================================
CREATE OR ALTER PROCEDURE dbo.usp_Attendance_Get
    @AttendanceDate DATE,
    @Shift NVARCHAR(50),
    @Search NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn VARCHAR(50) = 'Name',
    @SortDirection VARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    WITH EmployeeBase AS (
        SELECT 
            e.EmployeeId,
            e.Name,
            e.Designation,
            es.Shift
        FROM dbo.Employees e
        LEFT JOIN dbo.EmployeeShifts es ON e.EmployeeId = es.EmployeeId AND es.IsActive = 1
        WHERE e.IsActive = 1
    ),
    FilteredByShift AS (
        SELECT eb.* FROM EmployeeBase eb WHERE eb.Shift = @Shift
        AND (@Search IS NULL OR eb.Name LIKE '%' + @Search + '%')
    ),
    AttendanceData AS (
        SELECT 
            f.EmployeeId,
            f.Name,
            f.Designation,
            f.Shift,
            ISNULL(a.Status, '') AS Status,
            ISNULL(a.Remarks, '') AS Remarks,
            a.AttendanceId
        FROM FilteredByShift f
        LEFT JOIN dbo.Attendance a ON f.EmployeeId = a.EmployeeId AND a.AttendanceDate = @AttendanceDate
    ),
    CountedData AS (
        SELECT COUNT(1) AS TotalCount FROM AttendanceData
    )
    SELECT 
        ad.*,
        c.TotalCount
    FROM AttendanceData ad
    CROSS JOIN CountedData c
    ORDER BY 
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Name' THEN ad.Name END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Name' THEN ad.Name END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Status' THEN ad.Status END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Status' THEN ad.Status END DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Attendance_Save
    @Assignments dbo.AttendanceSaveType READONLY,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- UPDATE existing
        UPDATE a
        SET a.Status = t.Status,
            a.Remarks = t.Remarks,
            a.UpdatedBy = @UpdatedBy,
            a.UpdatedDate = GETDATE()
        FROM dbo.Attendance a
        INNER JOIN @Assignments t ON a.EmployeeId = t.EmployeeId AND a.AttendanceDate = t.AttendanceDate;

        -- INSERT new
        INSERT INTO dbo.Attendance (EmployeeId, AttendanceDate, Shift, Status, Remarks, CreatedBy, CreatedDate)
        SELECT t.EmployeeId, t.AttendanceDate, t.Shift, t.Status, t.Remarks, @UpdatedBy, GETDATE()
        FROM @Assignments t
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.Attendance a WHERE a.EmployeeId = t.EmployeeId AND a.AttendanceDate = t.AttendanceDate
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Attendance_GetSummary
    @AttendanceDate DATE,
    @Shift NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.Status,
        COUNT(1) AS StatusCount
    FROM dbo.Attendance a
    WHERE a.AttendanceDate = @AttendanceDate AND a.Shift = @Shift
    GROUP BY a.Status;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Attendance_GetByDateRange
    @FromDate DATE,
    @ToDate DATE,
    @EmployeeId INT = NULL,
    @Shift NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        AttendanceId, EmployeeId, AttendanceDate, Shift, Status, Remarks
    FROM dbo.Attendance
    WHERE AttendanceDate BETWEEN @FromDate AND @ToDate
    AND (@EmployeeId IS NULL OR EmployeeId = @EmployeeId)
    AND (@Shift IS NULL OR Shift = @Shift);
END
GO

-- ========================================================
-- STORED PROCEDURES (SALARY)
-- ========================================================
CREATE OR ALTER PROCEDURE dbo.usp_Salary_GetCalculationData
    @EmployeeId INT,
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Employee Details
    SELECT 
        EmployeeId, Name, Designation, PerDaySalary
    FROM dbo.Employees
    WHERE EmployeeId = @EmployeeId;

    -- Attendance Stats
    SELECT 
        Status,
        COUNT(1) AS DaysCount
    FROM dbo.Attendance
    WHERE EmployeeId = @EmployeeId AND AttendanceDate BETWEEN @FromDate AND @ToDate
    GROUP BY Status;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Salary_Save
    @EmployeeId INT,
    @SalaryMonth NVARCHAR(50),
    @FromDate DATE,
    @ToDate DATE,
    @PresentDays DECIMAL(10,2),
    @AbsentDays DECIMAL(10,2),
    @HalfDays DECIMAL(10,2),
    @PerDaySalary DECIMAL(18,2),
    @SalaryForPresentDays DECIMAL(18,2),
    @Incentives DECIMAL(18,2),
    @Advance DECIMAL(18,2),
    @TotalSalary DECIMAL(18,2),
    @UpdatedBy INT = NULL,
    @SalaryId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM dbo.SalaryDetails WHERE EmployeeId = @EmployeeId AND SalaryMonth = @SalaryMonth)
        BEGIN
            UPDATE dbo.SalaryDetails
            SET FromDate = @FromDate,
                ToDate = @ToDate,
                PresentDays = @PresentDays,
                AbsentDays = @AbsentDays,
                HalfDays = @HalfDays,
                PerDaySalary = @PerDaySalary,
                SalaryForPresentDays = @SalaryForPresentDays,
                Incentives = @Incentives,
                Advance = @Advance,
                TotalSalary = @TotalSalary,
                UpdatedBy = @UpdatedBy,
                UpdatedDate = GETDATE()
            WHERE EmployeeId = @EmployeeId AND SalaryMonth = @SalaryMonth;
            
            SELECT @SalaryId = SalaryId FROM dbo.SalaryDetails WHERE EmployeeId = @EmployeeId AND SalaryMonth = @SalaryMonth;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.SalaryDetails (EmployeeId, SalaryMonth, FromDate, ToDate, PresentDays, AbsentDays, HalfDays, PerDaySalary, SalaryForPresentDays, Incentives, Advance, TotalSalary, CreatedBy, CreatedDate)
            VALUES (@EmployeeId, @SalaryMonth, @FromDate, @ToDate, @PresentDays, @AbsentDays, @HalfDays, @PerDaySalary, @SalaryForPresentDays, @Incentives, @Advance, @TotalSalary, @UpdatedBy, GETDATE());
            
            SET @SalaryId = SCOPE_IDENTITY();
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Salary_GetList
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @SalaryMonth NVARCHAR(50) = NULL,
    @Search NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn VARCHAR(50) = 'Name',
    @SortDirection VARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    WITH FilteredSalaries AS (
        SELECT 
            s.SalaryId,
            s.EmployeeId,
            e.Name,
            s.FromDate AS Date,
            s.SalaryMonth,
            e.Designation,
            s.PerDaySalary,
            s.TotalSalary,
            s.IsPaid,
            s.IsPrinted
        FROM dbo.SalaryDetails s
        INNER JOIN dbo.Employees e ON s.EmployeeId = e.EmployeeId
        WHERE (@SalaryMonth IS NULL OR s.SalaryMonth = @SalaryMonth)
        AND (@FromDate IS NULL OR @ToDate IS NULL OR (s.FromDate >= @FromDate AND s.ToDate <= @ToDate))
        AND (@Search IS NULL OR e.Name LIKE '%' + @Search + '%')
    ),
    CountedSalaries AS (
        SELECT COUNT(1) AS TotalCount FROM FilteredSalaries
    )
    SELECT 
        f.*,
        c.TotalCount
    FROM FilteredSalaries f
    CROSS JOIN CountedSalaries c
    ORDER BY 
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Name' THEN f.Name END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Name' THEN f.Name END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'SalaryMonth' THEN f.SalaryMonth END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'SalaryMonth' THEN f.SalaryMonth END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Designation' THEN f.Designation END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Designation' THEN f.Designation END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'TotalSalary' THEN f.TotalSalary END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'TotalSalary' THEN f.TotalSalary END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'PerDaySalary' THEN f.PerDaySalary END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'PerDaySalary' THEN f.PerDaySalary END DESC,
        CASE WHEN @SortDirection = 'ASC' AND @SortColumn = 'Date' THEN f.Date END ASC,
        CASE WHEN @SortDirection = 'DESC' AND @SortColumn = 'Date' THEN f.Date END DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Salary_GetById
    @SalaryId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.SalaryDetails WHERE SalaryId = @SalaryId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Salary_MarkPaid
    @SalaryId INT,
    @PaidBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.SalaryDetails SET IsPaid = 1, PaidDate = GETDATE(), UpdatedBy = @PaidBy, UpdatedDate = GETDATE() WHERE SalaryId = @SalaryId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Salary_MarkPrinted
    @SalaryId INT,
    @PrintedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.SalaryDetails SET IsPrinted = 1, PrintedDate = GETDATE(), UpdatedBy = @PrintedBy, UpdatedDate = GETDATE() WHERE SalaryId = @SalaryId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Salary_GetSummary
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @SalaryMonth NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        COUNT(DISTINCT EmployeeId) AS TotalEmployees,
        ISNULL(SUM(TotalSalary), 0) AS TotalPayment,
        ISNULL(SUM(CASE WHEN IsPaid = 1 THEN 1 ELSE 0 END), 0) AS CompletedCount,
        ISNULL(SUM(CASE WHEN IsPaid = 1 THEN TotalSalary ELSE 0 END), 0) AS PaidAmount,
        ISNULL(SUM(TotalSalary), 0) - ISNULL(SUM(CASE WHEN IsPaid = 1 THEN TotalSalary ELSE 0 END), 0) AS RemainingAmount
    FROM dbo.SalaryDetails
    WHERE (@SalaryMonth IS NULL OR SalaryMonth = @SalaryMonth)
    AND (@FromDate IS NULL OR @ToDate IS NULL OR (FromDate >= @FromDate AND ToDate <= @ToDate));
END
GO

-- ========================================================
-- STORED PROCEDURES (ACTIVITY LOG)
-- ========================================================
CREATE OR ALTER PROCEDURE dbo.usp_ActivityLog_Insert
    @UserId INT = NULL,
    @Module NVARCHAR(100),
    @Action NVARCHAR(100),
    @EntityId INT = NULL,
    @Description NVARCHAR(500),
    @IPAddress NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.HRMSActivityLog (UserId, Module, Action, EntityId, Description, IPAddress, CreatedDate)
    VALUES (@UserId, @Module, @Action, @EntityId, @Description, @IPAddress, GETDATE());
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ActivityLog_GetList
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    WITH LogData AS (
        SELECT * FROM dbo.HRMSActivityLog
    ),
    CountedData AS (
        SELECT COUNT(1) AS TotalCount FROM LogData
    )
    SELECT 
        l.*,
        c.TotalCount
    FROM LogData l
    CROSS JOIN CountedData c
    ORDER BY l.CreatedDate DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ========================================================
-- OPTIONAL DEVELOPMENT TEST DATA
-- ========================================================
-- Uncomment to seed initial configuration or employees
/*
INSERT INTO dbo.Employees (EmployeeCode, Name, Designation, City, PerDaySalary, JoinedYear)
VALUES 
('EMP001', 'Arun Kumar', 'Admin', 'Chennai', 1000, 2023),
('EMP002', 'Siva Priya', 'Operator', 'Coimbatore', 800, 2022),
('EMP003', 'Manikandan', 'Helper', 'Madurai', 600, 2024),
('EMP004', 'Jeeva S', 'Framer', 'Trichy', 900, 2021),
('EMP005', 'Lakshmi P', 'Quality Checker', 'Salem', 850, 2023);
*/
GO
