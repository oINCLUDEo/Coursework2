-- UDF: Total flight hours by student
IF OBJECT_ID(N'dbo.fn_TotalFlightHoursByStudent', N'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_TotalFlightHoursByStudent;
GO
CREATE FUNCTION dbo.fn_TotalFlightHoursByStudent (@studentId INT)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @total DECIMAL(10,2);
    SELECT @total = ISNULL(SUM(CAST(DurationHours AS DECIMAL(10,2))), 0)
    FROM Lessons
    WHERE StudentID = @studentId AND Status IN ('Completed','Approved');
    RETURN @total;
END;
GO

-- UDF: Available aircraft on date (not scheduled that day and with Status='Available')
IF OBJECT_ID(N'dbo.fn_AvailableAircraftOnDate', N'IF') IS NOT NULL
    DROP FUNCTION dbo.fn_AvailableAircraftOnDate;
GO
CREATE FUNCTION dbo.fn_AvailableAircraftOnDate (@date DATE)
RETURNS TABLE
AS
RETURN (
    SELECT a.*
    FROM Aircraft a
    WHERE a.Status = 'Available'
      AND NOT EXISTS (
          SELECT 1 FROM Lessons l
          WHERE l.AircraftID = a.AircraftID
            AND CAST(l.Date AS DATE) = @date
            AND l.Status IN ('Planned','InProgress')
      )
);
GO

-- SP: Get upcoming lessons within next @daysAhead days
IF OBJECT_ID(N'dbo.sp_GetUpcomingLessons', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUpcomingLessons;
GO
CREATE PROCEDURE dbo.sp_GetUpcomingLessons
    @daysAhead INT = 7
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (200)
        l.LessonID,
        l.Date,
        l.DurationHours,
        l.Topic,
        l.Status,
        s.FullName AS StudentName,
        i.FullName AS InstructorName,
        a.TailNumber
    FROM Lessons l
    INNER JOIN Students s ON s.StudentID = l.StudentID
    INNER JOIN Instructors i ON i.InstructorID = l.InstructorID
    LEFT JOIN Aircraft a ON a.AircraftID = l.AircraftID
    WHERE l.Date >= CAST(GETDATE() AS DATE)
      AND l.Date < DATEADD(DAY, @daysAhead, CAST(GETDATE() AS DATE))
    ORDER BY l.Date ASC;
END;
GO


