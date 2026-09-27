USE [SSManagement_HRMS];
GO
CREATE OR ALTER PROCEDURE dbo.usp_ShiftManagement_Save
    @EmployeeId INT = NULL,
    @Name NVARCHAR(150),
    @Designation NVARCHAR(100),
    @City NVARCHAR(100),
    @PerDaySalary DECIMAL(18,2),
    @JoinedYear INT,
    @Shift NVARCHAR(50),
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF @EmployeeId IS NULL OR @EmployeeId = 0
        BEGIN
            SELECT TOP 1 @EmployeeId = EmployeeId FROM dbo.Employees WHERE Name = @Name AND IsActive = 1;
        END
        
        IF @EmployeeId IS NOT NULL AND @EmployeeId > 0
        BEGIN
            UPDATE dbo.Employees 
            SET Name = @Name, Designation = @Designation, City = @City, PerDaySalary = @PerDaySalary, JoinedYear = @JoinedYear, UpdatedBy = @UpdatedBy, UpdatedDate = GETDATE()
            WHERE EmployeeId = @EmployeeId;
        END
        ELSE
        BEGIN
            DECLARE @NewEmpCode VARCHAR(50) = 'EMP_' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR);
            INSERT INTO dbo.Employees (EmployeeCode, Name, Designation, City, PerDaySalary, JoinedYear, IsActive, CreatedBy, CreatedDate)
            VALUES (@NewEmpCode, @Name, @Designation, @City, @PerDaySalary, @JoinedYear, 1, @UpdatedBy, GETDATE());
            
            SET @EmployeeId = SCOPE_IDENTITY();
        END
        
        UPDATE dbo.EmployeeShifts
        SET IsActive = 0, EffectiveTo = CAST(GETDATE() AS DATE), UpdatedBy = @UpdatedBy, UpdatedDate = GETDATE()
        WHERE EmployeeId = @EmployeeId AND IsActive = 1 AND Shift != @Shift;
        
        IF NOT EXISTS (SELECT 1 FROM dbo.EmployeeShifts WHERE EmployeeId = @EmployeeId AND IsActive = 1 AND Shift = @Shift)
        BEGIN
            INSERT INTO dbo.EmployeeShifts (EmployeeId, Shift, EffectiveFrom, IsActive, CreatedBy, CreatedDate)
            VALUES (@EmployeeId, @Shift, CAST(GETDATE() AS DATE), 1, @UpdatedBy, GETDATE());
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
