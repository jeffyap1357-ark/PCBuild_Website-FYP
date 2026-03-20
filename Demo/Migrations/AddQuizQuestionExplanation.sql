-- Add Explanation to QuizQuestions table
-- Run in Visual Studio: Server Explorer -> DB.mdf -> Right-click -> New Query
-- Or: dotnet ef database update

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuizQuestions') AND name = 'Explanation')
BEGIN
    ALTER TABLE dbo.QuizQuestions ADD Explanation nvarchar(1000) NULL;
END
GO
