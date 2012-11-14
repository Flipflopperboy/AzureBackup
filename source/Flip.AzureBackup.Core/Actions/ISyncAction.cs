using System;



namespace Flip.AzureBackup.Actions
{
	public interface ISyncAction
	{
		event EventHandler<ActionProgressEventArgs> Progress;
		void Invoke();
	}
}
