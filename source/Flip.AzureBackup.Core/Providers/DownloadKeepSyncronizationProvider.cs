using Flip.AzureBackup.Actions;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.WindowsAzure;
using Flip.Common.Messages;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public class DownloadKeepSyncronizationProvider : ISyncronizationProvider
	{
		public DownloadKeepSyncronizationProvider(IMessageBus messageBus, IFileSystem fileSystem, ICloudBlobStorage storage)
		{
			this._messageBus = messageBus;
			this._fileSystem = fileSystem;
			this._blobStorage = storage;
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

		public ISyncAction CreateUpdateSyncAction(CloudBlob blob, FileInformation fileInfo)
		{
			this._statistics.UpdatedCount++;
			return new UpdateFileSyncAction(_messageBus, _fileSystem, fileInfo, blob);
		}

		public ISyncAction CreateUpdateModifiedDateSyncAction(CloudBlob blob, FileInformation fileInfo)
		{
			this._statistics.UpdatedModifiedDateCount++;
			return new UpdateFileModifiedDateSyncAction(_messageBus, _fileSystem, fileInfo, blob);
		}

		public virtual ISyncAction CreateBlobNotExistsSyncAction(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			return new EmptySyncAction();
		}

		public ISyncAction CreateFileNotExistsSyncAction(CloudBlob blob, string basePath)
		{
			this._statistics.FileNotExistCount++;
			string relativePath = blob.GetRelativeFilePath();
			string fullPath = this._fileSystem.Combine(basePath, relativePath);
			return new CreateFileSyncAction(_messageBus, _fileSystem, _blobStorage, fullPath, blob);
		}



		protected readonly IMessageBus _messageBus;
		protected readonly IFileSystem _fileSystem;
		protected readonly ICloudBlobStorage _blobStorage;
		protected SyncronizationStatistics _statistics;
	}
}
