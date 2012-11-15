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
			_messageBus.Subscribe<SyncStateChangedMessage>(OnSyncStateChanged);
		}



		public abstract void Invoke();



		protected void ReportProgress(string fileFullPath, string message, decimal fraction)
		{
			_messageBus.Publish(new ActionProgressedMessage(fileFullPath, message, fraction));
		}

		protected virtual void OnSyncStateChanged(SyncStateChangedMessage message)
		{
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_messageBus.Unsubscribe<SyncStateChangedMessage>(OnSyncStateChanged);
			}
			_disposed = true;
		}



		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}



		protected bool _disposed;
		private readonly IMessageBus _messageBus;
	}
}
