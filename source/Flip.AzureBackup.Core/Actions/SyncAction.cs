using System;
using Flip.AzureBackup.Messages;
using Flip.Common.Messages;



namespace Flip.AzureBackup.Actions
{
	public abstract class SyncAction : ISyncAction, IDisposable
	{
		public SyncAction(IMessageBus messageBus)
		{
			_messageBus = messageBus;
			_messageBus.Subscribe<SyncPausedMessage>(OnSyncStateChanged);
		}



		public abstract void Invoke();



		protected void ReportProgress(string fileFullPath, string message, decimal fraction)
		{
			_messageBus.Publish(new ActionProgressedMessage(fileFullPath, message, fraction));
		}

		protected virtual void OnSyncStateChanged(SyncPausedMessage message)
		{
		}



		void IDisposable.Dispose()
		{
			try
			{
				_messageBus.Unsubscribe<SyncPausedMessage>(OnSyncStateChanged);
			}
			catch { }
			GC.SuppressFinalize(this);
		}



		private readonly IMessageBus _messageBus;
	}
}
