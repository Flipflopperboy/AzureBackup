using System;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public sealed class UploadSyncronizationProvider : ISyncronizationProvider
	{
		public UploadSyncronizationProvider(ILogger logger)
		{
			this._logger = logger;
		}



		public void WriteStart()
		{
			this._logger.WriteLine("UPLOAD");
		}



		public bool HasBeenModified(CloudBlob blob, FileInformation fileInfo)
		{
			return fileInfo.LastWriteTimeUtc > blob.GetFileLastModifiedUtc();
		}

		public void HandleUpdate(CloudBlob blob, FileInformation fileInfo)
		{
			this._logger.WriteLine("Updating blob " + blob.Uri.ToString() + "...");
			blob.UploadFile(fileInfo.FullPath, fileInfo.LastWriteTimeUtc);
		}

		public void HandleUpdateModifiedDate(CloudBlob blob, FileInformation fileInfo)
		{
			this._logger.WriteLine("Updating modification date for blob " + blob.Uri.ToString() + "...");
			blob.SetFileLastModifiedUtc(fileInfo.LastWriteTimeUtc, true);
		}

		public void HandleBlobNotExists(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			CloudBlob blob = blobContainer.GetBlobReference(fileInfo.FullPath);
			this._logger.WriteLine("Uploading file " + fileInfo.FullPath + " to blob " + blob.Uri.ToString() + "...");
			blob.UploadFile(fileInfo.FullPath, fileInfo.LastWriteTimeUtc);
		}

		public void HandleFileNotExists(CloudBlob blob)
		{
			this._logger.WriteLine("Deleting blob " + blob.Uri.ToString() + "...");
			blob.DeleteIfExists();
		}



		private readonly ILogger _logger;
	}
}
