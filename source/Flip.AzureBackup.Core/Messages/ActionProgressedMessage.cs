using Flip.Common.Messages;



namespace Flip.AzureBackup.Messages
{
	public sealed class ActionProgressedMessage : MessageBase
	{
		public ActionProgressedMessage(string fileFullPath, string message, decimal fraction)
		{
			this.FileFullPath = fileFullPath;
			this.Message = message;
			this.Fraction = fraction;
		}



		public string FileFullPath { get; private set; }
		public string Message { get; private set; }
		public decimal Fraction { get; private set; }
	}
}
