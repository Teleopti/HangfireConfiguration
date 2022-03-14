namespace Hangfire.Configuration;

public interface IStorageOptionsFactory
{
	public object Make(StoredConfiguration configuration);
}