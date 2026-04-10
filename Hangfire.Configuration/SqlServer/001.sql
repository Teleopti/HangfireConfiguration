PRINT 'Installing HangfireConfiguration schema version 1';
CREATE TABLE [$(HangfireConfigurationSchema)].[Configuration](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Key] [nvarchar](100) NOT NULL,
    [Value] [nvarchar](max) NULL,
        
    CONSTRAINT [PK_HangfireConfiguration_Value] PRIMARY KEY CLUSTERED (
        [Id] ASC
    )
);
PRINT 'Created table [$(HangfireConfigurationSchema)].[Configuration]';
