-- =======================================================================================
-- Attendance Management Table Setup, Unique Index, and Stored Procedures
-- =======================================================================================

-- 1. Ensure Attendance Table exists (assuming it might need alteration or creation)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Attendance]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Attendance](
        [AttendanceId] [int] IDENTITY(1,1) NOT NULL,
        [EmployeeId] [int] NOT NULL,
        [AttendanceDate] [date] NOT NULL,
        [Shift] [varchar](50) NOT NULL,
        [Status] [varchar](50) NOT NULL,
        [Remarks] [nvarchar](500) NULL,
        [CreatedBy] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [UpdatedBy] [int] NULL,
        [UpdatedDate] [datetime] NULL,
        CONSTRAINT [PK_Attendance] PRIMARY KEY CLUSTERED ([AttendanceId] ASC)
    )
END
GO

-- 2. Create Unique Index to enforce One Employee + One Date + One Shift = One Record
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Attendance]') AND name = N'UX_Attendance_Employee_Date_Shift')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Attendance_Employee_Date_Shift] ON [dbo].[Attendance]
    (
        [EmployeeId] ASC,
        [AttendanceDate] ASC,
        [Shift] ASC
    )
END
GO

-- 3. Stored Procedure for Upsert (Insert or Update)
CREATE OR ALTER PROCEDURE [dbo].[usp_Attendance_Upsert]
    @EmployeeId INT,
    @AttendanceDate DATE,
    @Shift VARCHAR(50),
    @Status VARCHAR(50),
    @Remarks NVARCHAR(500) = NULL,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Step 1: Validate Employee exists and is active
        IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE EmployeeId = @EmployeeId AND IsActive = 1)
        BEGIN
            RAISERROR('Invalid or inactive Employee.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Step 2: Validate Applicable Shift
        -- Employee must have the given shift assigned on the attendance date
        IF NOT EXISTS (
            SELECT 1 
            FROM dbo.EmployeeShifts 
            WHERE EmployeeId = @EmployeeId
              AND Shift = @Shift
              AND IsActive = 1
              AND EffectiveFrom <= @AttendanceDate
              AND (EffectiveTo IS NULL OR EffectiveTo >= @AttendanceDate)
        )
        BEGIN
            RAISERROR('The specified shift is not applicable for this employee on the given date.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        DECLARE @AttendanceId INT;

        -- Step 4: Check whether the attendance record exists
        SELECT @AttendanceId = AttendanceId
        FROM dbo.Attendance WITH (UPDLOCK, HOLDLOCK)
        WHERE EmployeeId = @EmployeeId 
          AND AttendanceDate = @AttendanceDate 
          AND Shift = @Shift;

        IF @AttendanceId IS NOT NULL
        BEGIN
            -- Step 5: If record exists, UPDATE
            UPDATE dbo.Attendance
            SET Status = @Status,
                Remarks = @Remarks,
                UpdatedBy = @UserId,
                UpdatedDate = GETUTCDATE()
            WHERE AttendanceId = @AttendanceId;
        END
        ELSE
        BEGIN
            -- Step 6: If record does not exist, INSERT
            INSERT INTO dbo.Attendance (EmployeeId, AttendanceDate, Shift, Status, Remarks, CreatedBy, CreatedDate)
            VALUES (@EmployeeId, @AttendanceDate, @Shift, @Status, @Remarks, @UserId, GETUTCDATE());
            
            SET @AttendanceId = SCOPE_IDENTITY();
        END

        -- Step 7: Return the final saved attendance record
        SELECT 
            AttendanceId,
            EmployeeId,
            AttendanceDate,
            Shift,
            Status,
            Remarks,
            CreatedBy,
            CreatedDate,
            UpdatedBy,
            UpdatedDate
        FROM dbo.Attendance
        WHERE AttendanceId = @AttendanceId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO


-- 4. Stored Procedure for Getting Attendance with Left Join
CREATE OR ALTER PROCEDURE [dbo].[usp_Attendance_Get]
    @AttendanceDate DATE = NULL,
    @Shift VARCHAR(50) = NULL,
    @EmployeeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @AttendanceDate IS NULL
        SET @AttendanceDate = CAST(GETUTCDATE() AS DATE);

    SELECT 
        A.AttendanceId,
        E.EmployeeId,
        E.EmployeeCode,
        E.Name,
        E.Designation,
        @AttendanceDate AS AttendanceDate,
        ES.Shift,
        A.Status,
        A.Remarks
    FROM dbo.Employees E
    -- Join to get the applicable shift for the employee on the given date
    INNER JOIN (
        SELECT EmployeeId, Shift
        FROM dbo.EmployeeShifts
        WHERE IsActive = 1 
          AND EffectiveFrom <= @AttendanceDate
          AND (EffectiveTo IS NULL OR EffectiveTo >= @AttendanceDate)
    ) ES ON E.EmployeeId = ES.EmployeeId
    -- Left join to attendance to see if it's entered or not
    LEFT JOIN dbo.Attendance A ON E.EmployeeId = A.EmployeeId 
                               AND A.AttendanceDate = @AttendanceDate
                               AND A.Shift = ES.Shift
    WHERE E.IsActive = 1
      AND (@Shift IS NULL OR ES.Shift = @Shift)
      AND (@EmployeeId IS NULL OR E.EmployeeId = @EmployeeId);

END
GO
