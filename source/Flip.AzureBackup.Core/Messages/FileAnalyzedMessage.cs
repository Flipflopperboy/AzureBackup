using Flip.Common.Messages;



namespace Flip.AzureBackup.Messages
{
	public sealed class FileAnalyzedMessage : MessageBase
	{
		public FileAnalyzedMessage(string fullFilePath, int number, int total)
		{
			this.FullFilePath = fullFilePath;
			this.Number = number;
			this.Total = total;
		}



		public string FullFilePath { get; private set; }
		public int Number { get; private set; }
		public int Total { get; private set; }
	}
}
