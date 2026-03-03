-- Add DraftBuild and DraftBuildDetail tables for temporary recommendation storage
-- Run this script against your database (e.g. in SQL Server Management Studio or dotnet ef database update)

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DraftBuilds')
BEGIN
    CREATE TABLE [dbo].[DraftBuilds] (
        [DraftBuildId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] INT NOT NULL,
        [BuildName] NVARCHAR(200) NOT NULL DEFAULT 'Recommended Build',
        [TotalPrice] DECIMAL(10,2) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_DraftBuilds_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_DraftBuilds_UserId] ON [dbo].[DraftBuilds]([UserId]);
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DraftBuildDetails')
BEGIN
    CREATE TABLE [dbo].[DraftBuildDetails] (
        [DraftBuildDetailId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [DraftBuildId] INT NOT NULL,
        [HardwareId] INT NOT NULL,
        [Quantity] INT NOT NULL DEFAULT 1,
        CONSTRAINT [FK_DraftBuildDetails_DraftBuilds] FOREIGN KEY ([DraftBuildId]) REFERENCES [dbo].[DraftBuilds]([DraftBuildId]) ON DELETE CASCADE,
        CONSTRAINT [FK_DraftBuildDetails_Hardware] FOREIGN KEY ([HardwareId]) REFERENCES [Hardware]([HardwareId]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_DraftBuildDetails_DraftBuildId] ON [dbo].[DraftBuildDetails]([DraftBuildId]);
    CREATE INDEX [IX_DraftBuildDetails_HardwareId] ON [dbo].[DraftBuildDetails]([HardwareId]);
END
