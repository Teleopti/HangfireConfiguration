PRINT 'Installing HangfireConfiguration schema version 3';

ALTER TABLE [$(HangfireConfigurationSchema)].[Configuration] ADD [Name] nvarchar(max) NULL

EXEC sp_executesql N'
	DECLARE @FirstConfigurationId INT;
	SELECT @FirstConfigurationId = MIN(Id) FROM [$(HangfireConfigurationSchema)].[Configuration];
	UPDATE [$(HangfireConfigurationSchema)].[Configuration] SET [Name] = ''Hangfire'' WHERE Id = @FirstConfigurationId;
';
