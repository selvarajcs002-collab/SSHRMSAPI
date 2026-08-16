-- ==========================================
-- Employee Management System (EMS)
-- Database Creation & Schema Script
-- Run this script in SSMS to set up tables and stored procedures.
-- ==========================================

USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'EMS_Database')
BEGIN
    CREATE DATABASE [EMS_Database];
END;
GO

USE [EMS_Database];
GO

-- 1. Create EMS_Employees table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Employees]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EMS_Employees] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [EmployeeCode] NVARCHAR(50) NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [AadhaarNumber] NVARCHAR(20) NOT NULL,
        [PhoneNumber] NVARCHAR(20) NOT NULL,
        [Address] NVARCHAR(250) NOT NULL,
        [City] NVARCHAR(100) NOT NULL,
        [State] NVARCHAR(100) NOT NULL,
        [District] NVARCHAR(100) NOT NULL,
        [Pincode] NVARCHAR(10) NOT NULL,
        [PanNumber] NVARCHAR(20) NULL,
        [BloodGroup] NVARCHAR(5) NULL,
        [MaritalStatus] NVARCHAR(20) NULL,
        [PerDaySalary] DECIMAL(18,2) NOT NULL,
        [Referral] NVARCHAR(100) NULL,
        [IsConfirmed] BIT NOT NULL,
        [Status] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_EMS_Employees] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;
GO

-- 2. Create EMS_EmployeeBankDetails table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EMS_EmployeeBankDetails]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EMS_EmployeeBankDetails] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
        [AccountNumber] NVARCHAR(50) NOT NULL,
        [IfscCode] NVARCHAR(20) NOT NULL,
        [BankName] NVARCHAR(100) NOT NULL,
        [PhoneNumber] NVARCHAR(20) NOT NULL,
        [UpiId] NVARCHAR(50) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_EMS_EmployeeBankDetails] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_EMS_EmployeeBankDetails_EMS_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[EMS_Employees] ([Id]) ON DELETE CASCADE
    );
END;
GO

-- 3. Create EMS_EmployeeDocuments table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EMS_EmployeeDocuments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EMS_EmployeeDocuments] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
        [DocumentType] INT NOT NULL,
        [FileName] NVARCHAR(250) NOT NULL,
        [FilePath] NVARCHAR(500) NOT NULL,
        [ContentType] NVARCHAR(100) NOT NULL,
        [UploadedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_EMS_EmployeeDocuments] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_EMS_EmployeeDocuments_EMS_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[EMS_Employees] ([Id]) ON DELETE CASCADE
    );
END;
GO

-- 4. Create EMS_Shifts table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Shifts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EMS_Shifts] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
        [ShiftType] INT NOT NULL,
        [MachineAllocation] NVARCHAR(100) NOT NULL,
        [AssignmentDate] DATE NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_EMS_Shifts] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_EMS_Shifts_EMS_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[EMS_Employees] ([Id]) ON DELETE CASCADE
    );
END;
GO

-- 5. Create EMS_Attendances table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Attendances]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EMS_Attendances] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
        [Date] DATE NOT NULL,
        [Status] INT NOT NULL,
        [Remarks] NVARCHAR(250) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_EMS_Attendances] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_EMS_Attendances_EMS_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[EMS_Employees] ([Id]) ON DELETE CASCADE
    );
END;
GO

-- 6. Create EMS_Payrolls table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Payrolls]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EMS_Payrolls] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
        [Month] INT NOT NULL,
        [Year] INT NOT NULL,
        [BaseSalary] DECIMAL(18,2) NOT NULL,
        [Incentives] DECIMAL(18,2) NOT NULL,
        [Allowances] DECIMAL(18,2) NOT NULL,
        [AdvancePayments] DECIMAL(18,2) NOT NULL,
        [NetPayable] DECIMAL(18,2) NOT NULL,
        [IsPaid] BIT NOT NULL,
        [PaidDate] DATETIME2 NULL,
        [PayslipFilePath] NVARCHAR(250) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_EMS_Payrolls] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_EMS_Payrolls_EMS_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[EMS_Employees] ([Id]) ON DELETE CASCADE
    );
END;
GO

-- 7. Create EMS_Settings table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Settings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EMS_Settings] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Key] NVARCHAR(100) NOT NULL,
        [Value] NVARCHAR(MAX) NOT NULL,
        [Description] NVARCHAR(250) NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_EMS_Settings] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;
GO


-- Alter EMS_Employees to add new columns if they do not exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Employees]') AND name = N'Email')
BEGIN
    ALTER TABLE [dbo].[EMS_Employees] ADD [Email] NVARCHAR(150) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Employees]') AND name = N'Designation')
BEGIN
    ALTER TABLE [dbo].[EMS_Employees] ADD [Designation] NVARCHAR(100) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Employees]') AND name = N'DepartmentName')
BEGIN
    ALTER TABLE [dbo].[EMS_Employees] ADD [DepartmentName] NVARCHAR(100) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Employees]') AND name = N'ProfilePicture')
BEGIN
    ALTER TABLE [dbo].[EMS_Employees] ADD [ProfilePicture] NVARCHAR(MAX) NULL;
END;
GO

-- 8. Create EMS_Departments table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Departments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EMS_Departments] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(250) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_EMS_Departments] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;
GO

-- 9. Create EMS_Leaves table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EMS_Leaves]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EMS_Leaves] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
        [LeaveType] INT NOT NULL,
        [StartDate] DATE NOT NULL,
        [EndDate] DATE NOT NULL,
        [Status] INT NOT NULL,
        [Reason] NVARCHAR(250) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_EMS_Leaves] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_EMS_Leaves_EMS_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[EMS_Employees] ([Id]) ON DELETE CASCADE
    );
END;
GO


-- =========================================================
-- STORED PROCEDURES
-- =========================================================

-- 1. sp_EMS_RegisterEmployeeBasic
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_EMS_RegisterEmployeeBasic]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_EMS_RegisterEmployeeBasic];
END;
GO
CREATE PROCEDURE [dbo].[sp_EMS_RegisterEmployeeBasic]
    @Id UNIQUEIDENTIFIER,
    @EmployeeCode NVARCHAR(50),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @AadhaarNumber NVARCHAR(20),
    @PhoneNumber NVARCHAR(20),
    @Address NVARCHAR(250),
    @City NVARCHAR(100),
    @State NVARCHAR(100),
    @District NVARCHAR(100),
    @Pincode NVARCHAR(10),
    @PanNumber NVARCHAR(20) = NULL,
    @BloodGroup NVARCHAR(5) = NULL,
    @MaritalStatus NVARCHAR(20) = NULL,
    @PerDaySalary DECIMAL(18,2) = 0,
    @Referral NVARCHAR(100) = NULL,
    @IsConfirmed BIT = 0,
    @Status INT = 1,
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2,
    @Email NVARCHAR(150) = NULL,
    @Designation NVARCHAR(100) = NULL,
    @DepartmentName NVARCHAR(100) = NULL,
    @ProfilePicture NVARCHAR(MAX) = NULL
AS
BEGIN
    INSERT INTO [dbo].[EMS_Employees] (
        [Id], [EmployeeCode], [FirstName], [LastName], [AadhaarNumber], [PhoneNumber],
        [Address], [City], [State], [District], [Pincode], [PanNumber], [BloodGroup],
        [MaritalStatus], [PerDaySalary], [Referral], [IsConfirmed], [Status], [CreatedAt], [UpdatedAt],
        [Email], [Designation], [DepartmentName], [ProfilePicture]
    )
    VALUES (
        @Id, @EmployeeCode, @FirstName, @LastName, @AadhaarNumber, @PhoneNumber,
        @Address, @City, @State, @District, @Pincode, @PanNumber, @BloodGroup,
        @MaritalStatus, @PerDaySalary, @Referral, @IsConfirmed, @Status, @CreatedAt, @UpdatedAt,
        @Email, @Designation, @DepartmentName, @ProfilePicture
    );
END;
GO

-- 2. sp_EMS_SaveBankDetails
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_EMS_SaveBankDetails]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_EMS_SaveBankDetails];
END;
GO
CREATE PROCEDURE [dbo].[sp_EMS_SaveBankDetails]
    @Id UNIQUEIDENTIFIER,
    @EmployeeId UNIQUEIDENTIFIER,
    @AccountNumber NVARCHAR(50),
    @IfscCode NVARCHAR(20),
    @BankName NVARCHAR(100),
    @PhoneNumber NVARCHAR(20),
    @UpiId NVARCHAR(50) = NULL,
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2
AS
BEGIN
    IF EXISTS (SELECT 1 FROM [dbo].[EMS_EmployeeBankDetails] WHERE [EmployeeId] = @EmployeeId)
    BEGIN
        UPDATE [dbo].[EMS_EmployeeBankDetails]
        SET [AccountNumber] = @AccountNumber,
            [IfscCode] = @IfscCode,
            [BankName] = @BankName,
            [PhoneNumber] = @PhoneNumber,
            [UpiId] = @UpiId,
            [UpdatedAt] = @UpdatedAt
        WHERE [EmployeeId] = @EmployeeId;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[EMS_EmployeeBankDetails] (
            [Id], [EmployeeId], [AccountNumber], [IfscCode], [BankName], [PhoneNumber], [UpiId], [CreatedAt], [UpdatedAt]
        )
        VALUES (
            @Id, @EmployeeId, @AccountNumber, @IfscCode, @BankName, @PhoneNumber, @UpiId, @CreatedAt, @UpdatedAt
        );
    END;
END;
GO

-- 3. sp_EMS_UpdateEmployee
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_EMS_UpdateEmployee]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_EMS_UpdateEmployee];
END;
GO
CREATE PROCEDURE [dbo].[sp_EMS_UpdateEmployee]
    @Id UNIQUEIDENTIFIER,
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @AadhaarNumber NVARCHAR(20),
    @PhoneNumber NVARCHAR(20),
    @Address NVARCHAR(250),
    @City NVARCHAR(100),
    @State NVARCHAR(100),
    @District NVARCHAR(100),
    @Pincode NVARCHAR(10),
    @PanNumber NVARCHAR(20) = NULL,
    @BloodGroup NVARCHAR(5) = NULL,
    @MaritalStatus NVARCHAR(20) = NULL,
    @PerDaySalary DECIMAL(18,2) = 0,
    @Referral NVARCHAR(100) = NULL,
    @Status INT,
    @IsConfirmed BIT,
    @EmployeeCode NVARCHAR(50) = NULL,
    @UpdatedAt DATETIME2,
    @Email NVARCHAR(150) = NULL,
    @Designation NVARCHAR(100) = NULL,
    @DepartmentName NVARCHAR(100) = NULL,
    @ProfilePicture NVARCHAR(MAX) = NULL
AS
BEGIN
    UPDATE [dbo].[EMS_Employees]
    SET [FirstName] = @FirstName,
        [LastName] = @LastName,
        [AadhaarNumber] = @AadhaarNumber,
        [PhoneNumber] = @PhoneNumber,
        [Address] = @Address,
        [City] = @City,
        [State] = @State,
        [District] = @District,
        [Pincode] = @Pincode,
        [PanNumber] = @PanNumber,
        [BloodGroup] = @BloodGroup,
        [MaritalStatus] = @MaritalStatus,
        [PerDaySalary] = @PerDaySalary,
        [Referral] = @Referral,
        [Status] = @Status,
        [IsConfirmed] = @IsConfirmed,
        [EmployeeCode] = COALESCE(@EmployeeCode, [EmployeeCode]),
        [UpdatedAt] = @UpdatedAt,
        [Email] = @Email,
        [Designation] = @Designation,
        [DepartmentName] = @DepartmentName,
        [ProfilePicture] = @ProfilePicture
    WHERE [Id] = @Id;
END;
GO

-- 4. sp_EMS_SoftDeleteEmployee
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_EMS_SoftDeleteEmployee]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_EMS_SoftDeleteEmployee];
END;
GO
CREATE PROCEDURE [dbo].[sp_EMS_SoftDeleteEmployee]
    @Id UNIQUEIDENTIFIER,
    @Status INT = 2, -- Inactive
    @UpdatedAt DATETIME2
AS
BEGIN
    UPDATE [dbo].[EMS_Employees]
    SET [Status] = @Status,
        [UpdatedAt] = @UpdatedAt
    WHERE [Id] = @Id;
END;
GO

-- 5. sp_EMS_AssignShift
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_EMS_AssignShift]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_EMS_AssignShift];
END;
GO
CREATE PROCEDURE [dbo].[sp_EMS_AssignShift]
    @Id UNIQUEIDENTIFIER,
    @EmployeeId UNIQUEIDENTIFIER,
    @ShiftType INT,
    @MachineAllocation NVARCHAR(100),
    @AssignmentDate DATE,
    @CreatedAt DATETIME2
AS
BEGIN
    -- Delete previous assignment on same date for that employee
    DELETE FROM [dbo].[EMS_Shifts] WHERE [EmployeeId] = @EmployeeId AND [AssignmentDate] = @AssignmentDate;

    INSERT INTO [dbo].[EMS_Shifts] (
        [Id], [EmployeeId], [ShiftType], [MachineAllocation], [AssignmentDate], [CreatedAt]
    )
    VALUES (
        @Id, @EmployeeId, @ShiftType, @MachineAllocation, @AssignmentDate, @CreatedAt
    );
END;
GO

-- 6. sp_EMS_MarkAttendance
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_EMS_MarkAttendance]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_EMS_MarkAttendance];
END;
GO
CREATE PROCEDURE [dbo].[sp_EMS_MarkAttendance]
    @Id UNIQUEIDENTIFIER,
    @EmployeeId UNIQUEIDENTIFIER,
    @Date DATE,
    @Status INT,
    @Remarks NVARCHAR(250) = NULL,
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2
AS
BEGIN
    IF EXISTS (SELECT 1 FROM [dbo].[EMS_Attendances] WHERE [EmployeeId] = @EmployeeId AND [Date] = @Date)
    BEGIN
        UPDATE [dbo].[EMS_Attendances]
        SET [Status] = @Status,
            [Remarks] = @Remarks,
            [UpdatedAt] = @UpdatedAt
        WHERE [EmployeeId] = @EmployeeId AND [Date] = @Date;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[EMS_Attendances] (
            [Id], [EmployeeId], [Date], [Status], [Remarks], [CreatedAt], [UpdatedAt]
        )
        VALUES (
            @Id, @EmployeeId, @Date, @Status, @Remarks, @CreatedAt, @UpdatedAt
        );
    END;
END;
GO

-- 7. sp_EMS_GeneratePayroll
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_EMS_GeneratePayroll]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_EMS_GeneratePayroll];
END;
GO
CREATE PROCEDURE [dbo].[sp_EMS_GeneratePayroll]
    @Id UNIQUEIDENTIFIER,
    @EmployeeId UNIQUEIDENTIFIER,
    @Month INT,
    @Year INT,
    @BaseSalary DECIMAL(18,2),
    @Incentives DECIMAL(18,2),
    @Allowances DECIMAL(18,2),
    @AdvancePayments DECIMAL(18,2),
    @NetPayable DECIMAL(18,2),
    @IsPaid BIT,
    @PaidDate DATETIME2 = NULL,
    @PayslipFilePath NVARCHAR(250) = NULL,
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2 = NULL
AS
BEGIN
    IF EXISTS (SELECT 1 FROM [dbo].[EMS_Payrolls] WHERE [EmployeeId] = @EmployeeId AND [Month] = @Month AND [Year] = @Year)
    BEGIN
        UPDATE [dbo].[EMS_Payrolls]
        SET [BaseSalary] = @BaseSalary,
            [Incentives] = @Incentives,
            [Allowances] = @Allowances,
            [AdvancePayments] = @AdvancePayments,
            [NetPayable] = @NetPayable,
            [IsPaid] = @IsPaid,
            [PaidDate] = @PaidDate,
            [PayslipFilePath] = COALESCE(@PayslipFilePath, [PayslipFilePath]),
            [UpdatedAt] = @UpdatedAt
        WHERE [EmployeeId] = @EmployeeId AND [Month] = @Month AND [Year] = @Year;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[EMS_Payrolls] (
            [Id], [EmployeeId], [Month], [Year], [BaseSalary], [Incentives], [Allowances], [AdvancePayments], [NetPayable], [IsPaid], [PaidDate], [PayslipFilePath], [CreatedAt], [UpdatedAt]
        )
        VALUES (
            @Id, @EmployeeId, @Month, @Year, @BaseSalary, @Incentives, @Allowances, @AdvancePayments, @NetPayable, @IsPaid, @PaidDate, @PayslipFilePath, @CreatedAt, @UpdatedAt
        );
    END;
END;
GO

-- 8. sp_EMS_UpdateSetting
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_EMS_UpdateSetting]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_EMS_UpdateSetting];
END;
GO
CREATE PROCEDURE [dbo].[sp_EMS_UpdateSetting]
    @Id UNIQUEIDENTIFIER,
    @Key NVARCHAR(100),
    @Value NVARCHAR(MAX),
    @Description NVARCHAR(250) = NULL,
    @UpdatedAt DATETIME2
AS
BEGIN
    IF EXISTS (SELECT 1 FROM [dbo].[EMS_Settings] WHERE [Key] = @Key)
    BEGIN
        UPDATE [dbo].[EMS_Settings]
        SET [Value] = @Value,
            [UpdatedAt] = @UpdatedAt
        WHERE [Key] = @Key;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[EMS_Settings] (
            [Id], [Key], [Value], [Description], [UpdatedAt]
        )
        VALUES (
            @Id, @Key, @Value, @Description, @UpdatedAt
        );
    END;
END;
GO

-- 9. sp_EMS_UpsertDepartment
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_EMS_UpsertDepartment]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_EMS_UpsertDepartment];
END;
GO
CREATE PROCEDURE [dbo].[sp_EMS_UpsertDepartment]
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Description NVARCHAR(250) = NULL,
    @CreatedAt DATETIME2
AS
BEGIN
    IF EXISTS (SELECT 1 FROM [dbo].[EMS_Departments] WHERE [Id] = @Id)
    BEGIN
        UPDATE [dbo].[EMS_Departments]
        SET [Name] = @Name,
            [Description] = @Description
        WHERE [Id] = @Id;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[EMS_Departments] ([Id], [Name], [Description], [CreatedAt])
        VALUES (@Id, @Name, @Description, @CreatedAt);
    END;
END;
GO

-- 10. sp_EMS_DeleteDepartment
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_EMS_DeleteDepartment]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_EMS_DeleteDepartment];
END;
GO
CREATE PROCEDURE [dbo].[sp_EMS_DeleteDepartment]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    DELETE FROM [dbo].[EMS_Departments] WHERE [Id] = @Id;
END;
GO

-- 11. sp_EMS_ApplyLeave
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_EMS_ApplyLeave]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_EMS_ApplyLeave];
END;
GO
CREATE PROCEDURE [dbo].[sp_EMS_ApplyLeave]
    @Id UNIQUEIDENTIFIER,
    @EmployeeId UNIQUEIDENTIFIER,
    @LeaveType INT,
    @StartDate DATE,
    @EndDate DATE,
    @Status INT,
    @Reason NVARCHAR(250) = NULL,
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2
AS
BEGIN
    INSERT INTO [dbo].[EMS_Leaves] (
        [Id], [EmployeeId], [LeaveType], [StartDate], [EndDate], [Status], [Reason], [CreatedAt], [UpdatedAt]
    )
    VALUES (
        @Id, @EmployeeId, @LeaveType, @StartDate, @EndDate, @Status, @Reason, @CreatedAt, @UpdatedAt
    );
END;
GO

-- 12. sp_EMS_UpdateLeaveStatus
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_EMS_UpdateLeaveStatus]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_EMS_UpdateLeaveStatus];
END;
GO
CREATE PROCEDURE [dbo].[sp_EMS_UpdateLeaveStatus]
    @Id UNIQUEIDENTIFIER,
    @Status INT,
    @UpdatedAt DATETIME2
AS
BEGIN
    UPDATE [dbo].[EMS_Leaves]
    SET [Status] = @Status,
        [UpdatedAt] = @UpdatedAt
    WHERE [Id] = @Id;
END;
GO
