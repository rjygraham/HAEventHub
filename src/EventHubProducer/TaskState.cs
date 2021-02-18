using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubProducer
{
	internal class TaskState
	{
		internal Task Task { get; private set; }
		internal CancellationTokenSource CancellationTokenSource { get; private set; }
		

		public TaskState(Task task, CancellationTokenSource cancellationTokenSource)
		{
			Task = task;
			CancellationTokenSource = cancellationTokenSource;
		}
	}
}
