-- Add CleanPicture column to Hardware table (for admin-uploaded clean/build diagram images).
-- Run if you have not applied the EF migration AddCleanPictureToHardware.

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Hardware') AND name = N'CleanPicture'
)
BEGIN
    ALTER TABLE dbo.Hardware ADD CleanPicture nvarchar(200) NULL;
    PRINT 'Added column Hardware.CleanPicture.';
END
ELSE
    PRINT 'Column Hardware.CleanPicture already exists.';
