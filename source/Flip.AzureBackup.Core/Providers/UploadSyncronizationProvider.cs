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



		public void CreateBlob(CloudBlobContainer blobContainer, FileInformation fileInfo, string path)
		{
			CloudBlob blob = blobContainer.GetBlobReference(path);
			blob.UploadFile(path, fileInfo.LastWriteTimeUtc);
		}

		public void UpdateBlob(CloudBlob blob, FileInformation fileInfo, string path)
		{
			blob.UploadFile(path, fileInfo.LastWriteTimeUtc);
		}

		public void UpdateBlobModifiedDate(CloudBlob blob, FileInformation fileInfo)
		{
			blob.SetFileLastModifiedUtc(fileInfo.LastWriteTimeUtc, true);
		}

		public void DeleteBlob(CloudBlob blob)
		{
			blob.DeleteIfExists();
		}



		private readonly ILogger _logger;
	}
}
