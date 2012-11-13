using System;
using System.Collections.Generic;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public class DownloadKeepSyncronizationProvider : ISyncronizationProvider
	{
		public DownloadKeepSyncronizationProvider(ILogger logger, IFileAccessor fileAccessor)
		{
			this._logger = logger;
			this._fileAccessor = fileAccessor;
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
			this._fileAccessor.CreateDirectoryIfNotExists(path);
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
			this._fileAccessor.SetLastWriteTimeUtc(fileInfo.FullPath, blob.GetFileLastModifiedUtc());
		}

		public virtual void HandleBlobNotExists(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			this._logger.WriteLine("File no longer in cloud " + fileInfo.FullPath + "...");
		}

		public void HandleFileNotExists(CloudBlob blob, string basePath)
		{
			this._statistics.FileNotExistCount++;

			Uri relativeUri = blob.GetRelativeFileUri();
			Uri baseUri = new Uri(basePath);
			Uri fullUri = new Uri(baseUri, relativeUri);
			string fullPath = fullUri.LocalPath;

			this._logger.WriteLine("Downloading new file " + fullPath + "...");
			this._fileAccessor.EnsureFileDirectory(fullPath);

			blob.DownloadToFile(fullPath);
		}



		protected readonly ILogger _logger;
		protected readonly IFileAccessor _fileAccessor;
		protected SyncronizationStatistics _statistics;
	}
}
