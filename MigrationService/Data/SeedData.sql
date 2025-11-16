-- Очистка существующих данных
DELETE FROM StatusChanges;
DELETE FROM Documents;
DELETE FROM Applications;
DELETE FROM MigrantLanguages;
DELETE FROM Migrants;
DELETE FROM Officers;
DELETE FROM Languages;
DELETE FROM Countries;
GO

-- Сброс идентификаторов
DBCC CHECKIDENT ('Countries', RESEED, 0);
DBCC CHECKIDENT ('Languages', RESEED, 0);
DBCC CHECKIDENT ('Officers', RESEED, 0);
DBCC CHECKIDENT ('Migrants', RESEED, 0);
DBCC CHECKIDENT ('Applications', RESEED, 0);
GO

-- Заполнение справочников
-- Страны (с явным указанием ID)
SET IDENTITY_INSERT Countries ON;
INSERT INTO Countries (CountryID, CountryName, ISOCode, VisaRequired) VALUES
(1, 'Россия', 'RUS', 0),
(2, 'Казахстан', 'KAZ', 0),
(3, 'Узбекистан', 'UZB', 1),
(4, 'Таджикистан', 'TJK', 1),
(5, 'Киргизия', 'KGZ', 1);
SET IDENTITY_INSERT Countries OFF;
GO

-- Языки (с явным указанием ID)
SET IDENTITY_INSERT Languages ON;
INSERT INTO Languages (LanguageID, LanguageName) VALUES
(1, 'Русский'),
(2, 'Английский'),
(3, 'Казахский'),
(4, 'Узбекский'),
(5, 'Таджикский');
SET IDENTITY_INSERT Languages OFF;
GO

-- Сотрудники (с явным указанием ID)
SET IDENTITY_INSERT Officers ON;
INSERT INTO Officers (OfficerID, FullName, Position, Email, Login, Password) VALUES
(1, 'Иванов Иван Иванович', 'Старший инспектор', 'ivanov@migration.ru', 'ivanov', 'AQAAAAIAAYagAAAAELpX5QJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQ=='),
(2, 'Петрова Анна Сергеевна', 'Инспектор', 'petrova@migration.ru', 'petrova', 'AQAAAAIAAYagAAAAELpX5QJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQ=='),
(3, 'Смирнов Дмитрий Александрович', 'Инспектор', 'smirnov@migration.ru', 'smirnov', 'AQAAAAIAAYagAAAAELpX5QJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQ=='),
(4, 'Козлова Елена Владимировна', 'Старший инспектор', 'kozlova@migration.ru', 'kozlova', 'AQAAAAIAAYagAAAAELpX5QJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQ=='),
(5, 'Морозов Андрей Петрович', 'Инспектор', 'morozov@migration.ru', 'morozov', 'AQAAAAIAAYagAAAAELpX5QJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQ=='),
(6, 'Новикова Ольга Игоревна', 'Инспектор', 'novikova@migration.ru', 'novikova', 'AQAAAAIAAYagAAAAELpX5QJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQ=='),
(7, 'Лебедев Сергей Николаевич', 'Старший инспектор', 'lebedev@migration.ru', 'lebedev', 'AQAAAAIAAYagAAAAELpX5QJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQJ8ZQ==');
SET IDENTITY_INSERT Officers OFF;
GO

-- Мигранты (с явным указанием ID)
SET IDENTITY_INSERT Migrants ON;
INSERT INTO Migrants (MigrantID, FullName, PassportNumber, BirthDate, Address, CountryID, Gender, PhoneNumber) VALUES
(1, 'Ахмедов Алишер Рахимович', 'AB1234567', '1990-05-15', 'ул. Ленина, 15-23, Ташкент', 3, 'Male', '+998 90 123-45-67'),
(2, 'Каримова Дильноза Алиевна', 'CD2345678', '1992-08-23', 'пр. Мирзо-Улугбека, 45-12, Ташкент', 3, 'Female', '+998 91 234-56-78'),
(3, 'Рахимов Фарход Умарович', 'EF3456789', '1988-03-10', 'ул. Рудаки, 78-34, Душанбе', 4, 'Male', '+992 92 345-67-89'),
(4, 'Алиева Мадина Каримовна', 'GH4567890', '1995-11-28', 'ул. Исмоили Сомони, 23-45, Душанбе', 4, 'Female', '+992 93 456-78-90'),
(5, 'Садыков Азамат Бекович', 'IJ5678901', '1991-07-19', 'ул. Манаса, 56-78, Бишкек', 5, 'Male', '+996 94 567-89-01'),
(6, 'Орозбаева Айгуль Садыковна', 'KL6789012', '1993-09-05', 'пр. Чуй, 89-12, Бишкек', 5, 'Female', '+996 95 678-90-12'),
(7, 'Нурланов Дастан Аманжолович', 'MN7890123', '1989-12-14', 'ул. Абая, 34-56, Алматы', 2, 'Male', '+7 96 789-01-23');
SET IDENTITY_INSERT Migrants OFF;
GO

-- Заявления (с явным указанием ID)
SET IDENTITY_INSERT Applications ON;
INSERT INTO Applications (ApplicationID, MigrantID, OfficerID, Type, Status, SubmissionDate, DecisionDate) VALUES
(1, 1, 1, 'Виза', 'Одобрено', '2024-01-15', '2024-02-01'),
(2, 2, 2, 'Вид на жительство', 'На рассмотрении', '2024-02-01', NULL),
(3, 3, 3, 'Рабочая виза', 'Подано', '2024-02-15', NULL),
(4, 4, 4, 'Студенческая виза', 'Требуются дополнительные документы', '2024-03-01', NULL),
(5, 5, 5, 'Воссоединение семьи', 'Одобрено', '2024-01-20', '2024-02-10'),
(6, 6, 6, 'Виза', 'Отклонено', '2024-02-10', '2024-02-25'),
(7, 7, 7, 'Вид на жительство', 'На рассмотрении', '2024-03-10', NULL);
SET IDENTITY_INSERT Applications OFF;
GO

-- Документы (с явным указанием ID)
SET IDENTITY_INSERT Documents ON;
INSERT INTO Documents (DocumentID, ApplicationID, FileName, FileType, UploadedAt) VALUES
(1, 1, 'passport_ahmedov.pdf', 'Паспорт', '2024-01-15 10:00:00'),
(2, 1, 'photo_ahmedov.jpg', 'Фотография', '2024-01-15 10:00:00'),
(3, 2, 'passport_karimova.pdf', 'Паспорт', '2024-02-01 11:00:00'),
(4, 2, 'employment_karimova.pdf', 'Трудовой договор', '2024-02-01 11:00:00'),
(5, 3, 'passport_rahimov.pdf', 'Паспорт', '2024-02-15 09:00:00'),
(6, 4, 'passport_alieva.pdf', 'Паспорт', '2024-03-01 14:00:00'),
(7, 4, 'university_alieva.pdf', 'Справка из университета', '2024-03-01 14:00:00'),
(8, 5, 'passport_sadykov.pdf', 'Паспорт', '2024-01-20 16:00:00'),
(9, 5, 'marriage_sadykov.pdf', 'Свидетельство о браке', '2024-01-20 16:00:00'),
(10, 6, 'passport_orozbaeva.pdf', 'Паспорт', '2024-02-10 13:00:00'),
(11, 7, 'passport_nurlanov.pdf', 'Паспорт', '2024-03-10 10:00:00'),
(12, 7, 'employment_nurlanov.pdf', 'Трудовой договор', '2024-03-10 10:00:00');
SET IDENTITY_INSERT Documents OFF;
GO

-- Изменения статусов (с явным указанием ID)
SET IDENTITY_INSERT StatusChanges ON;
INSERT INTO StatusChanges (StatusChangeID, ApplicationID, Status, ChangedAt, Comment) VALUES
(1, 1, 'На рассмотрении', '2024-01-16 10:00:00', 'Заявление принято на рассмотрение'),
(2, 1, 'Одобрено', '2024-02-01 15:30:00', 'Заявление одобрено'),
(3, 2, 'На рассмотрении', '2024-02-02 11:00:00', 'Заявление принято на рассмотрение'),
(4, 3, 'На рассмотрении', '2024-02-16 09:00:00', 'Заявление принято на рассмотрение'),
(5, 4, 'Требуются дополнительные документы', '2024-03-02 14:00:00', 'Необходимо предоставить справку из университета'),
(6, 5, 'На рассмотрении', '2024-01-21 16:00:00', 'Заявление принято на рассмотрение'),
(7, 5, 'Одобрено', '2024-02-10 11:30:00', 'Заявление одобрено'),
(8, 6, 'На рассмотрении', '2024-02-11 13:00:00', 'Заявление принято на рассмотрение'),
(9, 6, 'Отклонено', '2024-02-25 15:00:00', 'Недостаточно документов'),
(10, 7, 'На рассмотрении', '2024-03-11 10:00:00', 'Заявление принято на рассмотрение');
SET IDENTITY_INSERT StatusChanges OFF;
GO

-- Языки мигрантов
INSERT INTO MigrantLanguages (MigrantID, LanguageID, ProficiencyLevel) VALUES
(1, 1, 'Средний'),
(1, 2, 'Начальный'),
(2, 1, 'Продвинутый'),
(2, 2, 'Средний'),
(3, 1, 'Средний'),
(3, 4, 'Родной'),
(4, 1, 'Начальный'),
(4, 4, 'Родной'),
(5, 1, 'Средний'),
(5, 5, 'Родной'),
(6, 1, 'Начальный'),
(6, 5, 'Родной'),
(7, 1, 'Продвинутый'),
(7, 3, 'Родной');
GO 