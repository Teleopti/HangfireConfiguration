INSERT INTO $(HangfireConfigurationSchema).keyvaluestore (key, value)
SELECT
	'Configuration:' || id::text,
	json_build_object(
		'Id', id,
		'ConnectionString', connectionstring,
		'SchemaName', schemaname,
		'GoalWorkerCount', goalworkercount,
		'Active', active,
		'Name', name,
		'MaxWorkersPerServer', maxworkersperserver,
		'WorkerBalancerEnabled', workerbalancerenabled
	)::text
FROM $(HangfireConfigurationSchema).configuration;

DROP TABLE $(HangfireConfigurationSchema).configuration;
