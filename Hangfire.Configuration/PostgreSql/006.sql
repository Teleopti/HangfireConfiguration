ALTER TABLE $(HangfireConfigurationSchema).Configuration
	ADD COLUMN WorkerBalancerEnabled BOOLEAN NULL;
