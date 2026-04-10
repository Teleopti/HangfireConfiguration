PRINT 'Installing HangfireConfiguration schema version 2';

CREATE TABLE [$(HangfireConfigurationSchema)].[Configuration2] (
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ConnectionString] [nvarchar](max) NULL,
	[SchemaName] [nvarchar](max) NULL,
	[GoalWorkerCount] [int] NULL,
	[Active] [int] NULL,

	CONSTRAINT [PK_HangfireConfiguration_Configuration] PRIMARY KEY CLUSTERED ([Id] ASC)
);
PRINT 'Created table [$(HangfireConfigurationSchema)].[Configuration2]';

EXEC sp_executesql N'
	INSERT INTO [$(HangfireConfigurationSchema)].[Configuration2] ([GoalWorkerCount]) 
	SELECT CONVERT(int, [Value]) FROM [$(HangfireConfigurationSchema)].[Configuration] 
	WHERE [Key] = ''GoalWorkerCount''
';
PRINT 'Inserted goal worker count';

DROP TABLE [$(HangfireConfigurationSchema)].[Configuration];
PRINT 'Dropped Configuration table';

EXEC sp_rename '[$(HangfireConfigurationSchema)].[Configuration2]', 'Configuration';
PRINT 'Renamed table to [$(HangfireConfigurationSchema)].[Configuration]';
