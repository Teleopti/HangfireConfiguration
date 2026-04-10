PRINT 'Installing HangfireConfiguration schema version 5';

ALTER TABLE [$(HangfireConfigurationSchema)].[Configuration]
ADD MaxWorkersPerServer [INT] NULL

PRINT 'Added MaxWorkersPerServer column in [$(HangfireConfigurationSchema)].[Configuration]';
