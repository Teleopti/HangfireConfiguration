
SET NOCOUNT ON
SET XACT_ABORT ON
DECLARE @TARGET_SCHEMA_VERSION INT;
SET @TARGET_SCHEMA_VERSION = $(HangfireConfigurationSchemaVersion);

PRINT 'Installing HangfireConfiguration SQL objects...';

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

PRINT 'Current HangfireConfiguration schema version: ' + CASE WHEN @CURRENT_SCHEMA_VERSION IS NULL THEN 'none' ELSE CONVERT(nvarchar, @CURRENT_SCHEMA_VERSION) END;

IF @CURRENT_SCHEMA_VERSION IS NULL AND @TARGET_SCHEMA_VERSION >= 1
BEGIN
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
    
    SET @CURRENT_SCHEMA_VERSION = 1;    
END

IF @CURRENT_SCHEMA_VERSION < 2 AND @TARGET_SCHEMA_VERSION >= 2
BEGIN
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

	SET @CURRENT_SCHEMA_VERSION = 2;
END

IF @CURRENT_SCHEMA_VERSION < 3 AND @TARGET_SCHEMA_VERSION >= 3
BEGIN
    
    PRINT 'Installing HangfireConfiguration schema version 3';
    
    ALTER TABLE [$(HangfireConfigurationSchema)].[Configuration] ADD [Name] nvarchar(max) NULL
    
	EXEC sp_executesql N'
		DECLARE @FirstConfigurationId INT;
		SELECT @FirstConfigurationId = MIN(Id) FROM [$(HangfireConfigurationSchema)].[Configuration];
		UPDATE [$(HangfireConfigurationSchema)].[Configuration] SET [Name] = ''Hangfire'' WHERE Id = @FirstConfigurationId;
    ';
    
SET @CURRENT_SCHEMA_VERSION = 3;
END

IF @CURRENT_SCHEMA_VERSION < 4 AND @TARGET_SCHEMA_VERSION >= 4
BEGIN
    
    PRINT 'Installing HangfireConfiguration schema version 4';
    
   CREATE TABLE [$(HangfireConfigurationSchema)].[KeyValueStore] (
        [Key] [nvarchar] (100) NOT NULL,
		[Value] [nvarchar](max),
		CONSTRAINT [PK_HangfireConfiguration_KeyValueStore] PRIMARY KEY CLUSTERED ([Key])
	);
	
	PRINT 'Created table [$(HangfireConfigurationSchema)].[KeyValueStore]';
    
SET @CURRENT_SCHEMA_VERSION = 4;
END

IF @CURRENT_SCHEMA_VERSION < 5 AND @TARGET_SCHEMA_VERSION >= 5
BEGIN
    
    PRINT 'Installing HangfireConfiguration schema version 5';
    
    ALTER TABLE [$(HangfireConfigurationSchema)].[Configuration]
    ADD MaxWorkersPerServer [INT] NULL
	
	PRINT 'Added MaxWorkersPerServer column in [$(HangfireConfigurationSchema)].[Configuration]';
    
SET @CURRENT_SCHEMA_VERSION = 5;
END

IF @CURRENT_SCHEMA_VERSION < 6 AND @TARGET_SCHEMA_VERSION >= 6
BEGIN
    
    PRINT 'Installing HangfireConfiguration schema version 6';
	
	ALTER TABLE [$(HangfireConfigurationSchema)].[Configuration]
	ADD WorkerBalancerEnabled [INT] NULL
	
	PRINT 'Added WorkerBalancerEnabled column in [$(HangfireConfigurationSchema)].[Configuration]';
	
SET @CURRENT_SCHEMA_VERSION = 6;
END




UPDATE [$(HangfireConfigurationSchema)].[Schema] SET [Version] = @CURRENT_SCHEMA_VERSION
IF @@ROWCOUNT = 0 
	INSERT INTO [$(HangfireConfigurationSchema)].[Schema] ([Version]) VALUES (@CURRENT_SCHEMA_VERSION)        

PRINT 'HangfireConfiguration database schema installed';

COMMIT TRANSACTION;
PRINT 'HangfireConfiguration SQL objects installed';


--
-- IF @CURRENT_SCHEMA_VERSION < 100 AND @TARGET_SCHEMA_VERSION >= 100
-- BEGIN
--     
-- PRINT 'Installing HangfireConfiguration schema version 100';
-- 
-- Insert migration here
-- 
-- SET @CURRENT_SCHEMA_VERSION = 100;
-- END
