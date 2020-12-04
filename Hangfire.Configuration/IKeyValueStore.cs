namespace Hangfire.Configuration
{
	public interface IKeyValueStore
	{
		void Write(string key, string value);
		string Read(string key);
	}
}