using Flip.AzureBackup.Actions;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public sealed class DownloadDeleteSyncronizationProvider : DownloadKeepSyncronizationProvider
	{
		public DownloadDeleteSyncronizationProvider(IFileSystem fileSystem, ICloudBlobStorage storage)
			: base(fileSystem, storage)
		{
		}



		public override string Description
		{
			get { return "Download - Delete Deleted Files"; }
		}

		public override void WriteStatistics(ILogger logger)
		{
			logger.WriteLine("");
			logger.WriteFixedLine('-');
			logger.WriteFixedLine("New files:", this._statistics.FileNotExistCount);
			logger.WriteFixedLine("Updated files:", this._statistics.UpdatedCount);
			logger.WriteFixedLine("Updated file dates:", this._statistics.UpdatedModifiedDateCount);
			logger.WriteFixedLine("Deleted files:", this._statistics.BlobNotExistCount);
			logger.WriteFixedLine('-');
			logger.WriteLine("");
		}

		public override ISyncAction CreateBlobNotExistsSyncAction(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			this._statistics.BlobNotExistCount++;
			return new DeleteFileSyncAction(_fileSystem, fileInfo);
		}
	}
}
