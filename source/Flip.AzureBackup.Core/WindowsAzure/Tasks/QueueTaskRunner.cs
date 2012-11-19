using System;
using System.Collections.Generic;
using Flip.AzureBackup.Messages;
using Flip.Common.Messages;
using Flip.Common.Threading;



namespace Flip.AzureBackup.WindowsAzure.Tasks
{
	public sealed class QueueTaskRunner : MessageBasedTaskRunner<SyncStartedMessage, SyncPausedMessage, SyncStoppedMessage>
	{
		public QueueTaskRunner(IMessageBus messageBus, Queue<TaskRunner> taskQueue)
			: base(messageBus)
		{
			_taskQueue = taskQueue;
		}



		protected override bool LoopCondition()
		{
			return _taskQueue.Count > 0;
		}

		protected override void LoopAction()
		{
			using (TaskRunner item = _taskQueue.Dequeue())
			{
				item
					.Start()
					.Wait();
			}
		}



		private readonly Queue<TaskRunner> _taskQueue;
	}
}
