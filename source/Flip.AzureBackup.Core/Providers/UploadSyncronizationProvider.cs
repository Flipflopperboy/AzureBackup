using System.Collections.Generic;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public sealed class UploadSyncronizationProvider : ISyncronizationProvider
	{
		public UploadSyncronizationProvider(ILogger logger, IFileAccessor fileAccessor)
		{
			this._logger = logger;
			this._fileAccessor = fileAccessor;
			this._statistics = new SyncronizationStatistics();
		}



		public void WriteStart()
		{
			this._logger.WriteLine("UPLOAD");
		}

		public void WriteStatistics()
		{
			this._logger.WriteLine("");
			this._logger.WriteFixedLine('-');
			this._logger.WriteFixedLine("New blobs:", this._statistics.BlobNotExistCount);
			this._logger.WriteFixedLine("Blobs updated:", this._statistics.UpdatedCount);
			this._logger.WriteFixedLine("Blob dates updated:", this._statistics.UpdatedModifiedDateCount);
			this._logger.WriteFixedLine("Blobs deleted:", this._statistics.FileNotExistCount);
			this._logger.WriteFixedLine('-');
			this._logger.WriteLine("");
		}

		public bool InitializeDirectory(string path)
		{
			if (!this._fileAccessor.DirectoryExists(path))
			{
				this._logger.WriteLine("Directory does not exist '" + path + "'.");
				return false;
			}
			return true;
		}

		public bool NeedToCheckCloud(List<FileInformation> files)
		{
			if (files.Count == 0)
			{
				this._logger.WriteLine("No files to process...");
				return false;
			}
			return true;
		}

		public bool HasBeenModified(CloudBlob blob, FileInformation fileInfo)
		{
			return fileInfo.LastWriteTimeUtc > blob.GetFileLastModifiedUtc();
		}

		public void HandleUpdate(CloudBlob blob, FileInformation fileInfo)
		{
			this._statistics.UpdatedCount++;

			this._logger.WriteLine("Updating blob " + blob.Uri.ToString() + "...");
			blob.UploadFile(fileInfo);
		}

		public void HandleUpdateModifiedDate(CloudBlob blob, FileInformation fileInfo)
		{
			this._statistics.UpdatedModifiedDateCount++;

			this._logger.WriteLine("Updating modification date for blob " + blob.Uri.ToString() + "...");
			blob.SetFileLastModifiedUtc(fileInfo.LastWriteTimeUtc, true);
		}

		public void HandleBlobNotExists(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			this._statistics.BlobNotExistCount++;

			CloudBlob blob = blobContainer.GetBlobReference(fileInfo.RelativePath);
			this._logger.WriteLine("Uploading file " + fileInfo.FullPath + "...");
			blob.UploadFile(fileInfo);
		}

		public void HandleFileNotExists(CloudBlob blob, string basePath)
		{
			this._statistics.FileNotExistCount++;

			this._logger.WriteLine("Deleting blob " + blob.Uri.ToString() + "...");
			blob.DeleteIfExists();
		}



		private readonly ILogger _logger;
		private readonly IFileAccessor _fileAccessor;
		private readonly SyncronizationStatistics _statistics;
	}
}
