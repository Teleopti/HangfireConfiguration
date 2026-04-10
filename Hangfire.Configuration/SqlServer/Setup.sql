SET NOCOUNT ON
SET XACT_ABORT ON

PRINT 'Installing HangfireConfiguration SQL objects...';

-- Acquire exclusive lock to prevent deadlocks caused by schema creation / version update
DECLARE @SchemaLockResult INT;
EXEC @SchemaLockResult = sp_getapplock @Resource = '$(HangfireConfigurationSchema):SchemaLock', @LockMode = 'Exclusive'

-- Create the database schema if it doesn't exists
IF NOT EXISTS (SELECT [schema_id] FROM [sys].[schemas] WHERE [name] = '$(HangfireConfigurationSchema)')
BEGIN
    EXEC (N'CREATE SCHEMA [$(HangfireConfigurationSchema)]');
    PRINT 'Created database schema [$(HangfireConfigurationSchema)]';
END
ELSE
    PRINT 'Database schema [$(HangfireConfigurationSchema)] already exists';
    
    
DECLARE @SCHEMA_ID int;
SELECT @SCHEMA_ID = [schema_id] FROM [sys].[schemas] WHERE [name] = '$(HangfireConfigurationSchema)';

-- Create the [$(HangfireConfigurationSchema)].Schema table if not exists
IF NOT EXISTS(SELECT [object_id] FROM [sys].[tables] 
    WHERE [name] = 'Schema' AND [schema_id] = @SCHEMA_ID)
BEGIN
    CREATE TABLE [$(HangfireConfigurationSchema)].[Schema](
        [Version] [int] NOT NULL,
        CONSTRAINT [PK_HangfireConfiguration_Schema] PRIMARY KEY CLUSTERED ([Version] ASC)
    );
    PRINT 'Created table [$(HangfireConfigurationSchema)].[Schema]';
END
ELSE
    PRINT 'Table [$(HangfireConfigurationSchema)].[Schema] already exists';
