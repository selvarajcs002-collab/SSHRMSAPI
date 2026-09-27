-- Attendance period summary and employee details.
-- Safe to run more than once. Does not alter existing tables or Save Attendance SPs.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetAttendanceSummary
    @FromDate DATE,
    @ToDate DATE,
    @Shift NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromDate IS NULL OR @ToDate IS NULL OR @FromDate > @ToDate
        THROW 50004, 'Invalid date range.', 1;

    ;WITH DateRange AS
    (
        SELECT @FromDate AS AttendanceDate
        UNION ALL
        SELECT DATEADD(DAY, 1, AttendanceDate)
        FROM DateRange
        WHERE AttendanceDate < @ToDate
    ),
    ShiftedEmployees AS
    (
        SELECT DISTINCT
            e.EmployeeId,
            e.EmployeeCode,
            e.Name AS EmployeeName,
            es.Shift
        FROM dbo.Employees e
        INNER JOIN dbo.EmployeeShifts es
            ON es.EmployeeId = e.EmployeeId
           AND es.IsActive = 1
           AND es.EffectiveFrom <= @ToDate
           AND (es.EffectiveTo IS NULL OR es.EffectiveTo >= @FromDate)
        WHERE e.IsActive = 1
          AND (@Shift IS NULL OR LTRIM(RTRIM(@Shift)) = '' OR es.Shift = @Shift)
    )
    SELECT
        se.EmployeeId,
        se.EmployeeCode,
        se.EmployeeName,
        se.Shift,
        SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END) AS PresentDays,
        SUM(CASE WHEN a.Status = 'Leave' THEN 1 ELSE 0 END) AS AbsentDays,
        SUM(CASE WHEN a.Status = 'Half-Day' THEN 1 ELSE 0 END) AS HalfDays,
        SUM(CASE WHEN a.AttendanceId IS NULL THEN 1 ELSE 0 END) AS NotMarkedDays
    FROM ShiftedEmployees se
    CROSS JOIN DateRange d
    LEFT JOIN dbo.Attendance a
        ON a.EmployeeId = se.EmployeeId
       AND a.AttendanceDate = d.AttendanceDate
       AND a.Shift = se.Shift
    GROUP BY
        se.EmployeeId,
        se.EmployeeCode,
        se.EmployeeName,
        se.Shift
    ORDER BY se.EmployeeName
    OPTION (MAXRECURSION 0);
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
        END AS Status
    FROM EmployeeDates ed
    LEFT JOIN dbo.Attendance a
        ON a.EmployeeId = ed.EmployeeId
       AND a.AttendanceDate = ed.AttendanceDate
       AND (ed.Shift IS NULL OR ed.Shift = '' OR a.Shift = ed.Shift)
    ORDER BY ed.AttendanceDate
    OPTION (MAXRECURSION 0);
END
GO
