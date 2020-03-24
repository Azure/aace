DECLARE @current_version bigint
DECLARE @target_version bigint
DECLARE @upgrade_version bigint

--SET @target_version = $(target_version)
SET @target_version = 2

IF NOT EXISTS (select * from sys.tables tb join sys.schemas sch on tb.schema_id = sch.schema_id where tb.name = 'MetadataVersion' AND sch.name = 'dbo')
BEGIN
	CREATE TABLE [dbo].[MetadataVersion] (currentVersion bigint)
	INSERT INTO [dbo].[MetadataVersion] VALUES (1)
	SET @current_version = 1
END
ELSE BEGIN
	SELECT @current_version = currentVersion FROM [dbo].[MetadataVersion]
END

--- Upgrade from Version 1 to Version 2 ---
SET @upgrade_version = 2
IF (@current_version < @upgrade_version AND @target_version >= @upgrade_version)
BEGIN
	PRINT "Upgrade from version 1 to version 2"
END
