-- Single-employee attendance upsert for the Attendance Details date dialog.
-- Safe to run more than once. Does not change the existing bulk Save Attendance flow.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Add EmployeeId + AttendanceDate uniqueness only when no duplicates exist.
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_Attendance_Employee_Date'
      AND object_id = OBJECT_ID(N'dbo.Attendance')
)
AND NOT EXISTS (
    SELECT EmployeeId, AttendanceDate
    FROM dbo.Attendance
    GROUP BY EmployeeId, AttendanceDate
    HAVING COUNT(1) > 1
)
BEGIN
    CREATE UNIQUE INDEX UX_Attendance_Employee_Date
        ON dbo.Attendance (EmployeeId, AttendanceDate);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SaveEmployeeAttendance
    @EmployeeId INT,
    @AttendanceDate DATE,
    @Status NVARCHAR(50),
    @Remarks NVARCHAR(500) = NULL,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @EmployeeId IS NULL OR @EmployeeId <= 0
        BEGIN
            RAISERROR('Invalid EmployeeId.', 16, 1);
        END

        IF @AttendanceDate IS NULL
        BEGIN
            RAISERROR('AttendanceDate is required.', 16, 1);
        END

        IF @Status NOT IN ('Present', 'Leave', 'Half-Day')
        BEGIN
            RAISERROR('Invalid status. Allowed values are Present, Leave, Half-Day.', 16, 1);
        END

        IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE EmployeeId = @EmployeeId AND IsActive = 1)
        BEGIN
            RAISERROR('Employee not found or inactive.', 16, 1);
        END

        DECLARE @Shift NVARCHAR(50);

        SELECT TOP 1 @Shift = es.Shift
        FROM dbo.EmployeeShifts es
        WHERE es.EmployeeId = @EmployeeId
          AND es.IsActive = 1
          AND es.EffectiveFrom <= @AttendanceDate
          AND (es.EffectiveTo IS NULL OR es.EffectiveTo >= @AttendanceDate)
        ORDER BY es.EffectiveFrom DESC;

        IF @Shift IS NULL OR LTRIM(RTRIM(@Shift)) = ''
            SET @Shift = 'Morning';

        DECLARE @AttendanceId INT;

        SELECT TOP 1 @AttendanceId = AttendanceId
        FROM dbo.Attendance WITH (UPDLOCK, HOLDLOCK)
        WHERE EmployeeId = @EmployeeId
          AND AttendanceDate = @AttendanceDate
        ORDER BY AttendanceId DESC;

        IF @AttendanceId IS NOT NULL
        BEGIN
            UPDATE dbo.Attendance
            SET Status = @Status,
                Remarks = @Remarks,
                Shift = @Shift,
                UpdatedBy = @UserId,
                UpdatedDate = GETDATE()
            WHERE AttendanceId = @AttendanceId;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Attendance
            (
                EmployeeId,
                AttendanceDate,
                Shift,
                Status,
                Remarks,
                CreatedBy,
                CreatedDate
            )
            VALUES
            (
                @EmployeeId,
                @AttendanceDate,
                @Shift,
                @Status,
                @Remarks,
                @UserId,
                GETDATE()
            );

            SET @AttendanceId = SCOPE_IDENTITY();
        END

        SELECT
            AttendanceId,
            EmployeeId,
            AttendanceDate,
            Shift,
            Status,
            Remarks
        FROM dbo.Attendance
        WHERE AttendanceId = @AttendanceId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetEmployeeAttendanceDetails
    @EmployeeId INT,
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @EmployeeId IS NULL OR @EmployeeId <= 0
        THROW 50004, 'Invalid EmployeeId.', 1;

    IF @FromDate IS NULL OR @ToDate IS NULL OR @FromDate > @ToDate
        THROW 50004, 'Invalid date range.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE EmployeeId = @EmployeeId)
        THROW 50002, 'Employee not found.', 1;

    ;WITH DateRange AS
    (
        SELECT @FromDate AS AttendanceDate
        UNION ALL
        SELECT DATEADD(DAY, 1, AttendanceDate)
        FROM DateRange
        WHERE AttendanceDate < @ToDate
    ),
    EmployeeDates AS
    (
        SELECT
            e.EmployeeId,
            e.EmployeeCode,
            e.Name AS EmployeeName,
            d.AttendanceDate,
            (
                SELECT TOP 1 es.Shift
                FROM dbo.EmployeeShifts es
                WHERE es.EmployeeId = e.EmployeeId
                  AND es.IsActive = 1
                  AND es.EffectiveFrom <= d.AttendanceDate
                  AND (es.EffectiveTo IS NULL OR es.EffectiveTo >= d.AttendanceDate)
                ORDER BY es.EffectiveFrom DESC
            ) AS Shift
        FROM dbo.Employees e
        CROSS JOIN DateRange d
        WHERE e.EmployeeId = @EmployeeId
    )
    SELECT
        ed.EmployeeId,
        ed.EmployeeCode,
        ed.EmployeeName,
        ISNULL(ed.Shift, '') AS Shift,
        ed.AttendanceDate,
        CASE
            WHEN a.Status = 'Present' THEN 'Present'
            WHEN a.Status = 'Leave' THEN 'Absent'
            WHEN a.Status = 'Half-Day' THEN 'Half Day'
            ELSE 'Not Marked'
        END AS Status,
        a.Remarks
    FROM EmployeeDates ed
    LEFT JOIN dbo.Attendance a
        ON a.EmployeeId = ed.EmployeeId
       AND a.AttendanceDate = ed.AttendanceDate
       AND (ed.Shift IS NULL OR ed.Shift = '' OR a.Shift = ed.Shift)
    ORDER BY ed.AttendanceDate
    OPTION (MAXRECURSION 0);
END
GO
