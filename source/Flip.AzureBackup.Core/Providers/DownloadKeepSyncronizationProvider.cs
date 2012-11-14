using Flip.AzureBackup.Actions;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public class DownloadKeepSyncronizationProvider : ISyncronizationProvider
	{
		public DownloadKeepSyncronizationProvider(ILogger logger, IFileSystem fileSystem, ICloudBlobStorage storage)
		{
			this._logger = logger;
			this._fileSystem = fileSystem;
			this._blobStorage = storage;
			this._statistics = new SyncronizationStatistics();
		}



		public virtual string Description
		{
			get { return "Download - Keep Deleted Files"; }
		}

		//public virtual void WriteStatistics()
		//{
		//	this._logger.WriteLine("");
		//	this._logger.WriteFixedLine('-');
		//	this._logger.WriteFixedLine("New files:", this._statistics.FileNotExistCount);
		//	this._logger.WriteFixedLine("Updated files:", this._statistics.UpdatedCount);
		//	this._logger.WriteFixedLine("Updated file dates:", this._statistics.UpdatedModifiedDateCount);
		//	this._logger.WriteFixedLine('-');
		//	this._logger.WriteLine("");
		//}

		public bool InitializeDirectory(string path)
		{
			this._fileSystem.CreateDirectoryIfNotExists(path);
			return true;
		}

		public ISyncAction CreateUpdateSyncAction(CloudBlob blob, FileInformation fileInfo)
		{
			return new UpdateFileSyncAction(_fileSystem, fileInfo, blob);
		}

		public ISyncAction CreateUpdateModifiedDateSyncAction(CloudBlob blob, FileInformation fileInfo)
		{
			return new UpdateFileModifiedDateSyncAction(_fileSystem, fileInfo, blob);
		}

		public virtual ISyncAction CreateBlobNotExistsSyncAction(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			return new EmptySyncAction();
		}

		public ISyncAction CreateFileNotExistsSyncAction(CloudBlob blob, string basePath)
		{
			return new CreateFileSyncAction(_fileSystem, _blobStorage, basePath, blob);
		}



		protected readonly ILogger _logger;
		protected readonly IFileSystem _fileSystem;
		protected readonly ICloudBlobStorage _blobStorage;
		protected SyncronizationStatistics _statistics;
	}
}
