using System;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public sealed class DownloadSyncronizationProvider : ISyncronizationProvider
	{
		public DownloadSyncronizationProvider(ILogger logger, IFileAccessor fileAccessor)
		{
			this._logger = logger;
			this._fileAccessor = fileAccessor;
		}



		public void WriteStart()
		{
			this._logger.WriteLine("DOWNLOAD");
		}



		public bool HasBeenModified(CloudBlob blob, FileInformation fileInfo)
		{
			return fileInfo.LastWriteTimeUtc < blob.GetFileLastModifiedUtc();
		}

		public void HandleUpdate(CloudBlob blob, FileInformation fileInfo)
		{
			this._logger.WriteLine("Downloading file " + blob.Uri.ToString() + "...");
			blob.DownloadToFile(fileInfo.FullPath);
		}

		public void HandleUpdateModifiedDate(CloudBlob blob, FileInformation fileInfo)
		{
			this._logger.WriteLine("Updating modification date for file " + fileInfo.FullPath + "...");
			this._fileAccessor.SetLastWriteTimeUtc(fileInfo.FullPath, blob.GetFileLastModifiedUtc());
		}

		public void HandleBlobNotExists(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			this._logger.WriteLine("Deleting file " + fileInfo.FullPath + "...");
			this._fileAccessor.DeleteFile(fileInfo.FullPath);
		}

		public void HandleFileNotExists(CloudBlob blob)
		{
			//Download - create
			//blob.DeleteIfExists();
		}



		private readonly ILogger _logger;
		private readonly IFileAccessor _fileAccessor;
	}
}
