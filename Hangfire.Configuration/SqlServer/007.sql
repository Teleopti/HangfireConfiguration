PRINT 'Installing HangfireConfiguration schema version 7';

EXEC sp_executesql N'
    INSERT INTO [$(HangfireConfigurationSchema)].[KeyValueStore] ([Key], [Value])
    SELECT
        ''Configuration:'' + CONVERT(nvarchar, [Id]),
        (SELECT
            [Id],
            [ConnectionString],
            [SchemaName],
            [GoalWorkerCount],
            [Active],
            [Name],
            [MaxWorkersPerServer],
            [WorkerBalancerEnabled]
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM [$(HangfireConfigurationSchema)].[Configuration]
';

PRINT 'Migrated Configuration rows to KeyValueStore';

DROP TABLE [$(HangfireConfigurationSchema)].[Configuration];

PRINT 'Dropped Configuration table';
