PRINT 'Installing HangfireConfiguration schema version 6';

ALTER TABLE [$(HangfireConfigurationSchema)].[Configuration]
ADD WorkerBalancerEnabled [INT] NULL

PRINT 'Added WorkerBalancerEnabled column in [$(HangfireConfigurationSchema)].[Configuration]';
