-- Seed data for FlightSchool
SET NOCOUNT ON;

INSERT INTO Instructors (FullName, Email, Phone, Rank, HireDate, IsActive)
VALUES
(N'Иванов Пётр Сергеевич', N'ivanov@school.ru', N'+7 900 111-11-11', N'Старший инструктор', GETDATE(), 1),
(N'Сидорова Анна Викторовна', N'sidorova@school.ru', N'+7 900 222-22-22', N'Инструктор', GETDATE(), 1),
(N'Павлов Дмитрий Олегович', N'pavlov@school.ru', N'+7 900 333-33-33', N'Инструктор', GETDATE(), 1);

INSERT INTO Courses (Name, Category, Description, RequiredHours, IsActive)
VALUES
(N'PPL(A)', N'Начальная подготовка', N'Частный пилот самолета', 45, 1),
(N'NVFR', N'Расширение', N'Полеты по приборам ночью', 10, 1),
(N'IR(A)', N'Инструментальный рейтинг', N'Полеты по приборам', 40, 1);

INSERT INTO Aircraft (TailNumber, Model, Type, Year, TotalHours, Status)
VALUES
(N'RA-12345', N'Cessna 172', N'SEP', 2005, 5200, N'Available'),
(N'RA-54321', N'Diamond DA40', N'SEP', 2010, 3100, N'Available'),
(N'RA-77777', N'Piper PA-28', N'SEP', 2002, 6800, N'Maintenance');

INSERT INTO Students (FullName, Email, Phone, Address, BirthDate, MedicalCertificateNo, EnrollmentDate, Notes)
VALUES
(N'Петров Алексей Николаевич', N'petrov@students.ru', N'+7 901 111-11-11', N'Москва', '2000-01-01', N'MED-001', GETDATE(), N''),
(N'Кузнецова Мария Игоревна', N'kuznecova@students.ru', N'+7 901 222-22-22', N'Санкт-Петербург', '1999-05-10', N'MED-002', GETDATE(), N''),
(N'Смирнов Иван Андреевич', N'smirnov@students.ru', N'+7 901 333-33-33', N'Казань', '2001-03-15', N'MED-003', GETDATE(), N''),
(N'Егорова Ольга Сергеевна', N'egorova@students.ru', N'+7 901 444-44-44', N'Новосибирск', '1998-11-20', N'MED-004', GETDATE(), N''),
(N'Васильев Сергей Петрович', N'vasiliev@students.ru', N'+7 901 555-55-55', N'Екатеринбург', '2002-07-07', N'MED-005', GETDATE(), N''),
(N'Морозова Татьяна Никитична', N'morozova@students.ru', N'+7 901 666-66-66', N'Нижний Новгород', '1997-02-02', N'MED-006', GETDATE(), N''),
(N'Григорьев Павел Романович', N'grigoriev@students.ru', N'+7 901 777-77-77', N'Самара', '1996-09-09', N'MED-007', GETDATE(), N'');

-- Enroll some students to courses
INSERT INTO StudentCourses (StudentID, CourseID, EnrolledAt, ProgressHours, Status)
SELECT s.StudentID, c.CourseID, GETDATE(), 0, N'Активен'
FROM Students s CROSS JOIN (SELECT TOP 2 * FROM Courses ORDER BY CourseID) c;

-- Sample lessons
INSERT INTO Lessons (StudentID, InstructorID, CourseID, AircraftID, Date, DurationHours, Topic, Status, Remarks)
SELECT TOP 10 s.StudentID, i.InstructorID, c.CourseID, a.AircraftID, DATEADD(DAY, ROW_NUMBER() OVER (ORDER BY s.StudentID), CAST(GETDATE() AS DATE)), 1.5,
       N'Основы пилотирования', N'Planned', N''
FROM Students s
CROSS JOIN (SELECT TOP 1 * FROM Instructors ORDER BY InstructorID) i
CROSS JOIN (SELECT TOP 1 * FROM Courses ORDER BY CourseID) c
CROSS JOIN (SELECT TOP 1 * FROM Aircraft WHERE Status = N'Available' ORDER BY AircraftID) a;


