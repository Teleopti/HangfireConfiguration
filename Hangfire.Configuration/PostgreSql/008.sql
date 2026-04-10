UPDATE $(HangfireConfigurationSchema).keyvaluestore
SET value = (
	(value::jsonb - 'GoalWorkerCount' - 'MaxWorkersPerServer' - 'WorkerBalancerEnabled')
	|| jsonb_build_object('Containers', jsonb_build_array(
		jsonb_strip_nulls(jsonb_build_object(
			'Tag', 'Hangfire',
			'GoalWorkerCount', (value::jsonb->>'GoalWorkerCount')::int,
			'MaxWorkersPerServer', (value::jsonb->>'MaxWorkersPerServer')::int,
			'WorkerBalancerEnabled', (value::jsonb->>'WorkerBalancerEnabled')::boolean
		))
	))
)::text
WHERE key LIKE 'Configuration:%';
