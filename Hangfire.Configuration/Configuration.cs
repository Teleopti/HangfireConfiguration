namespace Hangfire.Configuration
{
	public class Configuration
	{
		private readonly IConfigurationRepository _repository;
		
		public Configuration(IConfigurationRepository repository) => 
			_repository = repository;
		
		public void WriteGoalWorkerCount(int? workers) => 
			_repository.WriteGoalWorkerCount(workers);

		public int? ReadGoalWorkerCount() => 
			_repository.ReadGoalWorkerCount();
	}
}