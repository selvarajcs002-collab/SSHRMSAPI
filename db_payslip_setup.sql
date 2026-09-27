-- Payslip voucher support for Employee_Salary_Details.
-- Safe to run more than once.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('dbo.Employee_Salary_Details', 'VoucherNo') IS NULL
BEGIN
    ALTER TABLE dbo.Employee_Salary_Details
        ADD VoucherNo NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_Employee_Salary_Details_VoucherNo'
      AND object_id = OBJECT_ID(N'dbo.Employee_Salary_Details')
)
BEGIN
    CREATE UNIQUE INDEX UX_Employee_Salary_Details_VoucherNo
        ON dbo.Employee_Salary_Details (VoucherNo)
        WHERE VoucherNo IS NOT NULL;
END
GO

IF OBJECT_ID('dbo.Salary_Voucher_Counter', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Salary_Voucher_Counter
    (
        FinancialYear NVARCHAR(9) NOT NULL CONSTRAINT PK_Salary_Voucher_Counter PRIMARY KEY,
        LastNumber INT NOT NULL CONSTRAINT DF_Salary_Voucher_Counter_LastNumber DEFAULT (0),
        CONSTRAINT CK_Salary_Voucher_Counter_LastNumber CHECK (LastNumber >= 0)
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetOrCreateSalaryPayslip
    @EmployeeId INT,
    @SalaryFromDate DATE,
    @SalaryToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @EmployeeId IS NULL OR @EmployeeId <= 0
        THROW 50004, 'Invalid EmployeeId.', 1;

    IF @SalaryFromDate IS NULL OR @SalaryToDate IS NULL OR @SalaryFromDate > @SalaryToDate
        THROW 50004, 'Invalid salary period.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE EmployeeId = @EmployeeId)
        THROW 50002, 'Employee not found.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @EmployeeSalaryId INT;
        DECLARE @VoucherNo NVARCHAR(50);

        SELECT TOP 1
            @EmployeeSalaryId = EmployeeSalaryId,
            @VoucherNo = VoucherNo
        FROM dbo.Employee_Salary_Details WITH (UPDLOCK, HOLDLOCK)
        WHERE EmployeeId = @EmployeeId
          AND SalaryFromDate = @SalaryFromDate
          AND SalaryToDate = @SalaryToDate
          AND IsActive = 1
        ORDER BY EmployeeSalaryId DESC;

        IF @EmployeeSalaryId IS NULL
            THROW 50003, 'Salary details not found.', 1;

        IF @VoucherNo IS NULL OR LTRIM(RTRIM(@VoucherNo)) = ''
        BEGIN
            DECLARE @StartYear INT = CASE
                WHEN MONTH(@SalaryToDate) >= 4 THEN YEAR(@SalaryToDate)
                ELSE YEAR(@SalaryToDate) - 1
            END;
            DECLARE @FinancialYear NVARCHAR(9) = CONCAT(@StartYear, '-', @StartYear + 1);
            DECLARE @NextNumber INT;

            ;MERGE dbo.Salary_Voucher_Counter WITH (HOLDLOCK) AS target
            USING (SELECT @FinancialYear AS FinancialYear) AS source
                ON target.FinancialYear = source.FinancialYear
            WHEN NOT MATCHED THEN
                INSERT (FinancialYear, LastNumber)
                VALUES (
                    source.FinancialYear,
                    ISNULL((
                        SELECT MAX(TRY_CAST(PARSENAME(REPLACE(VoucherNo, '_', '.'), 1) AS INT))
                        FROM dbo.Employee_Salary_Details
                        WHERE VoucherNo LIKE 'SS_' + @FinancialYear + '[_]%'
                    ), 0)
                );

            UPDATE dbo.Salary_Voucher_Counter
            SET @NextNumber = LastNumber = LastNumber + 1
            WHERE FinancialYear = @FinancialYear;

            IF @NextNumber IS NULL OR @NextNumber <= 0
                THROW 50005, 'Voucher generation failed.', 1;

            SET @VoucherNo = CONCAT('SS_', @FinancialYear, '_', FORMAT(@NextNumber, '000'));

            UPDATE dbo.Employee_Salary_Details
            SET VoucherNo = @VoucherNo,
                UpdatedDate = GETDATE()
            WHERE EmployeeSalaryId = @EmployeeSalaryId
              AND (VoucherNo IS NULL OR LTRIM(RTRIM(VoucherNo)) = '');

            IF @@ROWCOUNT = 0
            BEGIN
                SELECT @VoucherNo = VoucherNo
                FROM dbo.Employee_Salary_Details
                WHERE EmployeeSalaryId = @EmployeeSalaryId;
            END
        END

        COMMIT TRANSACTION;

        SELECT
            s.EmployeeSalaryId,
            s.EmployeeId,
            s.EmployeeName,
            s.EmployeeCode,
            ISNULL(e.Designation, '') AS Designation,
            s.PerDaySalary,
            s.PresentDays,
            s.LeaveDays AS AbsentDays,
            s.HalfDays,
            s.TotalSalary,
            s.Incentive,
            s.Advance,
            s.Salary AS NetSalary,
            s.Remarks,
            s.SalaryFromDate,
            s.SalaryToDate,
            s.VoucherNo
        FROM dbo.Employee_Salary_Details s
        LEFT JOIN dbo.Employees e ON e.EmployeeId = s.EmployeeId
        WHERE s.EmployeeSalaryId = @EmployeeSalaryId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
