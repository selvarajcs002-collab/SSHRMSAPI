USE [SSManagement_HRMS];
GO

-- 1. Index Recommendation
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeShifts_EmployeeId_IsActive' AND object_id = OBJECT_ID('dbo.EmployeeShifts'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_EmployeeShifts_EmployeeId_IsActive] ON [dbo].[EmployeeShifts]
    (
        [EmployeeId] ASC,
        [IsActive] ASC
    )
    INCLUDE([Shift], [EffectiveFrom], [EffectiveTo]);
END
GO

-- 2. Stored Procedure: usp_SaveEmployeeShift
CREATE OR ALTER PROCEDURE dbo.usp_SaveEmployeeShift
    @EmployeeShiftId INT = NULL,
    @EmployeeId INT,
    @Shift VARCHAR(50),
    @EffectiveFrom DATE,
    @EffectiveTo DATE = NULL,
    @IsActive BIT,
    @CreatedBy INT,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @ExistingId INT = NULL;

        -- Check if an active record already exists for the employee
        SELECT TOP 1 @ExistingId = EmployeeShiftId 
        FROM dbo.EmployeeShifts WITH (UPDLOCK, SERIALIZABLE)
        WHERE EmployeeId = @EmployeeId AND IsActive = 1;

        IF @ExistingId IS NOT NULL
        BEGIN
            -- Update the existing active record
            UPDATE dbo.EmployeeShifts
            SET Shift = @Shift,
                EffectiveFrom = @EffectiveFrom,
                EffectiveTo = @EffectiveTo,
                IsActive = @IsActive,
                UpdatedBy = ISNULL(@UpdatedBy, @CreatedBy),
                UpdatedDate = GETDATE()
            WHERE EmployeeShiftId = @ExistingId;

            -- Return the updated ID
            SELECT @ExistingId AS EmployeeShiftId;
        END
        ELSE
        BEGIN
            -- Insert a new record
            INSERT INTO dbo.EmployeeShifts (EmployeeId, Shift, EffectiveFrom, EffectiveTo, IsActive, CreatedBy, CreatedDate)
            VALUES (@EmployeeId, @Shift, @EffectiveFrom, @EffectiveTo, @IsActive, @CreatedBy, GETDATE());

            -- Return the newly inserted ID
            SELECT CAST(SCOPE_IDENTITY() AS INT) AS EmployeeShiftId;
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

-- 3. Stored Procedure: usp_GetEmployeeShifts
CREATE OR ALTER PROCEDURE dbo.usp_GetEmployeeShifts
    @EmployeeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @EmployeeId IS NULL
    BEGIN
      SELECT 
            e.EmployeeId,
			e.Designation,
			es.EmployeeShiftId,
            es.EmployeeId,
            e.Name AS EmployeeName,
            es.Shift
        FROM dbo.Employees e
        LEFT JOIN dbo.EmployeeShifts es ON e.EmployeeId = es.EmployeeId;
    END
    ELSE
    BEGIN
        SELECT 
            es.EmployeeShiftId,
            es.EmployeeId,
            e.Name AS EmployeeName,
            es.Shift
        FROM dbo.EmployeeShifts es
        LEFT JOIN dbo.Employees e ON es.EmployeeId = e.EmployeeId
        WHERE es.EmployeeId = @EmployeeId;
    END
END
GO
