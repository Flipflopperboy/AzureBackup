namespace Flip.AzureBackup.Providers
{
	public sealed class SyncronizationStatistics
	{
		public int BlobNotExistCount { get; set; }
		public int UpdatedCount { get; set; }
		public int FileNotExistCount { get; set; }
		public int UpdatedModifiedDateCount { get; set; }
	}
}
