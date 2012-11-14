namespace Flip.AzureBackup
{
	public class AzureSyncSettings
	{
		public string ContainerName { get; set; }
		public string DirectoryPath { get; set; }
		public string CloudConnectionString { get; set; }
		public AzureSyncAction Action { get; set; }
	}
}