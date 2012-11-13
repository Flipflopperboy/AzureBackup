using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public sealed class DownloadDeleteSyncronizationProvider : DownloadKeepSyncronizationProvider
	{
		public DownloadDeleteSyncronizationProvider(ILogger logger, IFileSystem fileAccessor, ICloudBlobStorage storage)
			: base(logger, fileAccessor, storage)
		{
		}



		public override void WriteStart()
		{
			this._logger.WriteLine("DOWNLOAD AND DELETE REMOVED FILES");
		}

		public override void WriteStatistics()
		{
			this._logger.WriteLine("");
			this._logger.WriteFixedLine('-');
			this._logger.WriteFixedLine("New files:", this._statistics.FileNotExistCount);
			this._logger.WriteFixedLine("Updated files:", this._statistics.UpdatedCount);
			this._logger.WriteFixedLine("Updated file dates:", this._statistics.UpdatedModifiedDateCount);
			this._logger.WriteFixedLine("Deleted files:", this._statistics.BlobNotExistCount);
			this._logger.WriteFixedLine('-');
			this._logger.WriteLine("");
		}

		public override void HandleBlobNotExists(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			this._statistics.BlobNotExistCount++;

			this._logger.WriteLine("Deleting file " + fileInfo.FullPath + "...");
			this._fileAccessor.DeleteFile(fileInfo.FullPath);
		}
	}
}
