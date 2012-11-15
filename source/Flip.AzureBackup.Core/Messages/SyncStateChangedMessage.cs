using Flip.Common.Messages;



namespace Flip.AzureBackup.Messages
{
	public class SyncStateChangedMessage : MessageBase
	{
		public SyncStateChangedMessage(bool running)
		{
			this.Running = running;
		}



		public bool Running { get; private set; }
	}
}
