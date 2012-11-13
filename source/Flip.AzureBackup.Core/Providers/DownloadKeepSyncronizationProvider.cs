using System;
using System.Collections.Generic;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public class DownloadKeepSyncronizationProvider : ISyncronizationProvider
	{
		public DownloadKeepSyncronizationProvider(ILogger logger, IFileSystem fileAccessor, ICloudBlobStorage storage)
		{
			this._logger = logger;
			this._fileSystem = fileAccessor;
			this._storage = storage;
			this._statistics = new SyncronizationStatistics();
		}



		public virtual void WriteStart()
		{
			this._logger.WriteLine("DOWNLOAD BUT KEEP REMOVED FILES");
		}

		public virtual void WriteStatistics()
		{
			this._logger.WriteLine("");
			this._logger.WriteFixedLine('-');
			this._logger.WriteFixedLine("New files:", this._statistics.FileNotExistCount);
			this._logger.WriteFixedLine("Updated files:", this._statistics.UpdatedCount);
			this._logger.WriteFixedLine("Updated file dates:", this._statistics.UpdatedModifiedDateCount);
			this._logger.WriteFixedLine('-');
			this._logger.WriteLine("");
		}

		public bool InitializeDirectory(string path)
		{
			this._fileSystem.CreateDirectoryIfNotExists(path);
			return true;
		}

		public bool NeedToCheckCloud(List<FileInformation> files)
		{
			return true;
		}

		public bool HasBeenModified(CloudBlob blob, FileInformation fileInfo)
		{
			return fileInfo.LastWriteTimeUtc < blob.GetFileLastModifiedUtc();
		}

		public void HandleUpdate(CloudBlob blob, FileInformation fileInfo)
		{
			this._statistics.UpdatedCount++;

			this._logger.WriteLine("Downloading updated file " + blob.Uri.ToString() + "...");
			blob.DownloadToFile(fileInfo.FullPath);
		}

		public void HandleUpdateModifiedDate(CloudBlob blob, FileInformation fileInfo)
		{
			this._statistics.UpdatedModifiedDateCount++;

			this._logger.WriteLine("Updating modification date for file " + fileInfo.FullPath + "...");
			this._fileSystem.SetLastWriteTimeUtc(fileInfo.FullPath, blob.GetFileLastModifiedUtc());
		}

		public virtual void HandleBlobNotExists(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			this._logger.WriteLine("File no longer in cloud " + fileInfo.FullPath + "...");
		}

		public void HandleFileNotExists(CloudBlob blob, string basePath)
		{
			this._statistics.FileNotExistCount++;

			string relativePath = blob.GetRelativeFilePath();
			string fullPath = this._fileSystem.Combine(basePath, relativePath);

			this._logger.WriteLine("Downloading new file " + fullPath + "...");
			this._fileSystem.EnsureFileDirectory(fullPath);

			this._storage.DownloadFile(blob, fullPath);
		}



		protected readonly ILogger _logger;
		protected readonly IFileSystem _fileSystem;
		protected readonly ICloudBlobStorage _storage;
		protected SyncronizationStatistics _statistics;
	}
}
