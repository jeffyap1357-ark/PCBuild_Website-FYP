-- Add Level column to QuizQuestions (required for Knowledge Test levels)
-- Run this in Visual Studio: Server Explorer -> DB.mdf -> Right-click -> New Query

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('QuizQuestions') AND name = 'Level'
)
BEGIN
    ALTER TABLE QuizQuestions ADD Level nvarchar(20) NULL;
END
GO
