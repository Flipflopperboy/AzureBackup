using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.Messages;
using Flip.AzureBackup.WindowsAzure.Tasks;
using Flip.Common.Messages;
using Flip.Common.Threading;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.WindowsAzure.Providers
{
	public class DownloadKeepSyncronizationProvider : ISyncronizationProvider
	{
		public DownloadKeepSyncronizationProvider(IMessageBus messageBus, IFileSystem fileSystem)
		{
			this._messageBus = messageBus;
			this._fileSystem = fileSystem;
			this._statistics = new SyncronizationStatistics();
		}



		public virtual string Description
		{
			get { return "Download - Keep Deleted Files"; }
		}

		public virtual void WriteStatistics(ILog log)
		{
			log.WriteLine("");
			log.WriteFixedLine('-');
			log.WriteFixedLine("New files:", this._statistics.FileNotExistCount);
			log.WriteFixedLine("Updated files:", this._statistics.UpdatedCount);
			log.WriteFixedLine("Updated file dates:", this._statistics.UpdatedModifiedDateCount);
			log.WriteFixedLine('-');
			log.WriteLine("");
		}

		public bool InitializeDirectory(string path)
		{
			this._fileSystem.CreateDirectoryIfNotExists(path);
			return true;
		}

		public TaskRunner CreateUpdateTaskRunner(CloudBlob blob, FileInformation fileInfo)
		{
			this._statistics.UpdatedCount++;
			return new SingleActionTaskRunner(() =>
			{
				_messageBus.Publish(new ActionProgressedMessage(fileInfo.FullPath, 0));
				blob.DownloadToFile(fileInfo.FullPath);
				_fileSystem.SetLastWriteTimeUtc(fileInfo.FullPath, blob.GetFileLastModifiedUtc());
				_messageBus.Publish(new ActionProgressedMessage(fileInfo.FullPath, 1));
			});
		}

		public TaskRunner CreateUpdateModifiedDateTaskRunner(CloudBlob blob, FileInformation fileInfo)
		{
			this._statistics.UpdatedModifiedDateCount++;
			return new SingleActionTaskRunner(() =>
			{
				_messageBus.Publish(new ActionProgressedMessage(fileInfo.FullPath, 0));
				_fileSystem.SetLastWriteTimeUtc(fileInfo.FullPath, blob.GetFileLastModifiedUtc());
				_messageBus.Publish(new ActionProgressedMessage(fileInfo.FullPath, 1));
			});
		}

		public virtual TaskRunner CreateBlobNotExistsTaskRunner(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			return new EmptyTaskRunner();
		}

		public TaskRunner CreateFileNotExistsTaskRunner(CloudBlob blob, string basePath)
		{
			this._statistics.FileNotExistCount++;
			string relativePath = blob.GetRelativeFilePath();
			string fullFilePath = this._fileSystem.Combine(basePath, relativePath);

			if (blob.Properties.Length > CloudBlobConstants.FileSizeThresholdInBytes)
			{
				return new DownloadLargeFileTaskRunner(_messageBus, _fileSystem, blob, fullFilePath);
			}
			else
			{
				return new SingleActionTaskRunner(() =>
				{
					_messageBus.Publish(new ActionProgressedMessage(fullFilePath, 0));
					_fileSystem.EnsureFileDirectory(fullFilePath);
					blob.DownloadToFile(fullFilePath);
					_fileSystem.SetLastWriteTimeUtc(fullFilePath, blob.GetFileLastModifiedUtc());
					_messageBus.Publish(new ActionProgressedMessage(fullFilePath, 1));
				});
			}
		}



		protected readonly IMessageBus _messageBus;
		protected readonly IFileSystem _fileSystem;
		protected SyncronizationStatistics _statistics;
	}
}
