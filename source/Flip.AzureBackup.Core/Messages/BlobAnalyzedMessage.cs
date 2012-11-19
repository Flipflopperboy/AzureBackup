using Flip.Common.Messages;



namespace Flip.AzureBackup.Messages
{
	public sealed class BlobAnalyzedMessage : MessageBase
	{
		public BlobAnalyzedMessage(string fileRelativePath, int number, int total)
		{
			this.FileRelativePath = fileRelativePath;
			this.Number = number;
			this.Total = total;
		}



		public string FileRelativePath { get; private set; }
		public int Number { get; private set; }
		public int Total { get; private set; }
	}
}
