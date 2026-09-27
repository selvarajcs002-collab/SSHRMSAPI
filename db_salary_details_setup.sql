IF OBJECT_ID('dbo.Employee_Salary_Details', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employee_Salary_Details (
        EmployeeSalaryId INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeId INT NOT NULL FOREIGN KEY REFERENCES dbo.Employees(EmployeeId),
        EmployeeName NVARCHAR(150) NOT NULL,
        EmployeeCode VARCHAR(50) NOT NULL,
        PerDaySalary DECIMAL(18,2) NOT NULL,
        PresentDays INT NOT NULL,
        LeaveDays INT NOT NULL,
        HalfDays INT NOT NULL,
        TotalSalary DECIMAL(18,2) NOT NULL,
        Incentive DECIMAL(18,2) NOT NULL,
        Advance DECIMAL(18,2) NOT NULL,
        Salary DECIMAL(18,2) NOT NULL,
        Remarks NVARCHAR(500),
        VoucherNo NVARCHAR(50) NULL,
        SalaryFromDate DATE NOT NULL,
        SalaryToDate DATE NOT NULL,
        CreatedBy INT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    CREATE INDEX IX_Employee_Salary_Details_EmployeeId ON dbo.Employee_Salary_Details(EmployeeId);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetSalaryDetails
    @EmployeeId INT = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        EmployeeSalaryId,
        EmployeeId,
        EmployeeName,
        EmployeeCode,
        PerDaySalary,
        PresentDays,
        LeaveDays,
        HalfDays,
        TotalSalary,
        Incentive,
        Advance,
        Salary,
        Remarks,
        SalaryFromDate,
        SalaryToDate
    FROM 
        dbo.Employee_Salary_Details
    WHERE 
        IsActive = 1
        AND (@EmployeeId IS NULL OR EmployeeId = @EmployeeId)
        AND (@FromDate IS NULL OR SalaryFromDate >= @FromDate)
        AND (@ToDate IS NULL OR SalaryToDate <= @ToDate);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateSalaryDetails
    @EmployeeId INT,
    @SalaryFromDate DATE,
    @SalaryToDate DATE,
    @Incentive DECIMAL(18,2),
    @Advance DECIMAL(18,2),
    @Remarks NVARCHAR(500),
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Validation
        IF EXISTS (
            SELECT 1 
            FROM dbo.Employee_Salary_Details 
            WHERE EmployeeId = @EmployeeId 
              AND SalaryFromDate = @SalaryFromDate 
              AND SalaryToDate = @SalaryToDate
              AND IsActive = 1
        )
        BEGIN
            THROW 50001, 'Salary details already exist for this employee and salary period.', 1;
        END

        -- 2. Employee Details
        DECLARE @EmployeeName NVARCHAR(150);
        DECLARE @EmployeeCode VARCHAR(50);
        DECLARE @PerDaySalary DECIMAL(18,2);

        SELECT 
            @EmployeeName = Name, 
            @EmployeeCode = EmployeeCode, 
            @PerDaySalary = PerDaySalary 
        FROM dbo.Employees 
        WHERE EmployeeId = @EmployeeId AND IsActive = 1;

        IF @EmployeeName IS NULL
        BEGIN
            THROW 50002, 'Employee not found or inactive.', 1;
        END

        -- 3. Attendance Calculation
        DECLARE @PresentDays INT = 0;
        DECLARE @LeaveDays INT = 0;
        DECLARE @HalfDays INT = 0;

        SELECT 
            @PresentDays = ISNULL(SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END), 0),
            @LeaveDays = ISNULL(SUM(CASE WHEN Status = 'Leave' THEN 1 ELSE 0 END), 0),
            @HalfDays = ISNULL(SUM(CASE WHEN Status = 'Half-Day' THEN 1 ELSE 0 END), 0)
        FROM dbo.Attendance
        WHERE EmployeeId = @EmployeeId
          AND AttendanceDate >= @SalaryFromDate
          AND AttendanceDate < DATEADD(DAY, 1, @SalaryToDate);

        -- 4. Salary Calculation
        DECLARE @TotalSalary DECIMAL(18,2) = 0;
        DECLARE @FinalSalary DECIMAL(18,2) = 0;

        SET @TotalSalary = (@PerDaySalary * @PresentDays) + ((@PerDaySalary / 2.0) * @HalfDays);
        SET @FinalSalary = @TotalSalary + @Incentive - @Advance;

        -- 5. Insert
        DECLARE @InsertedData TABLE (EmployeeSalaryId INT);
        DECLARE @EmployeeSalaryId INT;

        INSERT INTO dbo.Employee_Salary_Details (
            EmployeeId, EmployeeName, EmployeeCode, PerDaySalary,
            PresentDays, LeaveDays, HalfDays, TotalSalary,
            Incentive, Advance, Salary, Remarks,
            SalaryFromDate, SalaryToDate, CreatedBy
        )
        OUTPUT inserted.EmployeeSalaryId INTO @InsertedData
        VALUES (
            @EmployeeId, @EmployeeName, @EmployeeCode, @PerDaySalary,
            @PresentDays, @LeaveDays, @HalfDays, @TotalSalary,
            @Incentive, @Advance, @FinalSalary, @Remarks,
            @SalaryFromDate, @SalaryToDate, @CreatedBy
        );

        SELECT TOP 1 @EmployeeSalaryId = EmployeeSalaryId FROM @InsertedData;

        COMMIT TRANSACTION;

        -- Return newly created details
        EXEC dbo.usp_GetSalaryDetails @EmployeeId = @EmployeeId, @FromDate = @SalaryFromDate, @ToDate = @SalaryToDate;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateSalaryDetails
    @EmployeeId INT,
    @SalaryFromDate DATE,
    @SalaryToDate DATE,
    @Incentive DECIMAL(18,2),
    @Advance DECIMAL(18,2),
    @Remarks NVARCHAR(500),
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Find existing record
        DECLARE @EmployeeSalaryId INT;
        DECLARE @EmployeeName NVARCHAR(150);
        DECLARE @EmployeeCode VARCHAR(50);
        DECLARE @PerDaySalary DECIMAL(18,2);

        SELECT 
            @EmployeeSalaryId = EmployeeSalaryId,
            @EmployeeName = EmployeeName,
            @EmployeeCode = EmployeeCode,
            @PerDaySalary = PerDaySalary
        FROM dbo.Employee_Salary_Details
        WHERE EmployeeId = @EmployeeId 
          AND SalaryFromDate = @SalaryFromDate 
          AND SalaryToDate = @SalaryToDate
          AND IsActive = 1;

        IF @EmployeeSalaryId IS NULL
        BEGIN
            THROW 50003, 'Salary details not found for this employee and salary period.', 1;
        END

        -- 2. Recalculate Attendance
        DECLARE @PresentDays INT = 0;
        DECLARE @LeaveDays INT = 0;
        DECLARE @HalfDays INT = 0;

        SELECT 
            @PresentDays = ISNULL(SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END), 0),
            @LeaveDays = ISNULL(SUM(CASE WHEN Status = 'Leave' THEN 1 ELSE 0 END), 0),
            @HalfDays = ISNULL(SUM(CASE WHEN Status = 'Half-Day' THEN 1 ELSE 0 END), 0)
        FROM dbo.Attendance
        WHERE EmployeeId = @EmployeeId
          AND AttendanceDate >= @SalaryFromDate
          AND AttendanceDate < DATEADD(DAY, 1, @SalaryToDate);

        -- 3. Salary Calculation
        DECLARE @TotalSalary DECIMAL(18,2) = 0;
        DECLARE @FinalSalary DECIMAL(18,2) = 0;

        SET @TotalSalary = (@PerDaySalary * @PresentDays) + ((@PerDaySalary / 2.0) * @HalfDays);
        SET @FinalSalary = @TotalSalary + @Incentive - @Advance;

        -- 4. Update
        UPDATE dbo.Employee_Salary_Details
        SET 
            PresentDays = @PresentDays,
            LeaveDays = @LeaveDays,
            HalfDays = @HalfDays,
            TotalSalary = @TotalSalary,
            Incentive = @Incentive,
            Advance = @Advance,
            Salary = @FinalSalary,
            Remarks = @Remarks,
            UpdatedBy = @UpdatedBy,
            UpdatedDate = GETDATE()
        WHERE EmployeeSalaryId = @EmployeeSalaryId;

        COMMIT TRANSACTION;

        -- Return updated details
        EXEC dbo.usp_GetSalaryDetails @EmployeeId = @EmployeeId, @FromDate = @SalaryFromDate, @ToDate = @SalaryToDate;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
