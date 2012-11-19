using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.Messages;
using Flip.AzureBackup.WindowsAzure.Tasks;
using Flip.Common.Messages;
using Flip.Common.Threading;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.WindowsAzure.Providers
{
	public sealed class DownloadDeleteSyncronizationProvider : DownloadKeepSyncronizationProvider
	{
		public DownloadDeleteSyncronizationProvider(IMessageBus messageBus, IFileSystem fileSystem)
			: base(messageBus, fileSystem)
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

		public override TaskRunner CreateBlobNotExistsTaskRunner(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			this._statistics.BlobNotExistCount++;
			return new SingleActionTaskRunner(() =>
			{
				_messageBus.Publish(new ActionProgressedMessage(fileInfo.FullPath, 0));
				_fileSystem.DeleteFile(fileInfo.FullPath);
				_messageBus.Publish(new ActionProgressedMessage(fileInfo.FullPath, 1));
			});
		}
	}
}
