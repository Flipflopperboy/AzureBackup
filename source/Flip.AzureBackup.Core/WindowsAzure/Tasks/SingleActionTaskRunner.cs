using System;
using Flip.AzureBackup.IO;
using Flip.Common.Threading;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.WindowsAzure.Tasks
{
	public class SingleActionTaskRunner : TaskRunner
	{
		public SingleActionTaskRunner(Action action)
		{
			_action = action;
		}



		protected override bool LoopCondition()
		{
			return !_done;
		}

		protected override void LoopAction()
		{
			_action();
			_done = true;
		}



		private bool _done = false;
		private readonly Action _action;
	}
}
