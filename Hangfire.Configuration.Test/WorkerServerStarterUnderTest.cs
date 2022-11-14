using Hangfire.Server;

namespace Hangfire.Configuration.Test
{
    public class WorkerServerStarterUnderTest
    {
        private readonly WorkerServerStarter _instance;
        private readonly Options _options;

        public WorkerServerStarterUnderTest(WorkerServerStarter instance, Options options)
        {
            _instance = instance;
            _options = options;
        }

        public void Start() => 
	        _instance.Start();

        public void Start(IBackgroundProcess additionalProcess) => 
	        _instance.Start(additionalProcess.AsArray());

        public void Start(IBackgroundProcess[] additionalProcesses) => 
	        _instance.Start(additionalProcesses);

        public void Start(ConfigurationOptions options)
        {
	        if (string.IsNullOrEmpty(options.ConnectionString))
		        options.ConnectionString = new ConfigurationOptionsForTest().ConnectionString;
	        _options.UseOptions(options);
	        _instance.Start();
        }
    }
}