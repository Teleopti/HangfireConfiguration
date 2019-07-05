
SET NOCOUNT ON
SET XACT_ABORT ON
DECLARE @TARGET_SCHEMA_VERSION INT;
SET @TARGET_SCHEMA_VERSION = 1;

PRINT 'Installing Hangfire Configuration SQL objects...';

BEGIN TRANSACTION;

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
    
DECLARE @CURRENT_SCHEMA_VERSION int;
SELECT @CURRENT_SCHEMA_VERSION = [Version] FROM [$(HangfireConfigurationSchema)].[Schema];

PRINT 'Current Hangfire schema version: ' + CASE WHEN @CURRENT_SCHEMA_VERSION IS NULL THEN 'none' ELSE CONVERT(nvarchar, @CURRENT_SCHEMA_VERSION) END;

IF @CURRENT_SCHEMA_VERSION IS NOT NULL AND @CURRENT_SCHEMA_VERSION > @TARGET_SCHEMA_VERSION
BEGIN
    ROLLBACK TRANSACTION;
    PRINT 'Hangfire current database schema version ' + CAST(@CURRENT_SCHEMA_VERSION AS NVARCHAR) +
          ' is newer than the configured SqlServerStorage schema version ' + CAST(@TARGET_SCHEMA_VERSION AS NVARCHAR) +
          '. Will not apply any migrations.';
    RETURN;
END    


IF @CURRENT_SCHEMA_VERSION IS NULL
BEGIN
    CREATE TABLE [$(HangfireConfigurationSchema)].[Configuration](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Key] [nvarchar](100) NOT NULL,
        [Value] [nvarchar](max) NULL,
            
        CONSTRAINT [PK_HangfireConfiguration_Value] PRIMARY KEY CLUSTERED (
            [Id] ASC
        )
    );
    PRINT 'Created table [$(HangfireConfigurationSchema)].[Configuration]';
    
    SET @CURRENT_SCHEMA_VERSION = 1;    
END
ELSE
    PRINT 'Table [$(HangfireConfigurationSchema)].[Configuration] already exists';

IF @CURRENT_SCHEMA_VERSION = 1
BEGIN
	PRINT 'Installing schema version 2';

    DECLARE @GOAL_WORKER_COUNT int;
    SELECT @GOAL_WORKER_COUNT = CONVERT(int, [Value]) FROM [$(HangfireConfigurationSchema)].[Configuration] WHERE [Key] = 'GoalWorkerCount';

	CREATE TABLE [$(HangfireConfigurationSchema)].[Configuration2] (
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[ConnectionString] [nvarchar](max) NULL,
		[SchemaName] [nvarchar](max) NULL,
		[GoalWorkerCount] [int] NULL,
		[Active] [int] NULL,

		CONSTRAINT [PK_HangfireConfiguration_Configuration] PRIMARY KEY CLUSTERED ([Id] ASC)
	);
	PRINT 'Created table [$(HangfireConfigurationSchema)].[Configuration2]';

    IF @GOAL_WORKER_COUNT IS NOT NULL
    BEGIN
        INSERT INTO [$(HangfireConfigurationSchema)].[Configuration2] ([GoalWorkerCount]) VALUES (@GOAL_WORKER_COUNT);
        PRINT 'Inserted goal worker count';
    END

    DROP TABLE [$(HangfireConfigurationSchema)].[Configuration];
    PRINT 'Dropped Configuration table';

    EXEC sp_rename '[$(HangfireConfigurationSchema)].[Configuration2]', 'Configuration';
    PRINT 'Renamed table to [$(HangfireConfigurationSchema)].[Configuration]';

	SET @CURRENT_SCHEMA_VERSION = 2;
END

UPDATE [$(HangfireConfigurationSchema)].[Schema] SET [Version] = @CURRENT_SCHEMA_VERSION
IF @@ROWCOUNT = 0 
	INSERT INTO [$(HangfireConfigurationSchema)].[Schema] ([Version]) VALUES (@CURRENT_SCHEMA_VERSION)        

PRINT 'Hangfire configuration database schema installed';

COMMIT TRANSACTION;
PRINT 'Hangfire configuration SQL objects installed';
