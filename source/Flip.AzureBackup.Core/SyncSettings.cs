namespace Flip.AzureBackup
{
	public class SyncSettings
	{
		public string ContainerName { get; set; }
		public string DirectoryPath { get; set; }
		public string CloudConnectionString { get; set; }
		public SyncAction Action { get; set; }
	}
}