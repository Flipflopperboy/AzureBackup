using System;



namespace Flip.AzureBackup.Actions
{
	public abstract class SyncAction : ISyncAction
	{
		public event EventHandler<ActionProgressEventArgs> Progress;

		public abstract void Invoke();

		protected void ReportProgress(string fileFullPath, string message, decimal fraction)
		{
			if (Progress != null)
			{
				Progress(this, new ActionProgressEventArgs(fileFullPath, message, fraction));
			}
		}
	}
}
