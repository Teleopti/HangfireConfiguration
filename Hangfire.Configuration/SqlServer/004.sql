PRINT 'Installing HangfireConfiguration schema version 4';

CREATE TABLE [$(HangfireConfigurationSchema)].[KeyValueStore] (
    [Key] [nvarchar] (100) NOT NULL,
	[Value] [nvarchar](max),
	CONSTRAINT [PK_HangfireConfiguration_KeyValueStore] PRIMARY KEY CLUSTERED ([Key])
);

PRINT 'Created table [$(HangfireConfigurationSchema)].[KeyValueStore]';
