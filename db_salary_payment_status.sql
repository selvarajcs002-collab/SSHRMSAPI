-- Salary payment-status support for Employee_Salary_Details.
-- Safe to run more than once.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ========================================================
-- 1. COLUMNS
-- ========================================================
IF COL_LENGTH('dbo.Employee_Salary_Details', 'PaymentStatus') IS NULL
BEGIN
    ALTER TABLE dbo.Employee_Salary_Details
        ADD PaymentStatus NVARCHAR(20) NOT NULL
            CONSTRAINT DF_EmployeeSalaryDetails_PaymentStatus
            DEFAULT ('Pending');
END
GO

IF COL_LENGTH('dbo.Employee_Salary_Details', 'PaidDate') IS NULL
BEGIN
    ALTER TABLE dbo.Employee_Salary_Details
        ADD PaidDate DATETIME2 NULL;
END
GO

IF COL_LENGTH('dbo.Employee_Salary_Details', 'PaidBy') IS NULL
BEGIN
    ALTER TABLE dbo.Employee_Salary_Details
        ADD PaidBy INT NULL;
END
GO

IF COL_LENGTH('dbo.Employee_Salary_Details', 'VoucherNo') IS NULL
BEGIN
    ALTER TABLE dbo.Employee_Salary_Details
        ADD VoucherNo NVARCHAR(50) NULL;
END
GO

UPDATE dbo.Employee_Salary_Details
SET PaymentStatus = 'Pending'
WHERE PaymentStatus IS NULL
   OR LTRIM(RTRIM(PaymentStatus)) = '';
GO

-- ========================================================
-- 2. CHECK CONSTRAINT
-- ========================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Employee_Salary_Details_PaymentStatus'
      AND parent_object_id = OBJECT_ID(N'dbo.Employee_Salary_Details')
)
BEGIN
    ALTER TABLE dbo.Employee_Salary_Details
        ADD CONSTRAINT CK_Employee_Salary_Details_PaymentStatus
        CHECK (PaymentStatus IN ('Pending', 'Paid'));
END
GO

-- ========================================================
-- 3. DEACTIVATE DUPLICATE ACTIVE PERIODS
-- ========================================================
;WITH DuplicateSalary AS (
    SELECT
        EmployeeSalaryId,
        ROW_NUMBER() OVER (
            PARTITION BY EmployeeId, SalaryFromDate, SalaryToDate
            ORDER BY EmployeeSalaryId DESC
        ) AS RowNum
    FROM dbo.Employee_Salary_Details
    WHERE IsActive = 1
)
UPDATE s
SET s.IsActive = 0,
    s.UpdatedDate = GETDATE()
FROM dbo.Employee_Salary_Details s
INNER JOIN DuplicateSalary d ON d.EmployeeSalaryId = s.EmployeeSalaryId
WHERE d.RowNum > 1;
GO

-- ========================================================
-- 4. UNIQUE FILTERED INDEX
-- ========================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_Employee_Salary_Details_EmployeePeriod_Active'
      AND object_id = OBJECT_ID(N'dbo.Employee_Salary_Details')
)
BEGIN
    CREATE UNIQUE INDEX UX_Employee_Salary_Details_EmployeePeriod_Active
        ON dbo.Employee_Salary_Details (EmployeeId, SalaryFromDate, SalaryToDate)
        WHERE IsActive = 1;
END
GO

-- ========================================================
-- 5. GET SAVED SALARY RECORD
-- ========================================================
CREATE OR ALTER PROCEDURE dbo.usp_GetSalaryDetails
    @EmployeeId INT = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.EmployeeSalaryId,
        s.EmployeeId,
        s.EmployeeCode,
        s.EmployeeName,
        ISNULL(e.Designation, '') AS Designation,
        s.PerDaySalary,
        s.PresentDays,
        s.LeaveDays,
        s.HalfDays,
        CAST((s.PerDaySalary * s.PresentDays) AS DECIMAL(18,2)) AS PresentSalary,
        CAST(((s.PerDaySalary / 2.0) * s.HalfDays) AS DECIMAL(18,2)) AS HalfDaySalary,
        s.TotalSalary,
        s.Incentive,
        s.Advance,
        s.Salary,
        s.Remarks,
        s.SalaryFromDate,
        s.SalaryToDate,
        ISNULL(s.PaymentStatus, 'Pending') AS PaymentStatus,
        s.PaidDate,
        s.PaidBy,
        s.VoucherNo
    FROM dbo.Employee_Salary_Details s
    LEFT JOIN dbo.Employees e ON e.EmployeeId = s.EmployeeId
    WHERE s.IsActive = 1
      AND (@EmployeeId IS NULL OR s.EmployeeId = @EmployeeId)
      AND (@FromDate IS NULL OR s.SalaryFromDate = @FromDate)
      AND (@ToDate IS NULL OR s.SalaryToDate = @ToDate);
END
GO

-- ========================================================
-- 6. GET ALL EMPLOYEES FOR A SALARY PERIOD
-- ========================================================
CREATE OR ALTER PROCEDURE dbo.usp_GetEmployeeSalaryDetails
    @EmployeeId INT = NULL,
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromDate IS NULL OR @ToDate IS NULL OR @FromDate > @ToDate
        THROW 50004, 'Invalid salary period.', 1;

    ;WITH AttendanceAgg AS (
        SELECT
            a.EmployeeId,
            ISNULL(SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END), 0) AS PresentDays,
            ISNULL(SUM(CASE WHEN a.Status = 'Leave' THEN 1 ELSE 0 END), 0) AS LeaveDays,
            ISNULL(SUM(CASE WHEN a.Status = 'Half-Day' THEN 1 ELSE 0 END), 0) AS HalfDays
        FROM dbo.Attendance a
        WHERE a.AttendanceDate >= @FromDate
          AND a.AttendanceDate < DATEADD(DAY, 1, @ToDate)
        GROUP BY a.EmployeeId
    )
    SELECT
        ISNULL(s.EmployeeSalaryId, 0) AS EmployeeSalaryId,
        e.EmployeeId,
        e.EmployeeCode,
        e.Name AS EmployeeName,
        ISNULL(e.Designation, '') AS Designation,
        CAST(ISNULL(s.PerDaySalary, e.PerDaySalary) AS DECIMAL(18,2)) AS PerDaySalary,
        CAST(ISNULL(s.PresentDays, ISNULL(att.PresentDays, 0)) AS INT) AS PresentDays,
        CAST(ISNULL(s.LeaveDays, ISNULL(att.LeaveDays, 0)) AS INT) AS LeaveDays,
        CAST(ISNULL(s.HalfDays, ISNULL(att.HalfDays, 0)) AS INT) AS HalfDays,
        CAST(
            ISNULL(s.PerDaySalary, e.PerDaySalary)
            * ISNULL(s.PresentDays, ISNULL(att.PresentDays, 0))
            AS DECIMAL(18,2)
        ) AS PresentSalary,
        CAST(
            (ISNULL(s.PerDaySalary, e.PerDaySalary) / 2.0)
            * ISNULL(s.HalfDays, ISNULL(att.HalfDays, 0))
            AS DECIMAL(18,2)
        ) AS HalfDaySalary,
        CAST(
            ISNULL(
                s.TotalSalary,
                (ISNULL(s.PerDaySalary, e.PerDaySalary) * ISNULL(att.PresentDays, 0))
                + ((ISNULL(s.PerDaySalary, e.PerDaySalary) / 2.0) * ISNULL(att.HalfDays, 0))
            ) AS DECIMAL(18,2)
        ) AS TotalSalary,
        CAST(ISNULL(s.Incentive, 0) AS DECIMAL(18,2)) AS Incentive,
        CAST(ISNULL(s.Advance, 0) AS DECIMAL(18,2)) AS Advance,
        CAST(
            ISNULL(
                s.Salary,
                (ISNULL(s.PerDaySalary, e.PerDaySalary) * ISNULL(att.PresentDays, 0))
                + ((ISNULL(s.PerDaySalary, e.PerDaySalary) / 2.0) * ISNULL(att.HalfDays, 0))
            ) AS DECIMAL(18,2)
        ) AS Salary,
        s.Remarks,
        CAST(ISNULL(s.SalaryFromDate, @FromDate) AS DATE) AS SalaryFromDate,
        CAST(ISNULL(s.SalaryToDate, @ToDate) AS DATE) AS SalaryToDate,
        ISNULL(s.PaymentStatus, 'Pending') AS PaymentStatus,
        s.PaidDate,
        s.PaidBy,
        s.VoucherNo
    FROM dbo.Employees e
    LEFT JOIN AttendanceAgg att ON att.EmployeeId = e.EmployeeId
    LEFT JOIN dbo.Employee_Salary_Details s
        ON s.EmployeeId = e.EmployeeId
       AND s.IsActive = 1
       AND s.SalaryFromDate = @FromDate
       AND s.SalaryToDate = @ToDate
    WHERE e.IsActive = 1
      AND (@EmployeeId IS NULL OR e.EmployeeId = @EmployeeId)
    ORDER BY e.Name;
END
GO

-- ========================================================
-- 7. CREATE SALARY DETAILS
-- ========================================================
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
    SET XACT_ABORT ON;

    IF @EmployeeId IS NULL OR @EmployeeId <= 0
        THROW 50004, 'Invalid EmployeeId.', 1;

    IF @SalaryFromDate IS NULL OR @SalaryToDate IS NULL OR @SalaryFromDate > @SalaryToDate
        THROW 50004, 'Invalid salary period.', 1;

    IF @Incentive < 0 OR @Advance < 0
        THROW 50004, 'Incentive and Advance cannot be negative.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (
            SELECT 1
            FROM dbo.Employee_Salary_Details WITH (UPDLOCK, HOLDLOCK)
            WHERE EmployeeId = @EmployeeId
              AND SalaryFromDate = @SalaryFromDate
              AND SalaryToDate = @SalaryToDate
              AND IsActive = 1
        )
        BEGIN
            THROW 50001, 'Salary details already exist for this employee and salary period.', 1;
        END

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
            THROW 50002, 'Employee not found or inactive.', 1;

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

        DECLARE @TotalSalary DECIMAL(18,2) = (@PerDaySalary * @PresentDays) + ((@PerDaySalary / 2.0) * @HalfDays);
        DECLARE @FinalSalary DECIMAL(18,2) = @TotalSalary + @Incentive - @Advance;

        INSERT INTO dbo.Employee_Salary_Details (
            EmployeeId, EmployeeName, EmployeeCode, PerDaySalary,
            PresentDays, LeaveDays, HalfDays, TotalSalary,
            Incentive, Advance, Salary, Remarks,
            SalaryFromDate, SalaryToDate, PaymentStatus, CreatedBy
        )
        VALUES (
            @EmployeeId, @EmployeeName, @EmployeeCode, @PerDaySalary,
            @PresentDays, @LeaveDays, @HalfDays, @TotalSalary,
            @Incentive, @Advance, @FinalSalary, @Remarks,
            @SalaryFromDate, @SalaryToDate, 'Pending', @CreatedBy
        );

        COMMIT TRANSACTION;

        EXEC dbo.usp_GetSalaryDetails
            @EmployeeId = @EmployeeId,
            @FromDate = @SalaryFromDate,
            @ToDate = @SalaryToDate;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ========================================================
-- 8. UPDATE SALARY DETAILS
-- ========================================================
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
    SET XACT_ABORT ON;

    IF @EmployeeId IS NULL OR @EmployeeId <= 0
        THROW 50004, 'Invalid EmployeeId.', 1;

    IF @SalaryFromDate IS NULL OR @SalaryToDate IS NULL OR @SalaryFromDate > @SalaryToDate
        THROW 50004, 'Invalid salary period.', 1;

    IF @Incentive < 0 OR @Advance < 0
        THROW 50004, 'Incentive and Advance cannot be negative.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @EmployeeSalaryId INT;
        DECLARE @ExistingIncentive DECIMAL(18,2);
        DECLARE @ExistingAdvance DECIMAL(18,2);
        DECLARE @ExistingPaymentStatus NVARCHAR(20);

        SELECT
            @EmployeeSalaryId = EmployeeSalaryId,
            @ExistingIncentive = Incentive,
            @ExistingAdvance = Advance,
            @ExistingPaymentStatus = ISNULL(PaymentStatus, 'Pending')
        FROM dbo.Employee_Salary_Details WITH (UPDLOCK, HOLDLOCK)
        WHERE EmployeeId = @EmployeeId
          AND SalaryFromDate = @SalaryFromDate
          AND SalaryToDate = @SalaryToDate
          AND IsActive = 1;

        IF @EmployeeSalaryId IS NULL
            THROW 50003, 'Salary details not found for this employee and salary period.', 1;

        IF @ExistingPaymentStatus = 'Paid'
        BEGIN
            IF @Incentive <> @ExistingIncentive OR @Advance <> @ExistingAdvance
                THROW 50008, 'Paid salary cannot be modified.', 1;

            UPDATE dbo.Employee_Salary_Details
            SET Remarks = @Remarks,
                UpdatedBy = @UpdatedBy,
                UpdatedDate = GETDATE()
            WHERE EmployeeSalaryId = @EmployeeSalaryId;
        END
        ELSE
        BEGIN
            DECLARE @PerDaySalary DECIMAL(18,2);
            DECLARE @PresentDays INT = 0;
            DECLARE @LeaveDays INT = 0;
            DECLARE @HalfDays INT = 0;

            SELECT @PerDaySalary = PerDaySalary
            FROM dbo.Employee_Salary_Details
            WHERE EmployeeSalaryId = @EmployeeSalaryId;

            SELECT
                @PresentDays = ISNULL(SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END), 0),
                @LeaveDays = ISNULL(SUM(CASE WHEN Status = 'Leave' THEN 1 ELSE 0 END), 0),
                @HalfDays = ISNULL(SUM(CASE WHEN Status = 'Half-Day' THEN 1 ELSE 0 END), 0)
            FROM dbo.Attendance
            WHERE EmployeeId = @EmployeeId
              AND AttendanceDate >= @SalaryFromDate
              AND AttendanceDate < DATEADD(DAY, 1, @SalaryToDate);

            DECLARE @TotalSalary DECIMAL(18,2) = (@PerDaySalary * @PresentDays) + ((@PerDaySalary / 2.0) * @HalfDays);
            DECLARE @FinalSalary DECIMAL(18,2) = @TotalSalary + @Incentive - @Advance;

            UPDATE dbo.Employee_Salary_Details
            SET PresentDays = @PresentDays,
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
        END

        COMMIT TRANSACTION;

        EXEC dbo.usp_GetSalaryDetails
            @EmployeeId = @EmployeeId,
            @FromDate = @SalaryFromDate,
            @ToDate = @SalaryToDate;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ========================================================
-- 9. MARK SALARY AS PAID
-- ========================================================
CREATE OR ALTER PROCEDURE dbo.usp_MarkEmployeeSalaryAsPaid
    @EmployeeId INT,
    @SalaryFromDate DATE,
    @SalaryToDate DATE,
    @PaidBy INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @EmployeeId IS NULL OR @EmployeeId <= 0
        THROW 50004, 'Invalid EmployeeId.', 1;

    IF @SalaryFromDate IS NULL OR @SalaryToDate IS NULL OR @SalaryFromDate > @SalaryToDate
        THROW 50004, 'Invalid salary period.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @EmployeeSalaryId INT;
        DECLARE @PaymentStatus NVARCHAR(20);
        DECLARE @Salary DECIMAL(18,2);

        SELECT
            @EmployeeSalaryId = EmployeeSalaryId,
            @PaymentStatus = ISNULL(PaymentStatus, 'Pending'),
            @Salary = Salary
        FROM dbo.Employee_Salary_Details WITH (UPDLOCK, HOLDLOCK)
        WHERE EmployeeId = @EmployeeId
          AND SalaryFromDate = @SalaryFromDate
          AND SalaryToDate = @SalaryToDate
          AND IsActive = 1;

        IF @EmployeeSalaryId IS NULL
            THROW 50003, 'Salary details not found for this employee and salary period.', 1;

        IF @PaymentStatus = 'Paid'
            THROW 50006, 'Salary is already marked as paid.', 1;

        IF @Salary IS NULL OR @Salary < 0
            THROW 50007, 'Salary amount cannot be negative.', 1;

        UPDATE dbo.Employee_Salary_Details
        SET PaymentStatus = 'Paid',
            PaidDate = GETDATE(),
            PaidBy = @PaidBy,
            UpdatedBy = @PaidBy,
            UpdatedDate = GETDATE()
        WHERE EmployeeSalaryId = @EmployeeSalaryId;

        COMMIT TRANSACTION;

        EXEC dbo.usp_GetSalaryDetails
            @EmployeeId = @EmployeeId,
            @FromDate = @SalaryFromDate,
            @ToDate = @SalaryToDate;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ========================================================
-- 10. SALARY SUMMARY FOR SELECTED PERIOD
-- ========================================================
CREATE OR ALTER PROCEDURE dbo.usp_GetEmployeeSalarySummary
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromDate IS NULL OR @ToDate IS NULL OR @FromDate > @ToDate
        THROW 50004, 'Invalid salary period.', 1;

    ;WITH AttendanceAgg AS (
        SELECT
            a.EmployeeId,
            ISNULL(SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END), 0) AS PresentDays,
            ISNULL(SUM(CASE WHEN a.Status = 'Half-Day' THEN 1 ELSE 0 END), 0) AS HalfDays
        FROM dbo.Attendance a
        WHERE a.AttendanceDate >= @FromDate
          AND a.AttendanceDate < DATEADD(DAY, 1, @ToDate)
        GROUP BY a.EmployeeId
    ),
    PeriodSalaries AS (
        SELECT
            e.EmployeeId,
            ISNULL(s.PaymentStatus, 'Pending') AS PaymentStatus,
            CAST(
                ISNULL(
                    s.Salary,
                    (e.PerDaySalary * ISNULL(att.PresentDays, 0))
                    + ((e.PerDaySalary / 2.0) * ISNULL(att.HalfDays, 0))
                ) AS DECIMAL(18,2)
            ) AS Salary
        FROM dbo.Employees e
        LEFT JOIN AttendanceAgg att ON att.EmployeeId = e.EmployeeId
        LEFT JOIN dbo.Employee_Salary_Details s
            ON s.EmployeeId = e.EmployeeId
           AND s.IsActive = 1
           AND s.SalaryFromDate = @FromDate
           AND s.SalaryToDate = @ToDate
        WHERE e.IsActive = 1
    )
    SELECT
        COUNT(1) AS TotalEmployees,
        CAST(ISNULL(SUM(Salary), 0) AS DECIMAL(18,2)) AS TotalPayment,
        SUM(CASE WHEN PaymentStatus = 'Paid' THEN 1 ELSE 0 END) AS CompletedPaid,
        SUM(CASE WHEN PaymentStatus = 'Pending' THEN 1 ELSE 0 END) AS Remaining,
        CAST(ISNULL(SUM(CASE WHEN PaymentStatus = 'Paid' THEN Salary ELSE 0 END), 0) AS DECIMAL(18,2)) AS PaidAmount,
        CAST(ISNULL(SUM(CASE WHEN PaymentStatus = 'Pending' THEN Salary ELSE 0 END), 0) AS DECIMAL(18,2)) AS PendingAmount
    FROM PeriodSalaries;
END
GO
