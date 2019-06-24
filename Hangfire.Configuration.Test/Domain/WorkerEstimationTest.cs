using System;
using Xunit;

namespace Hangfire.Configuration.Test.Domain
{
	public class WorkerEstimationTest
	{
		[Theory]
		[InlineData(10)]
		[InlineData(11)]
		public void TheFirstServerShouldGetTheTarget(int target)
		{
			var basis = new WorkerEstimationBasis(){WorkerTarget = target};
			
			Assert.Equal(target, WorkersForMyServer.Estimate(basis));
		}
		
		[Fact]
		public void IncreaseToTheTargetIfItHasNotBeenReached()
		{
			var basis = new WorkerEstimationBasis()
			{
				ExistingServers = 1,
				ExistingWorkers = 2,
				WorkerTarget = 10
			};
			
			Assert.Equal(8, WorkersForMyServer.Estimate(basis));
		}
		
		[Fact]
		public void WhenAlreadyOnTargetWithFirstServerAssignHalfOfTargetToSecondServer()
		{
			var basis = new WorkerEstimationBasis()
			{
				ExistingServers = 1,
				ExistingWorkers = 10,
				WorkerTarget = 10
			};
			
			Assert.Equal(5, WorkersForMyServer.Estimate(basis));
		}
		
		[Fact]
		public void WhenThereIsWorkerSurplusAssignTheNewServerTargetDividedByServers()
		{
			var basis = new WorkerEstimationBasis()
			{
				ExistingServers = 4,
				ExistingWorkers = 15,
				WorkerTarget = 10
			};
			
			Assert.Equal(2, WorkersForMyServer.Estimate(basis));
		}		
		
		[Fact]
		public void WhenThereIsWorkerSurplusAssignTheNewServerTargetDividedByServersRoundedUp()
		{
			var basis = new WorkerEstimationBasis()
			{
				ExistingServers = 2,
				ExistingWorkers = 15,
				WorkerTarget = 10
			};
			
			Assert.Equal(4, WorkersForMyServer.Estimate(basis));
		}
		
		[Fact]
		public void WhenTargetIsZeroUseOneWorker_EdgeCase()
		{
			var basis = new WorkerEstimationBasis(){WorkerTarget = 0};
			
			Assert.Equal(1, WorkersForMyServer.Estimate(basis));
		}

		[Fact]
		public void DontAcceptExistingWorkersWithoutServers()
		{
			var basis = new WorkerEstimationBasis()
			{
				ExistingServers = 0,
				ExistingWorkers = 1,
			};
			
			var exception = Assert.Throws<ArgumentException>(() => WorkersForMyServer.Estimate(basis));
			Assert.Equal("Workers can't exist without servers", exception.Message);
		}
		
		[Fact]
		public void DontAcceptNegativeAmountOfServers()
		{
			var basis = new WorkerEstimationBasis(){ExistingServers = -1};
			
			var exception = Assert.Throws<ArgumentException>(() => WorkersForMyServer.Estimate(basis));
			Assert.Equal("Existing servers can't be negative", exception.Message);
		}
	}

	public static class WorkersForMyServer
	{
		private const int minimumWorkers = 1;
		
		public static int Estimate(WorkerEstimationBasis basis)
		{
			checkBasisViolation(basis);
			
			if (shouldUseMinimum(basis))
				return minimumWorkers;

			if (shouldIncreaseToTarget(basis))
				return basis.WorkerTarget - basis.ExistingWorkers;

			return doEstimation(basis);
		}
		
		private static void checkBasisViolation(WorkerEstimationBasis basis)
		{
			if (basis.ExistingServers < 0)
				throw new ArgumentException("Existing servers can't be negative");
			
			if (basis.ExistingServers < 1 && basis.ExistingWorkers > 0)
				throw new ArgumentException("Workers can't exist without servers");
		}
		
		private static bool shouldUseMinimum(WorkerEstimationBasis basis) => 
			basis.WorkerTarget < minimumWorkers;		

		private static bool shouldIncreaseToTarget(WorkerEstimationBasis basis) => 
			basis.ExistingWorkers < basis.WorkerTarget;
		
		private static int doEstimation(WorkerEstimationBasis basis) =>
			Convert.ToInt32(
				Math.Ceiling(basis.WorkerTarget / ((decimal) basis.ExistingServers + 1)
				));
	}
	
	public class WorkerEstimationBasis
	{
		public int WorkerTarget;
		public int ExistingWorkers;
		public int ExistingServers;
	}
	

}