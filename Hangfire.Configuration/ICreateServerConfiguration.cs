namespace Hangfire.Configuration;

internal interface ICreateServerConfiguration
{
	void Create(CreateServerConfiguration config);
}