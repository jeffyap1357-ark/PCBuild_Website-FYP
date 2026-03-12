-- Create Component table (replacement for Hardware)
-- Run this to add the Component table. Hardware table remains for now.
-- To fully replace: migrate data, update FKs, then drop Hardware.

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Component')
BEGIN
    CREATE TABLE [dbo].[Component] (
        [ComponentId] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [CategoryId] INT NOT NULL,
        [Price] DECIMAL(10, 2) NOT NULL,
        [Detail] NVARCHAR(2000) NULL,
        [Brand] NVARCHAR(100) NULL,
        [Picture] NVARCHAR(200) NULL,
        [Reference] NVARCHAR(500) NULL,
        [Socket] NVARCHAR(50) NULL,
        [DdrType] NVARCHAR(20) NULL,
        [FormFactor] NVARCHAR(50) NULL,
        [Special] INT NULL,
        [RamSlots] INT NULL,
        [ReleaseYear] INT NULL,
        [Rating] INT NULL,
        [PerformanceScore] INT NULL,
        [PowerConsumption] INT NULL,
        [GamingScore] INT NULL,
        [OfficeScore] INT NULL,
        [EditingScore] INT NULL,
        CONSTRAINT [PK_Component] PRIMARY KEY ([ComponentId]),
        CONSTRAINT [FK_Component_Category_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories]([CategoryId])
    );

    CREATE INDEX [IX_Component_CategoryId] ON [dbo].[Component]([CategoryId]);

    PRINT 'Component table created.';
END
ELSE
BEGIN
    PRINT 'Component table already exists.';
END
