CREATE DATABASE QL_LICHHOP
USE QL_LICHHOP

CREATE TABLE Departments (
    DepartmentID   INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(255) NOT NULL
);

CREATE TABLE Users (
    UserID       INT IDENTITY(1,1) PRIMARY KEY,
    FullName     NVARCHAR(255) NOT NULL,
    UserName     NVARCHAR(100) NULL,
    PasswordHash NVARCHAR(255) NULL,
    PhoneNumber  NVARCHAR(20),
    Email        NVARCHAR(255) NULL,
    Role         NVARCHAR(50) NOT NULL,
	RoleInMeet	 BIT, -- 1 là chủ trì, 0 là người tham gia 
    DepartmentID INT NOT NULL,
    CreatedAt    DATETIME DEFAULT GETDATE(),
    CreatedBy    NVARCHAR(255) NULL,
    UpdatedAt    DATETIME NULL,
    UpdatedBy    NVARCHAR(255) NULL,
    FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID)
);
CREATE TABLE ScheduleTypes (
    ScheduleTypeID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) UNIQUE NOT NULL,
    Description NVARCHAR(500) NULL
);

CREATE TABLE Meetings (
    MeetingID       INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(MAX) NOT NULL,
    RegistrationPlace NVARCHAR(MAX) NULL,
	DepartmentID    INT NOT NULL,  -- Nơi đăng ký cuộc họp
    ScheduleTypeID  INT NOT NULL, -- Lịch làm việc của phòng hay chi cục...
    ScheduleType    NVARCHAR(50) NOT NULL, -- Loại lịch sáng/chiều
    StartTime       DATETIME NOT NULL,
    DurationMinutes INT NOT NULL,  -- Thời gian dự kiến họp    
    Location        NVARCHAR(255) NOT NULL,
    VehicleType     NVARCHAR(255),  -- Phương tiện hỗ trợ họp
    Preparation     NVARCHAR(MAX),  -- Nội dung cần chuẩn bị
    Status          NVARCHAR(50) DEFAULT 'Chờ duyệt',    
    CreatedBy       NVARCHAR(255) NULL,
    CreatedAt       DATETIME DEFAULT GETDATE(),
    UpdatedBy       NVARCHAR(255) NULL,
    UpdatedAt       DATETIME NULL,
    FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID),
	FOREIGN KEY (ScheduleTypeID) REFERENCES ScheduleTypes(ScheduleTypeID)	
);

CREATE TABLE MeetingHosts (
    ID        INT IDENTITY(1,1) PRIMARY KEY,
    MeetingID INT NOT NULL,
    UserID    INT NOT NULL,  -- Người chủ trì
    CreatedBy NVARCHAR(255) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MeetingID) REFERENCES Meetings(MeetingID) ON DELETE CASCADE,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

CREATE TABLE MeetingParticipants (
    ID          INT IDENTITY(1,1) PRIMARY KEY,
    MeetingID   INT NOT NULL,
    UserID      INT NOT NULL,
    Note		NVARCHAR(50),
    CreatedBy   NVARCHAR(255) NULL,
    CreatedAt   DATETIME DEFAULT GETDATE(),
    UpdatedBy   NVARCHAR(255) NULL,
    UpdatedAt   DATETIME NULL,
    FOREIGN KEY (MeetingID) REFERENCES Meetings(MeetingID) ON DELETE CASCADE,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

CREATE TABLE MeetingAttachments (
    ID          INT IDENTITY(1,1) PRIMARY KEY,
    MeetingID   INT NOT NULL,
    FilePath    NVARCHAR(500) NOT NULL,  -- Đường dẫn file
    UploadedBy  NVARCHAR(255) NOT NULL, -- Người tải lên
    UploadedAt  DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MeetingID) REFERENCES Meetings(MeetingID) ON DELETE CASCADE    
);


INSERT INTO Departments (DepartmentName) 
VALUES 
    (N'Phòng Kế Toán'),
    (N'Phòng Nhân Sự'),
    (N'Phòng Công Nghệ Thông Tin'),
    (N'Phòng Kinh Doanh'),
    (N'Phòng Hành Chính'),
	(N'Khác');
INSERT INTO ScheduleTypes (Name) 
VALUES 
    (N'Lịch Phòng'),
    (N'Lịch Chi cục')    

INSERT INTO Users (FullName, UserName, PasswordHash, PhoneNumber, Email, Role, RoleInMeet, DepartmentID, CreatedAt) 
VALUES 
    (N'Nguyễn Văn A', 'nguyenvana', 'hash123', '0987654321', 'a@example.com', N'Admin', 0, 1, GETDATE()),
    (N'Trần Thị B', 'tranthib', 'hash123', '0971234567', 'b@example.com', N'Trưởng phòng', 1, 2, GETDATE()),
    (N'Lê Văn C', 'levanc', 'hash123', '0962345678', 'c@example.com', N'Nhân viên', 0, 3, GETDATE()),
    (N'Phạm Thị D', 'phamthid', 'hash123', '0953456789', 'd@example.com', N'Nhân viên',0, 4, GETDATE()),
    (N'Hoàng Văn E', 'hoangvane', 'hash123', '0944567890', 'e@example.com', N'Nhân viên', 1, 5, GETDATE());

INSERT INTO Meetings (Title, DepartmentID, ScheduleTypeID, ScheduleType, StartTime, DurationMinutes, Location, VehicleType, Preparation, Status, CreatedAt)
VALUES 
    (N'Họp Kế Hoạch Quý I', 3, 1, N'Sáng', '2025-03-24 09:00:00', 90, N'Phòng họp lớn tầng 2', N'Không', N'Chuẩn bị tài liệu tài chính', N'Chờ duyệt',  GETDATE()),

    (N'Họp Đánh Giá Nhân Sự', 2, 2, N'Chiều', '2025-03-27 14:00:00', 120,  N'Phòng họp HR', N'Không', N'Chuẩn bị danh sách nhân viên', N'Chờ duyệt',  GETDATE()),

    (N'Họp Chiến Lược Kinh Doanh', 4, 1, N'Cả ngày', '2025-03-30 09:30:00', 300,  N'Phòng họp tầng 5', N'Xe đưa đón', N'Chuẩn bị slide trình bày', N'Đã duyệt',  GETDATE());

INSERT INTO MeetingParticipants (MeetingID, UserID, CreatedAt)
VALUES 
    (1, 1, GETDATE()), 
    (1, 3, GETDATE()), 
    (1, 4, GETDATE()), 

    (2, 2, GETDATE()), 
    (2, 5, GETDATE()), 

    (3, 3, GETDATE()), 
    (3, 1, GETDATE()), 
    (3, 4, GETDATE());

INSERT INTO MeetingHosts (MeetingID, UserID)
VALUES 
    (1, 1), 
    (2, 2),
    (3, 3),
    (1, 2),
    (3, 4);

select * from Departments
select * from Meetings
select * from MeetingHosts
select * from MeetingParticipants

