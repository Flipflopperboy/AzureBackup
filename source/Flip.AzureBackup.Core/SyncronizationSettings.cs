namespace Flip.AzureBackup
{
	public class SyncronizationSettings
	{
		public string ContainerName { get; set; }
		public string DirectoryPath { get; set; }
		public string CloudConnectionString { get; set; }
		public SynchronizationAction Action { get; set; }
	}
}