namespace Hangfire.Configuration.Internals;

internal interface ICreateServerConfiguration
{
	void Create(CreateServerConfiguration config);
}