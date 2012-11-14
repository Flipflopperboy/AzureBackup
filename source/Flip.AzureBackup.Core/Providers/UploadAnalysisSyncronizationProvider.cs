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

		public void WriteStatistics(ILog log)
		{
			log.WriteLine("");
			log.WriteFixedLine('-');
			log.WriteFixedLine("New blobs:", this._statistics.BlobNotExistCount);
			log.WriteFixedLine("Blobs updated:", this._statistics.UpdatedCount);
			log.WriteFixedLine("Blob dates updated:", this._statistics.UpdatedModifiedDateCount);
			log.WriteFixedLine("Blobs deleted:", this._statistics.FileNotExistCount);
			log.WriteFixedLine('-');
			log.WriteLine("");
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
