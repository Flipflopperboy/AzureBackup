using Flip.AzureBackup.Actions;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.WindowsAzure;
using Flip.Common.Messages;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public sealed class DownloadDeleteSyncronizationProvider : DownloadKeepSyncronizationProvider
	{
		public DownloadDeleteSyncronizationProvider(IMessageBus messageBus, IFileSystem fileSystem, ICloudBlobStorage storage)
			: base(messageBus, fileSystem, storage)
		{
		}



		public override string Description
		{
			get { return "Download - Delete Deleted Files"; }
		}

		public override void WriteStatistics(ILog log)
		{
			log.WriteLine("");
			log.WriteFixedLine('-');
			log.WriteFixedLine("New files:", this._statistics.FileNotExistCount);
			log.WriteFixedLine("Updated files:", this._statistics.UpdatedCount);
			log.WriteFixedLine("Updated file dates:", this._statistics.UpdatedModifiedDateCount);
			log.WriteFixedLine("Deleted files:", this._statistics.BlobNotExistCount);
			log.WriteFixedLine('-');
			log.WriteLine("");
		}

		public override ISyncAction CreateBlobNotExistsSyncAction(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			this._statistics.BlobNotExistCount++;
			return new DeleteFileSyncAction(_messageBus, _fileSystem, fileInfo);
		}
	}
}
