using System.Collections.Generic;
using System.Linq;

namespace Hangfire.Configuration
{
	public class ServerCountSamples
	{
		public IEnumerable<ServerCountSample> Samples { get; set; } =
			Enumerable.Empty<ServerCountSample>();
	}
}