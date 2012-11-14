using Flip.AzureBackup.Actions;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public sealed class DownloadDeleteSyncronizationProvider : DownloadKeepSyncronizationProvider
	{
		public DownloadDeleteSyncronizationProvider(ILogger logger, IFileSystem fileSystem, ICloudBlobStorage storage)
			: base(logger, fileSystem, storage)
		{
		}



		public override string Description
		{
			get { return "Download - Delete Deleted Files"; }
		}

		//public override void WriteStatistics()
		//{
		//	this._logger.WriteLine("");
		//	this._logger.WriteFixedLine('-');
		//	this._logger.WriteFixedLine("New files:", this._statistics.FileNotExistCount);
		//	this._logger.WriteFixedLine("Updated files:", this._statistics.UpdatedCount);
		//	this._logger.WriteFixedLine("Updated file dates:", this._statistics.UpdatedModifiedDateCount);
		//	this._logger.WriteFixedLine("Deleted files:", this._statistics.BlobNotExistCount);
		//	this._logger.WriteFixedLine('-');
		//	this._logger.WriteLine("");
		//}

		public override ISyncAction CreateBlobNotExistsSyncAction(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			return new DeleteFileSyncAction(_fileSystem, fileInfo);
		}
	}
}
