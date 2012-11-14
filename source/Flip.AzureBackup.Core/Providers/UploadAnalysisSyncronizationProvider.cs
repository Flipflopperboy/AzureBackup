using System;
using Flip.AzureBackup.Actions;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public class UploadAnalysisSyncronizationProvider : ISyncronizationProvider
	{
		public UploadAnalysisSyncronizationProvider(IFileSystem fileSystem)
		{
			this._fileSystem = fileSystem;
			this._statistics = new SyncronizationStatistics();
		}



		public string Description
		{
			get { return "Analysis"; }
		}

		public void WriteStatistics(ILog logger)
		{
			logger.WriteLine("");
			logger.WriteFixedLine('-');
			logger.WriteFixedLine("New blobs:", this._statistics.BlobNotExistCount);
			logger.WriteFixedLine("Blobs updated:", this._statistics.UpdatedCount);
			logger.WriteFixedLine("Blob dates updated:", this._statistics.UpdatedModifiedDateCount);
			logger.WriteFixedLine("Blobs deleted:", this._statistics.FileNotExistCount);
			logger.WriteFixedLine('-');
			logger.WriteLine("");
		}

		public bool InitializeDirectory(string path)
		{
			if (!this._fileSystem.DirectoryExists(path))
			{
				throw new Exception("Directory does not exist '" + path + "'.");
			}
			return true;
		}

		public ISyncAction CreateUpdateSyncAction(CloudBlob blob, FileInformation fileInfo)
		{
			this._statistics.UpdatedCount++;
			return new EmptySyncAction();
		}

		public ISyncAction CreateUpdateModifiedDateSyncAction(CloudBlob blob, FileInformation fileInfo)
		{
			this._statistics.UpdatedModifiedDateCount++;
			return new EmptySyncAction();
		}

		public ISyncAction CreateBlobNotExistsSyncAction(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			this._statistics.BlobNotExistCount++;
			return new EmptySyncAction();
		}

		public ISyncAction CreateFileNotExistsSyncAction(CloudBlob blob, string basePath)
		{
			this._statistics.FileNotExistCount++;
			return new EmptySyncAction();
		}



		private readonly IFileSystem _fileSystem;
		private readonly SyncronizationStatistics _statistics;
	}
}
