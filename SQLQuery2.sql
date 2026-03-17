IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'Avatar')
BEGIN
    ALTER TABLE dbo.Users ADD Avatar nvarchar(100) NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'Gender')
BEGIN
    ALTER TABLE dbo.Users ADD Gender nvarchar(20) NULL;
END
GO