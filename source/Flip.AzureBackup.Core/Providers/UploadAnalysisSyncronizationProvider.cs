using System.Collections.Generic;
using Flip.AzureBackup.Actions;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public class UploadAnalysisSyncronizationProvider : ISyncronizationProvider
	{
		public UploadAnalysisSyncronizationProvider(ILogger logger, IFileSystem fileSystem)
		{
			this._logger = logger;
			this._fileSystem = fileSystem;
			this._statistics = new SyncronizationStatistics();
		}



		public string Description
		{
			get { return "Analysis"; }
		}

		//public void WriteStatistics()
		//{
		//	this._logger.WriteLine("");
		//	this._logger.WriteFixedLine('-');
		//	this._logger.WriteFixedLine("New blobs:", this._statistics.BlobNotExistCount);
		//	this._logger.WriteFixedLine("Blobs updated:", this._statistics.UpdatedCount);
		//	this._logger.WriteFixedLine("Blob dates updated:", this._statistics.UpdatedModifiedDateCount);
		//	this._logger.WriteFixedLine("Blobs deleted:", this._statistics.FileNotExistCount);
		//	this._logger.WriteFixedLine('-');
		//	this._logger.WriteLine("");
		//}

		public bool InitializeDirectory(string path)
		{
			if (!this._fileSystem.DirectoryExists(path))
			{
				this._logger.WriteLine("Directory does not exist '" + path + "'.");
				return false;
			}
			return true;
		}

		public ISyncAction CreateUpdateSyncAction(CloudBlob blob, FileInformation fileInfo)
		{
			return new EmptySyncAction();
		}

		public ISyncAction CreateUpdateModifiedDateSyncAction(CloudBlob blob, FileInformation fileInfo)
		{
			return new EmptySyncAction();
		}

		public ISyncAction CreateBlobNotExistsSyncAction(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			return new EmptySyncAction();
		}

		public ISyncAction CreateFileNotExistsSyncAction(CloudBlob blob, string basePath)
		{
			return new EmptySyncAction();
		}



		private readonly ILogger _logger;
		private readonly IFileSystem _fileSystem;
		private readonly SyncronizationStatistics _statistics;
	}
}
