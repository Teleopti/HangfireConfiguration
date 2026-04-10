-- Create the database schema if it doesn't exists
CREATE SCHEMA IF NOT EXISTS $(HangfireConfigurationSchema);

-- Create the schema version table if not exists
CREATE TABLE IF NOT EXISTS $(HangfireConfigurationSchema).schema(
	version INTEGER NOT NULL
)
WITH (
OIDS=FALSE
);
ALTER TABLE $(HangfireConfigurationSchema).schema
	DROP CONSTRAINT IF EXISTS pk_hangfireconfiguration_schema;
ALTER TABLE $(HangfireConfigurationSchema).schema
	ADD CONSTRAINT pk_hangfireconfiguration_schema PRIMARY KEY (version);
