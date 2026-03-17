IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('QuizQuestions') AND name = 'Level'
)
BEGIN
    ALTER TABLE QuizQuestions ADD Level nvarchar(20) NULL;
END