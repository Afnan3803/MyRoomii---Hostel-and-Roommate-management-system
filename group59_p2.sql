-- group 59 phase 2 - FINAL SCRIPT


-- Create

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'myRoomii')
    DROP DATABASE myRoomii;
GO

CREATE DATABASE myRoomii;
GO

USE myRoomii;
GO

-- Optional: faster bulk inserts
ALTER DATABASE myRoomii SET RECOVERY SIMPLE;
GO


--TABLES


CREATE TABLE [User] (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    FullName VARCHAR(255) NOT NULL,
    Phone VARCHAR(20),
    Role VARCHAR(50) DEFAULT 'Student',
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    CONSTRAINT CK_User_Email CHECK (Email LIKE '%@%.%')
);
GO

CREATE TABLE UserPreference (
    UserID INT PRIMARY KEY,
    CleanlinessLevel INT DEFAULT 5 CHECK (CleanlinessLevel BETWEEN 1 AND 10),
    SmokingAllowed BIT DEFAULT 0,
    NoiseLevel INT DEFAULT 5 CHECK (NoiseLevel BETWEEN 1 AND 10),
    SleepSchedule VARCHAR(20) DEFAULT 'Normal', -- Early, Normal, Late
    PetFriendly BIT DEFAULT 0,
    BudgetMin DECIMAL(10,2) DEFAULT 0 CHECK (BudgetMin >= 0),
    BudgetMax DECIMAL(10,2) DEFAULT 50000 CHECK (BudgetMax >= 0),
    CONSTRAINT FK_UserPreference_User FOREIGN KEY (UserID) 
        REFERENCES [User](UserID) ON DELETE CASCADE
);
GO

CREATE TABLE Verification (
    UserID INT PRIMARY KEY,
    Status VARCHAR(20) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Approved', 'Rejected')),
    Method VARCHAR(50), -- Email, Phone, ID
    VerifiedAt DATETIME2,
    CONSTRAINT FK_Verification_User FOREIGN KEY (UserID) 
        REFERENCES [User](UserID) ON DELETE CASCADE
);
GO

CREATE TABLE Landlord (
    UserID INT PRIMARY KEY,
    CompanyName VARCHAR(255),
    Phone VARCHAR(20),
    OverallRating DECIMAL(3,2) DEFAULT 0.0 CHECK (OverallRating BETWEEN 0 AND 5),
    TotalReviews INT DEFAULT 0,
    CONSTRAINT FK_Landlord_User FOREIGN KEY (UserID) 
        REFERENCES [User](UserID) ON DELETE CASCADE
);
GO

CREATE TABLE RoomListing (
    ListingID INT IDENTITY(1,1) PRIMARY KEY,
    LandlordUserID INT NOT NULL,
    Title VARCHAR(255) NOT NULL,
    Description VARCHAR(MAX),
    City VARCHAR(100),
    HostelName VARCHAR(255),
    Rent DECIMAL(10,2) NOT NULL CHECK (Rent >= 0),
    AvailableFrom DATE,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_RoomListing_Landlord FOREIGN KEY (LandlordUserID) 
        REFERENCES Landlord(UserID) ON DELETE NO ACTION
);
GO

CREATE TABLE RoomAmenities (
    ListingID INT PRIMARY KEY,
    HasAC BIT DEFAULT 0,
    HasHeating BIT DEFAULT 0,
    HasPrivateWashroom BIT DEFAULT 0,
    AllowsPets BIT DEFAULT 0,
    HasWifi BIT DEFAULT 1,
    HasFurniture BIT DEFAULT 0,
    HasParking BIT DEFAULT 0,
    CONSTRAINT FK_RoomAmenities_RoomListing FOREIGN KEY (ListingID) 
        REFERENCES RoomListing(ListingID) ON DELETE CASCADE
);
GO

CREATE TABLE RoommateMatch (
    MatchID INT IDENTITY(1,1) PRIMARY KEY,
    UserID1 INT NOT NULL,
    UserID2 INT NOT NULL,
    ListingID INT NOT NULL,
    CompatibilityScore INT DEFAULT 0 CHECK (CompatibilityScore BETWEEN 0 AND 100),
    Status VARCHAR(20) DEFAULT 'Suggested' CHECK (Status IN ('Suggested', 'Accepted', 'Rejected')),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_RoommateMatch_User1 FOREIGN KEY (UserID1) 
        REFERENCES [User](UserID) ON DELETE NO ACTION,
    CONSTRAINT FK_RoommateMatch_User2 FOREIGN KEY (UserID2) 
        REFERENCES [User](UserID) ON DELETE NO ACTION,
    CONSTRAINT FK_RoommateMatch_RoomListing FOREIGN KEY (ListingID) 
        REFERENCES RoomListing(ListingID) ON DELETE NO ACTION,
    CONSTRAINT CK_RoommateMatch_DifferentUsers CHECK (UserID1 <> UserID2)
);
GO

CREATE TABLE Chat (
    ChatID INT IDENTITY(1,1) PRIMARY KEY,
    User1ID INT NOT NULL,
    User2ID INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    LastMessageAt DATETIME2,
    IsBlocked BIT DEFAULT 0,
    CONSTRAINT FK_Chat_User1 FOREIGN KEY (User1ID) 
        REFERENCES [User](UserID) ON DELETE NO ACTION,
    CONSTRAINT FK_Chat_User2 FOREIGN KEY (User2ID) 
        REFERENCES [User](UserID) ON DELETE NO ACTION,
    CONSTRAINT CK_Chat_DifferentUsers CHECK (User1ID <> User2ID)
);
GO

CREATE TABLE Message (
    MessageID BIGINT IDENTITY(1,1) PRIMARY KEY,
    ChatID INT NOT NULL,
    SenderID INT NOT NULL,
    Text VARCHAR(MAX) NOT NULL,
    SentAt DATETIME2 DEFAULT GETDATE(),
    IsRead BIT DEFAULT 0,
    CONSTRAINT FK_Message_Chat FOREIGN KEY (ChatID) 
        REFERENCES Chat(ChatID) ON DELETE CASCADE,
    CONSTRAINT FK_Message_Sender FOREIGN KEY (SenderID) 
        REFERENCES [User](UserID) ON DELETE NO ACTION
);
GO

CREATE TABLE Review (
    ReviewID INT IDENTITY(1,1) PRIMARY KEY,
    AuthorUserID INT NOT NULL,
    LandlordUserID INT NOT NULL,
    ListingID INT,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment VARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Review_Author FOREIGN KEY (AuthorUserID) 
        REFERENCES [User](UserID) ON DELETE NO ACTION,
    CONSTRAINT FK_Review_Landlord FOREIGN KEY (LandlordUserID) 
        REFERENCES Landlord(UserID) ON DELETE NO ACTION,
    CONSTRAINT FK_Review_Listing FOREIGN KEY (ListingID) 
        REFERENCES RoomListing(ListingID) ON DELETE SET NULL
);
GO


-- small seed data


INSERT INTO [User] (Email, FullName, Phone, Role) VALUES
('ali.ahmed@lums.edu.pk', 'Ali Ahmed', '03001234567', 'Student'),
('sara.khan@lums.edu.pk', 'Sara Khan', '03001234568', 'Student'),
('ahmad.hassan@lums.edu.pk', 'Ahmad Hassan', '03001234569', 'Student'),
('fatima.ali@lums.edu.pk', 'Fatima Ali', '03001234570', 'Student'),
('muhammad.raza@lums.edu.pk', 'Muhammad Raza', '03001234571', 'Student'),
('zainab.malik@lums.edu.pk', 'Zainab Malik', '03001234572', 'Student'),
('hassan.qureshi@lums.edu.pk', 'Hassan Qureshi', '03001234573', 'Landlord'),
('amna.sheikh@lums.edu.pk', 'Amna Sheikh', '03001234574', 'Landlord'),
('bilal.ahmed@lums.edu.pk', 'Bilal Ahmed', '03001234575', 'Student'),
('hira.khan@lums.edu.pk', 'Hira Khan', '03001234576', 'Student');

INSERT INTO UserPreference (UserID, CleanlinessLevel, SmokingAllowed, NoiseLevel, SleepSchedule, PetFriendly, BudgetMin, BudgetMax) VALUES
(1, 8, 0, 3, 'Early', 0, 10000, 25000),
(2, 7, 0, 5, 'Normal', 1, 15000, 30000),
(3, 6, 1, 6, 'Late', 0, 12000, 28000),
(4, 9, 0, 2, 'Early', 0, 18000, 35000),
(5, 5, 0, 7, 'Normal', 1, 10000, 22000),
(6, 8, 0, 4, 'Normal', 0, 14000, 30000),
(7, 7, 0, 5, 'Normal', 0, 0, 0),
(8, 6, 0, 5, 'Normal', 0, 0, 0),
(9, 7, 0, 5, 'Normal', 0, 11000, 26000),
(10, 8, 0, 3, 'Early', 0, 16000, 32000);

INSERT INTO Verification (UserID, Status, Method, VerifiedAt) VALUES
(1, 'Approved', 'Email', GETDATE()),
(2, 'Approved', 'Phone', GETDATE()),
(3, 'Pending', 'Email', NULL),
(4, 'Approved', 'ID', GETDATE()),
(5, 'Rejected', 'Email', NULL),
(7, 'Approved', 'ID', GETDATE()),
(8, 'Approved', 'ID', GETDATE());

INSERT INTO Landlord (UserID, CompanyName, Phone, OverallRating, TotalReviews) VALUES
(7, 'Qureshi Properties', '03001234573', 4.5, 10),
(8, 'Sheikh Rentals', '03001234574', 4.2, 8);

INSERT INTO RoomListing (LandlordUserID, Title, Description, City, HostelName, Rent, AvailableFrom, IsActive) VALUES
(7, 'Cozy Single Room in LUMS Hostel', 'Spacious single room with attached bathroom', 'Lahore', 'Hostel A', 20000, '2024-01-15', 1),
(7, 'Shared Room Near Campus', 'Shared room for 2 students, fully furnished', 'Lahore', 'Hostel B', 15000, '2024-02-01', 1),
(8, 'Premium Single Room', 'Luxury single room with AC and heating', 'Lahore', 'Hostel C', 30000, '2024-01-20', 1),
(8, 'Budget-Friendly Shared Room', 'Affordable shared accommodation', 'Lahore', 'Hostel D', 12000, '2024-02-10', 1);

INSERT INTO RoomAmenities (ListingID, HasAC, HasHeating, HasPrivateWashroom, AllowsPets, HasWifi, HasFurniture, HasParking) VALUES
(1, 1, 1, 1, 0, 1, 1, 0),
(2, 1, 0, 0, 0, 1, 1, 1),
(3, 1, 1, 1, 1, 1, 1, 1),
(4, 0, 0, 0, 0, 1, 0, 0);

INSERT INTO RoommateMatch (UserID1, UserID2, ListingID, CompatibilityScore, Status) VALUES
(1, 2, 2, 85, 'Suggested'),
(3, 4, 2, 72, 'Accepted'),
(5, 6, 4, 68, 'Suggested'),
(9, 10, 1, 90, 'Accepted');

INSERT INTO Chat (User1ID, User2ID, CreatedAt, LastMessageAt) VALUES
(1, 2, '2024-01-10', '2024-01-12'),
(3, 4, '2024-01-11', '2024-01-13'),
(5, 6, '2024-01-12', '2024-01-14'),
(1, 7, '2024-01-10', '2024-01-11'),
(2, 8, '2024-01-11', '2024-01-12');

INSERT INTO Message (ChatID, SenderID, Text, SentAt, IsRead) VALUES
(1, 1, 'Hi, are you still looking for a roommate?', '2024-01-10 10:00:00', 1),
(1, 2, 'Yes! I am interested in the shared room.', '2024-01-10 10:15:00', 1),
(1, 1, 'Great! When can we meet?', '2024-01-10 11:00:00', 1),
(2, 3, 'Hello, I saw your profile and we seem compatible.', '2024-01-11 09:00:00', 1),
(2, 4, 'Yes, lets discuss the details.', '2024-01-11 09:30:00', 1),
(3, 5, 'Hi there!', '2024-01-12 14:00:00', 0),
(4, 1, 'Is the room still available?', '2024-01-10 15:00:00', 1),
(4, 7, 'Yes, it is available. Would you like to schedule a viewing?', '2024-01-10 15:30:00', 1);

INSERT INTO Review (AuthorUserID, LandlordUserID, ListingID, Rating, Comment, CreatedAt) VALUES
(1, 7, 1, 5, 'Excellent landlord, very responsive and helpful.', '2024-01-20'),
(2, 7, 2, 4, 'Good experience overall, room was as described.', '2024-01-22'),
(3, 8, 3, 5, 'Amazing place, highly recommended!', '2024-01-25'),
(4, 8, 4, 3, 'Room was okay but could be better maintained.', '2024-01-26');

GO

-- Random data


PRINT '========================================';
PRINT 'RANDOM BULK DATA INSERTION (SIMPLIFIED)';
PRINT '========================================';

SET NOCOUNT ON;

-- Numbers table

IF OBJECT_ID('tempdb..#Numbers') IS NOT NULL DROP TABLE #Numbers;

SELECT TOP (400000)
    ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
INTO #Numbers
FROM sys.all_objects a
CROSS JOIN sys.all_objects b;

PRINT 'Numbers table created';


-- Extra Users 

DECLARE @UserExtra INT = 60000;
PRINT 'Inserting ' + CAST(@UserExtra AS VARCHAR(10)) + ' extra users...';

INSERT INTO [User] (Email, FullName, Phone, Role, CreatedAt)
SELECT TOP (@UserExtra)
    'user' + CAST(n AS VARCHAR(10)) + '@lums.edu.pk',
    'User ' + CAST(n AS VARCHAR(10)),
    '0300' + RIGHT('0000000' + CAST(n AS VARCHAR(10)), 7),
    CASE WHEN n % 10 = 0 THEN 'Landlord' ELSE 'Student' END,
    DATEADD(DAY, -(ABS(CHECKSUM(n)) % 365), GETDATE())
FROM #Numbers
ORDER BY n;

PRINT 'Extra users inserted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));


--User Preferences for all users after seed

PRINT 'Inserting user preferences...';

INSERT INTO UserPreference
(UserID, CleanlinessLevel, SmokingAllowed, NoiseLevel, SleepSchedule, PetFriendly, BudgetMin, BudgetMax)
SELECT
    u.UserID,
    (ABS(CHECKSUM(u.UserID * 11)) % 10) + 1,
    CASE WHEN ABS(CHECKSUM(u.UserID * 13)) % 3 = 0 THEN 1 ELSE 0 END,
    (ABS(CHECKSUM(u.UserID * 17)) % 10) + 1,
    CASE (ABS(CHECKSUM(u.UserID * 19)) % 3)
        WHEN 0 THEN 'Early'
        WHEN 1 THEN 'Normal'
        ELSE 'Late'
    END,
    CASE WHEN ABS(CHECKSUM(u.UserID * 23)) % 4 = 0 THEN 1 ELSE 0 END,
    (ABS(CHECKSUM(u.UserID * 29)) % 10000) + 5000,
    (ABS(CHECKSUM(u.UserID * 31)) % 30000) + 20000
FROM [User] u
LEFT JOIN UserPreference up ON up.UserID = u.UserID
WHERE u.UserID > 10
  AND up.UserID IS NULL;

PRINT 'User preferences inserted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

-- Landlords for all landlord users

PRINT 'Inserting landlords...';

INSERT INTO Landlord (UserID, CompanyName, Phone, OverallRating, TotalReviews)
SELECT
    u.UserID,
    'Company ' + CAST(u.UserID AS VARCHAR(10)),
    u.Phone,
    0.0,
    0
FROM [User] u
LEFT JOIN Landlord l ON l.UserID = u.UserID
WHERE u.Role = 'Landlord'
  AND l.UserID IS NULL;

PRINT 'Landlords inserted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));


-- Room Listings 

PRINT 'Inserting room listings...';

IF OBJECT_ID('tempdb..#Landlords') IS NOT NULL DROP TABLE #Landlords;

SELECT 
    ROW_NUMBER() OVER (ORDER BY UserID) AS rn,
    UserID
INTO #Landlords
FROM Landlord;

DECLARE @LandlordCount INT = (SELECT COUNT(*) FROM #Landlords);
DECLARE @ListingTarget INT = 150000;

INSERT INTO RoomListing
    (LandlordUserID, Title, Description, City, HostelName, Rent, AvailableFrom, IsActive, CreatedAt)
SELECT TOP (@ListingTarget)
    L.UserID,
    'Listing ' + CAST(N.n AS VARCHAR(10)),
    'Auto-generated listing ' + CAST(N.n AS VARCHAR(10)),
    CASE (ABS(CHECKSUM(N.n * 5)) % 5)
        WHEN 0 THEN 'Lahore'
        WHEN 1 THEN 'Karachi'
        WHEN 2 THEN 'Islamabad'
        WHEN 3 THEN 'Rawalpindi'
        ELSE 'Multan'
    END,
    'Hostel ' + CAST((ABS(CHECKSUM(N.n * 7)) % 20) + 1 AS VARCHAR(10)),
    (ABS(CHECKSUM(N.n * 11)) % 40000) + 10000,
    DATEADD(DAY, ABS(CHECKSUM(N.n * 13)) % 180, GETDATE()),
    CASE WHEN ABS(CHECKSUM(N.n * 17)) % 10 < 8 THEN 1 ELSE 0 END,
    DATEADD(DAY, -(ABS(CHECKSUM(N.n * 19)) % 365), GETDATE())
FROM #Numbers N
JOIN #Landlords L
  ON L.rn = ((N.n - 1) % @LandlordCount) + 1
ORDER BY N.n;

PRINT 'Room listings inserted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));


-- Room Amenities for all listings without one

PRINT 'Inserting room amenities...';

INSERT INTO RoomAmenities
(ListingID, HasAC, HasHeating, HasPrivateWashroom, AllowsPets, HasWifi, HasFurniture, HasParking)
SELECT 
    rl.ListingID,
    CASE WHEN ABS(CHECKSUM(rl.ListingID * 3)) % 3 < 2 THEN 1 ELSE 0 END,
    CASE WHEN ABS(CHECKSUM(rl.ListingID * 5)) % 2 = 0 THEN 1 ELSE 0 END,
    CASE WHEN ABS(CHECKSUM(rl.ListingID * 7)) % 2 = 0 THEN 1 ELSE 0 END,
    CASE WHEN ABS(CHECKSUM(rl.ListingID * 11)) % 4 = 0 THEN 1 ELSE 0 END,
    1,
    CASE WHEN ABS(CHECKSUM(rl.ListingID * 13)) % 3 < 2 THEN 1 ELSE 0 END,
    CASE WHEN ABS(CHECKSUM(rl.ListingID * 17)) % 3 = 0 THEN 1 ELSE 0 END
FROM RoomListing rl
LEFT JOIN RoomAmenities ra ON ra.ListingID = rl.ListingID
WHERE ra.ListingID IS NULL;

PRINT 'Room amenities inserted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));


-- Roommate Matches

PRINT 'Inserting roommate matches...';

IF OBJECT_ID('tempdb..#Students') IS NOT NULL DROP TABLE #Students;
IF OBJECT_ID('tempdb..#Listings') IS NOT NULL DROP TABLE #Listings;

SELECT 
    ROW_NUMBER() OVER (ORDER BY UserID) AS rn,
    UserID
INTO #Students
FROM [User]
WHERE Role = 'Student';

SELECT 
    ROW_NUMBER() OVER (ORDER BY ListingID) AS rn,
    ListingID
INTO #Listings
FROM RoomListing;

DECLARE @StudentCount INT = (SELECT COUNT(*) FROM #Students);
DECLARE @ListingCount INT = (SELECT COUNT(*) FROM #Listings);
DECLARE @MatchTarget INT = 200000;

;WITH NumMatch AS (
    SELECT TOP (@MatchTarget)
        ROW_NUMBER() OVER (ORDER BY n) AS n
    FROM #Numbers
)
INSERT INTO RoommateMatch
(UserID1, UserID2, ListingID, CompatibilityScore, Status, CreatedAt)
SELECT
    s1.UserID,
    s2.UserID,
    l.ListingID,
    (ABS(CHECKSUM(N.n * 23)) % 41) + 60,
    CASE (ABS(CHECKSUM(N.n * 29)) % 3)
        WHEN 0 THEN 'Suggested'
        WHEN 1 THEN 'Accepted'
        ELSE 'Rejected'
    END,
    DATEADD(DAY, -(ABS(CHECKSUM(N.n * 31)) % 365), GETDATE())
FROM NumMatch N
JOIN #Students s1
    ON s1.rn = ((N.n - 1) % @StudentCount) + 1
JOIN #Students s2
    ON s2.rn = ((N.n) % @StudentCount) + 1   -- always different when @StudentCount > 1
JOIN #Listings l
    ON l.rn = ((N.n * 11 - 1) % @ListingCount) + 1
WHERE s1.UserID <> s2.UserID;

PRINT 'Roommate matches inserted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));


-- Chats 

PRINT 'Inserting chats...';

DECLARE @ChatTarget INT = 30000;

;WITH NumChat AS (
    SELECT TOP (@ChatTarget)
        ROW_NUMBER() OVER (ORDER BY n) AS n
    FROM #Numbers
)
INSERT INTO Chat
(User1ID, User2ID, CreatedAt, LastMessageAt, IsBlocked)
SELECT
    s1.UserID,
    s2.UserID,
    DATEADD(DAY, -(ABS(CHECKSUM(N.n * 37)) % 365), GETDATE()),
    DATEADD(DAY, -(ABS(CHECKSUM(N.n * 41)) % 30), GETDATE()),
    CASE WHEN ABS(CHECKSUM(N.n * 43)) % 20 = 0 THEN 1 ELSE 0 END
FROM NumChat N
JOIN #Students s1
    ON s1.rn = ((N.n - 1) % @StudentCount) + 1
JOIN #Students s2
    ON s2.rn = ((N.n) % @StudentCount) + 1
WHERE s1.UserID <> s2.UserID;

PRINT 'Chats inserted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));


-- Messages 

PRINT 'Inserting messages...';

IF OBJECT_ID('tempdb..#Chats') IS NOT NULL DROP TABLE #Chats;

SELECT 
    ROW_NUMBER() OVER (ORDER BY ChatID) AS rn,
    ChatID,
    User1ID,
    User2ID
INTO #Chats
FROM Chat;

DECLARE @ChatCount INT = (SELECT COUNT(*) FROM #Chats);
DECLARE @MsgTarget INT = 400000;

;WITH NumMsg AS (
    SELECT TOP (@MsgTarget)
        ROW_NUMBER() OVER (ORDER BY n) AS n
    FROM #Numbers
)
INSERT INTO Message
(ChatID, SenderID, Text, SentAt, IsRead)
SELECT
    c.ChatID,
    CASE WHEN N.n % 2 = 0 THEN c.User1ID ELSE c.User2ID END,
    'Message ' + CAST(N.n AS VARCHAR(20)),
    DATEADD(MINUTE, -(ABS(CHECKSUM(N.n * 47)) % 525600), GETDATE()),
    CASE WHEN ABS(CHECKSUM(N.n * 53)) % 3 < 2 THEN 1 ELSE 0 END
FROM NumMsg N
JOIN #Chats c
    ON c.rn = ((N.n - 1) % @ChatCount) + 1;

PRINT 'Messages inserted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));


-- Reviews 

PRINT 'Inserting reviews...';

IF OBJECT_ID('tempdb..#LandlordsReview') IS NOT NULL DROP TABLE #LandlordsReview;

SELECT 
    ROW_NUMBER() OVER (ORDER BY UserID) AS rn,
    UserID
INTO #LandlordsReview
FROM Landlord;

DECLARE @ReviewTarget INT = 80000;
DECLARE @LandlordCountReview INT = (SELECT COUNT(*) FROM #LandlordsReview);

;WITH NumRev AS (
    SELECT TOP (@ReviewTarget)
        ROW_NUMBER() OVER (ORDER BY n) AS n
    FROM #Numbers
)
INSERT INTO Review
(AuthorUserID, LandlordUserID, ListingID, Rating, Comment, CreatedAt)
SELECT
    s.UserID,
    l.UserID,
    li.ListingID,
    (ABS(CHECKSUM(N.n * 59)) % 5) + 1,
    'Auto review ' + CAST(N.n AS VARCHAR(10)),
    DATEADD(DAY, -(ABS(CHECKSUM(N.n * 61)) % 365), GETDATE())
FROM NumRev N
JOIN #Students s
    ON s.rn = ((N.n - 1) % @StudentCount) + 1
JOIN #LandlordsReview l
    ON l.rn = ((N.n * 19 - 1) % @LandlordCountReview) + 1
JOIN #Listings li
    ON li.rn = ((N.n * 23 - 1) % @ListingCount) + 1;

PRINT 'Reviews inserted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

-- Clean up temp tables

DROP TABLE IF EXISTS #Numbers;
DROP TABLE IF EXISTS #Landlords;
DROP TABLE IF EXISTS #Students;
DROP TABLE IF EXISTS #Listings;
DROP TABLE IF EXISTS #Chats;
DROP TABLE IF EXISTS #LandlordsReview;

PRINT '========================================';
PRINT 'RANDOM BULK DATA INSERTION COMPLETE!';
PRINT '========================================';

GO

-- stored procedure


CREATE PROCEDURE spGetTopMatchesForUser
    @UserID INT,
    @TopN INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@TopN)
        rm.MatchID,
        rm.CompatibilityScore,
        rm.Status,
        u2.FullName AS MatchedUserName,
        u2.Email AS MatchedUserEmail,
        rl.Title AS ListingTitle,
        rl.Rent,
        rl.City
    FROM RoommateMatch rm
    INNER JOIN [User] u2 ON (rm.UserID1 = @UserID AND rm.UserID2 = u2.UserID) 
                         OR (rm.UserID2 = @UserID AND rm.UserID1 = u2.UserID)
    INNER JOIN RoomListing rl ON rm.ListingID = rl.ListingID
    WHERE (rm.UserID1 = @UserID OR rm.UserID2 = @UserID)
    ORDER BY rm.CompatibilityScore DESC, rm.CreatedAt DESC;
END;
GO

CREATE PROCEDURE spSendMessage
    @ChatID INT,
    @SenderID INT,
    @Text VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (
        SELECT 1 FROM Chat 
        WHERE ChatID = @ChatID 
        AND (User1ID = @SenderID OR User2ID = @SenderID)
    )
    BEGIN
        RAISERROR('Chat does not exist or sender is not part of this chat.', 16, 1);
        RETURN;
    END;
    
    INSERT INTO Message (ChatID, SenderID, Text, SentAt, IsRead)
    VALUES (@ChatID, @SenderID, @Text, GETDATE(), 0);
    
    SELECT 
        MessageID,
        ChatID,
        SenderID,
        Text,
        SentAt,
        IsRead
    FROM Message
    WHERE MessageID = SCOPE_IDENTITY();
END;
GO

CREATE PROCEDURE spCreateListingWithAmenities
    @LandlordUserID INT,
    @Title VARCHAR(255),
    @Description VARCHAR(MAX),
    @City VARCHAR(100),
    @HostelName VARCHAR(255),
    @Rent DECIMAL(10,2),
    @AvailableFrom DATE,
    @HasAC BIT = 0,
    @HasHeating BIT = 0,
    @HasPrivateWashroom BIT = 0,
    @AllowsPets BIT = 0,
    @HasWifi BIT = 1,
    @HasFurniture BIT = 0,
    @HasParking BIT = 0,
    @ListingID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Landlord WHERE UserID = @LandlordUserID)
        BEGIN
            RAISERROR('Invalid landlord user ID.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END;
 
        INSERT INTO RoomListing (LandlordUserID, Title, Description, City, HostelName, Rent, AvailableFrom, IsActive)
        VALUES (@LandlordUserID, @Title, @Description, @City, @HostelName, @Rent, @AvailableFrom, 1);
        
        SET @ListingID = SCOPE_IDENTITY();

        INSERT INTO RoomAmenities (ListingID, HasAC, HasHeating, HasPrivateWashroom, AllowsPets, HasWifi, HasFurniture, HasParking)
        VALUES (@ListingID, @HasAC, @HasHeating, @HasPrivateWashroom, @AllowsPets, @HasWifi, @HasFurniture, @HasParking);
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

-- func


CREATE FUNCTION fnCompatibilityScore(@UserID1 INT, @UserID2 INT)
RETURNS INT
AS
BEGIN
    DECLARE @Score INT = 50; -- Base
    
    DECLARE @Pref1 TABLE (
        CleanlinessLevel INT,
        SmokingAllowed BIT,
        NoiseLevel INT,
        SleepSchedule VARCHAR(20),
        PetFriendly BIT
    );
    
    DECLARE @Pref2 TABLE (
        CleanlinessLevel INT,
        SmokingAllowed BIT,
        NoiseLevel INT,
        SleepSchedule VARCHAR(20),
        PetFriendly BIT
    );
    
    INSERT INTO @Pref1
    SELECT CleanlinessLevel, SmokingAllowed, NoiseLevel, SleepSchedule, PetFriendly
    FROM UserPreference
    WHERE UserID = @UserID1;
    
    INSERT INTO @Pref2
    SELECT CleanlinessLevel, SmokingAllowed, NoiseLevel, SleepSchedule, PetFriendly
    FROM UserPreference
    WHERE UserID = @UserID2;
    
    SELECT @Score = @Score + 
        CASE WHEN ABS(p1.CleanlinessLevel - p2.CleanlinessLevel) <= 2 THEN 10 ELSE -5 END +
        CASE WHEN p1.SmokingAllowed = p2.SmokingAllowed THEN 10 ELSE -10 END +
        CASE WHEN ABS(p1.NoiseLevel - p2.NoiseLevel) <= 2 THEN 10 ELSE -5 END +
        CASE WHEN p1.SleepSchedule = p2.SleepSchedule THEN 10 ELSE 0 END +
        CASE WHEN p1.PetFriendly = p2.PetFriendly THEN 10 ELSE -5 END
    FROM @Pref1 p1
    CROSS JOIN @Pref2 p2;
    
    IF @Score > 100 SET @Score = 100;
    IF @Score < 0 SET @Score = 0;
    
    RETURN @Score;
END;
GO

CREATE FUNCTION fnAverageRatingForLandlord(@LandlordUserID INT)
RETURNS DECIMAL(3,2)
AS
BEGIN
    DECLARE @AvgRating DECIMAL(3,2);
    
    SELECT @AvgRating = CAST(AVG(CAST(Rating AS DECIMAL(3,2))) AS DECIMAL(3,2))
    FROM Review
    WHERE LandlordUserID = @LandlordUserID;
    
    IF @AvgRating IS NULL
        SET @AvgRating = 0.0;
    
    RETURN @AvgRating;
END;
GO

-- triggers


CREATE TRIGGER trgReview_UpdateLandlordRating
ON Review
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE l
    SET OverallRating = (
            SELECT CAST(AVG(CAST(r.Rating AS DECIMAL(3,2))) AS DECIMAL(3,2))
            FROM Review r
            WHERE r.LandlordUserID = l.UserID
        ),
        TotalReviews = (
            SELECT COUNT(*)
            FROM Review r
            WHERE r.LandlordUserID = l.UserID
        )
    FROM Landlord l
    INNER JOIN inserted i ON l.UserID = i.LandlordUserID;
END;
GO

CREATE TRIGGER trgMessage_UpdateChatLastMessage
ON Message
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE c
    SET LastMessageAt = i.SentAt
    FROM Chat c
    INNER JOIN inserted i ON c.ChatID = i.ChatID;
END;
GO

CREATE TRIGGER trgUser_SoftDelete
ON [User]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [User]
    SET IsActive = 0
    WHERE UserID IN (SELECT UserID FROM deleted);
END;
GO

-- views


CREATE VIEW vwActiveListingsWithLandlord
AS
SELECT 
    rl.ListingID,
    rl.Title,
    rl.Description,
    rl.City,
    rl.HostelName,
    rl.Rent,
    rl.AvailableFrom,
    rl.CreatedAt,
    l.UserID AS LandlordUserID,
    u.FullName AS LandlordName,
    u.Email AS LandlordEmail,
    l.CompanyName,
    l.OverallRating AS LandlordRating,
    ra.HasAC,
    ra.HasHeating,
    ra.HasPrivateWashroom,
    ra.AllowsPets,
    ra.HasWifi,
    ra.HasFurniture,
    ra.HasParking
FROM RoomListing rl
INNER JOIN Landlord l ON rl.LandlordUserID = l.UserID
INNER JOIN [User] u ON l.UserID = u.UserID
LEFT JOIN RoomAmenities ra ON rl.ListingID = ra.ListingID
WHERE rl.IsActive = 1;
GO

CREATE VIEW vwLandlordRatings
AS
SELECT 
    l.UserID AS LandlordUserID,
    u.FullName AS LandlordName,
    l.CompanyName,
    l.OverallRating AS AverageRating,
    l.TotalReviews AS ReviewCount,
    COALESCE(MIN(r.Rating), 0) AS MinRating,
    COALESCE(MAX(r.Rating), 0) AS MaxRating
FROM Landlord l
INNER JOIN [User] u ON l.UserID = u.UserID
LEFT JOIN Review r ON l.UserID = r.LandlordUserID
GROUP BY l.UserID, u.FullName, l.CompanyName, l.OverallRating, l.TotalReviews;
GO

CREATE VIEW vwUserMatchSummary
AS
SELECT 
    u.UserID,
    u.FullName,
    u.Email,
    COUNT(DISTINCT rm1.MatchID) + COUNT(DISTINCT rm2.MatchID) AS TotalMatches,
    COUNT(DISTINCT CASE WHEN rm1.Status = 'Accepted' OR rm2.Status = 'Accepted' THEN rm1.MatchID END) + 
    COUNT(DISTINCT CASE WHEN rm2.Status = 'Accepted' THEN rm2.MatchID END) AS AcceptedMatches,
    MAX(COALESCE(rm1.CompatibilityScore, rm2.CompatibilityScore)) AS BestCompatibilityScore
FROM [User] u
LEFT JOIN RoommateMatch rm1 ON u.UserID = rm1.UserID1
LEFT JOIN RoommateMatch rm2 ON u.UserID = rm2.UserID2
WHERE u.Role = 'Student'
GROUP BY u.UserID, u.FullName, u.Email;
GO


-- index


CREATE UNIQUE NONCLUSTERED INDEX IX_User_Email
ON [User](Email);
GO

CREATE NONCLUSTERED INDEX IX_Message_ChatID_SentAt
ON Message(ChatID, SentAt DESC);
GO

CREATE NONCLUSTERED INDEX IX_RoommateMatch_UserID1_CompatibilityScore
ON RoommateMatch(UserID1, CompatibilityScore DESC);
GO

CREATE NONCLUSTERED INDEX IX_RoommateMatch_UserID2_CompatibilityScore
ON RoommateMatch(UserID2, CompatibilityScore DESC);
GO

CREATE NONCLUSTERED INDEX IX_Review_LandlordUserID_Rating
ON Review(LandlordUserID, Rating);
GO


-- table partitioning


CREATE PARTITION FUNCTION pfMessageByYear (DATETIME2)
AS RANGE RIGHT FOR VALUES 
('2022-01-01', '2023-01-01', '2024-01-01', '2025-01-01');
GO

CREATE PARTITION SCHEME psMessageByYear
AS PARTITION pfMessageByYear
ALL TO ([PRIMARY]);
GO

CREATE NONCLUSTERED INDEX IX_Message_SentAt_Partitioned
ON Message(SentAt)
ON psMessageByYear(SentAt);
GO

CREATE PARTITION FUNCTION pfListingByYear (DATETIME2)
AS RANGE RIGHT FOR VALUES 
('2022-01-01', '2023-01-01', '2024-01-01', '2025-01-01');
GO

CREATE PARTITION SCHEME psListingByYear
AS PARTITION pfListingByYear
ALL TO ([PRIMARY]);
GO

CREATE NONCLUSTERED INDEX IX_RoomListing_CreatedAt_Partitioned
ON RoomListing(CreatedAt)
ON psListingByYear(CreatedAt);
GO


-- demo queries


SET NOCOUNT ON;
PRINT 'Running demo queries...';

PRINT 'Top matches for user (using stored procedure)';
EXEC spGetTopMatchesForUser @UserID = 1, @TopN = 5;
GO

PRINT 'Active listings with landlord information (using view)';
SELECT TOP 10 * FROM vwActiveListingsWithLandlord ORDER BY Rent DESC;
GO

PRINT 'Landlord ratings summary (using view)';
SELECT TOP 10 * FROM vwLandlordRatings ORDER BY AverageRating DESC;
GO

PRINT 'Top matches per user ranked by compatibility score (CTE)';
WITH RankedMatches AS (
    SELECT 
        u.UserID,
        u.FullName,
        rm.MatchID,
        rm.CompatibilityScore,
        rm.Status,
        ROW_NUMBER() OVER (PARTITION BY u.UserID ORDER BY rm.CompatibilityScore DESC) AS MatchRank
    FROM [User] u
    INNER JOIN RoommateMatch rm ON (u.UserID = rm.UserID1 OR u.UserID = rm.UserID2)
    WHERE u.Role = 'Student'
)
SELECT TOP 20
    UserID,
    FullName,
    MatchID,
    CompatibilityScore,
    Status,
    MatchRank
FROM RankedMatches
WHERE MatchRank <= 3
ORDER BY UserID, MatchRank;
GO

PRINT 'Most active chats ranked by message count (CTE)';
WITH ChatActivity AS (
    SELECT 
        c.ChatID,
        c.User1ID,
        c.User2ID,
        COUNT(m.MessageID) AS MessageCount,
        MAX(m.SentAt) AS LastMessageTime,
        ROW_NUMBER() OVER (ORDER BY COUNT(m.MessageID) DESC) AS ActivityRank
    FROM Chat c
    LEFT JOIN Message m ON c.ChatID = m.ChatID
    GROUP BY c.ChatID, c.User1ID, c.User2ID
)
SELECT TOP 10
    ChatID,
    User1ID,
    User2ID,
    MessageCount,
    LastMessageTime,
    ActivityRank
FROM ChatActivity
ORDER BY MessageCount DESC;
GO

PRINT 'Compatibility score calculation using function';
WITH SampleUsers AS (
    SELECT TOP 100 UserID, FullName
    FROM [User]
    WHERE Role = 'Student'
    ORDER BY UserID
)
SELECT TOP 10
    u1.UserID AS User1ID,
    u1.FullName AS User1Name,
    u2.UserID AS User2ID,
    u2.FullName AS User2Name,
    dbo.fnCompatibilityScore(u1.UserID, u2.UserID) AS CalculatedScore
FROM SampleUsers u1
CROSS JOIN SampleUsers u2
WHERE u1.UserID < u2.UserID
ORDER BY CalculatedScore DESC;
GO

PRINT 'Average rating for landlords using function';
SELECT TOP 10
    l.UserID AS LandlordUserID,
    u.FullName AS LandlordName,
    dbo.fnAverageRatingForLandlord(l.UserID) AS AverageRating,
    l.TotalReviews
FROM Landlord l
INNER JOIN [User] u ON l.UserID = u.UserID
ORDER BY AverageRating DESC;
GO

PRINT 'Partition information for Message table';
SELECT 
    OBJECT_NAME(p.object_id) AS TableName,
    p.partition_number,
    p.rows AS [RowCount],
    pf.name AS PartitionFunction,
    ps.name AS PartitionScheme
FROM sys.partitions p
INNER JOIN sys.indexes i ON p.object_id = i.object_id AND p.index_id = i.index_id
LEFT JOIN sys.partition_schemes ps ON i.data_space_id = ps.data_space_id
LEFT JOIN sys.partition_functions pf ON ps.function_id = pf.function_id
WHERE OBJECT_NAME(p.object_id) = 'Message'
    AND i.name = 'IX_Message_SentAt_Partitioned'
ORDER BY p.partition_number;
GO

PRINT 'Total row counts per table';
SELECT 
    'User' AS TableName, COUNT(*) AS [RowCount] FROM [User]
UNION ALL SELECT 'UserPreference', COUNT(*) FROM UserPreference
UNION ALL SELECT 'Verification', COUNT(*) FROM Verification
UNION ALL SELECT 'Landlord', COUNT(*) FROM Landlord
UNION ALL SELECT 'RoomListing', COUNT(*) FROM RoomListing
UNION ALL SELECT 'RoomAmenities', COUNT(*) FROM RoomAmenities
UNION ALL SELECT 'RoommateMatch', COUNT(*) FROM RoommateMatch
UNION ALL SELECT 'Chat', COUNT(*) FROM Chat
UNION ALL SELECT 'Message', COUNT(*) FROM Message
UNION ALL SELECT 'Review', COUNT(*) FROM Review
ORDER BY [RowCount] DESC;
GO

PRINT 'All demo queries completed!';
PRINT 'Script execution finished successfully.';