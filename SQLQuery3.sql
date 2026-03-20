IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuizQuestions') AND name = 'Explanation')
BEGIN
    ALTER TABLE dbo.QuizQuestions ADD Explanation nvarchar(1000) NULL;
END
GO