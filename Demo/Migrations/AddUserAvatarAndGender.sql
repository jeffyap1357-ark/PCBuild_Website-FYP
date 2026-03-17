-- Add Avatar and Gender to Users table
-- Run in Visual Studio: Server Explorer -> DB.mdf -> New Query

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Avatar')
BEGIN
    ALTER TABLE Users ADD Avatar nvarchar(100) NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Gender')
BEGIN
    ALTER TABLE Users ADD Gender nvarchar(20) NULL;
END
GO
