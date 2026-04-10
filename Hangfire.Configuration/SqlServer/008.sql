PRINT 'Installing HangfireConfiguration schema version 8';

UPDATE kv
SET kv.[Value] = JSON_MODIFY(
    JSON_MODIFY(
        JSON_MODIFY(
            JSON_MODIFY(
                kv.[Value],
                '$.Containers',
                JSON_QUERY('[' + container.json + ']')
            ),
            '$.GoalWorkerCount', NULL
        ),
        '$.MaxWorkersPerServer', NULL
    ),
    '$.WorkerBalancerEnabled', NULL
)
FROM [$(HangfireConfigurationSchema)].[KeyValueStore] kv
CROSS APPLY (
    SELECT '{"Tag":"Hangfire"' +
        CASE WHEN JSON_VALUE(kv.[Value], '$.GoalWorkerCount') IS NOT NULL 
             THEN ',"GoalWorkerCount":' + JSON_VALUE(kv.[Value], '$.GoalWorkerCount') ELSE '' END +
        CASE WHEN JSON_VALUE(kv.[Value], '$.MaxWorkersPerServer') IS NOT NULL 
             THEN ',"MaxWorkersPerServer":' + JSON_VALUE(kv.[Value], '$.MaxWorkersPerServer') ELSE '' END +
        CASE WHEN JSON_VALUE(kv.[Value], '$.WorkerBalancerEnabled') IS NOT NULL 
             THEN ',"WorkerBalancerEnabled":' + JSON_VALUE(kv.[Value], '$.WorkerBalancerEnabled') ELSE '' END +
        '}' AS json
) container
WHERE kv.[Key] LIKE 'Configuration:%';

PRINT 'Migrated worker properties to Containers array';
