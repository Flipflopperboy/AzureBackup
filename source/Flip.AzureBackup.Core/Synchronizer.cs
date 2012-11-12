using System;
using System.Collections.Generic;
using System.Linq;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.Providers;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup
{
	public class Synchronizer : ISynchronizer
	{
		public Synchronizer(ILogger logger, IFileAccessor fileAccessor)
		{
			this._logger = logger;
			this._fileAccessor = fileAccessor;
		}



		public void Synchronize(SyncronizationSettings settings)
		{
			ISyncronizationProvider provider = GetProvider(settings.Action);

			if (!provider.InitializeDirectory(settings.DirectoryPath))
			{
				return;
			}

			List<FileInformation> files = this._fileAccessor
				.GetFileInfoIncludingSubDirectories(settings.DirectoryPath).ToList();

			if (!provider.NeedToCheckCloud(files))
			{
				return;
			}

			CloudStorageAccount cloudStorageAccount;
			if (CloudStorageAccount.TryParse(settings.CloudConnectionString, out cloudStorageAccount))
			{
				CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
				CloudBlobContainer blobContainer = blobClient.GetContainerReference(settings.ContainerName);

				blobContainer.CreateIfNotExist();

				provider.WriteStart();

				SynchronizeToCloud(settings.DirectoryPath, files, blobContainer, provider);

				this._logger.WriteLine("Done...");
			}
			else
			{
				this._logger.WriteLine("Invalid cloud connection string '" + settings.CloudConnectionString + "'.");
			}			
		}

		private void SynchronizeToCloud(
			string directoryPath,
			List<FileInformation> files,
			CloudBlobContainer blobContainer,
			ISyncronizationProvider provider)
		{
			var statistics = new SyncronizationStatistics();

			Dictionary<Uri, CloudBlob> blobs = blobContainer.ListBlobs(blobRequestOptions)
				.Cast<CloudBlob>()
				.ToDictionary(blob => blob.Uri, blob => blob);

			foreach (var fileInfo in files)
			{
				var blobUri = blobContainer.GetBlobUri(fileInfo.RelativePath);
				if (blobs.ContainsKey(blobUri))
				{
					CloudBlob blob = blobs[blobUri];
					if (provider.HasBeenModified(blob, fileInfo))
					{
						string contentMD5 = this._fileAccessor.GetMD5HashForFile(fileInfo.FullPath);
						if (contentMD5 == blob.Properties.ContentMD5)
						{
							statistics.UpdatedModifiedDateCount++;
							provider.HandleUpdateModifiedDate(blob, fileInfo);
						}
						else
						{
							statistics.UpdatedFileCount++;
							provider.HandleUpdate(blob, fileInfo);
						}
					}

					blobs.Remove(blobUri);
				}
				else
				{
					statistics.NewFileCount++;
					provider.HandleBlobNotExists(blobContainer, fileInfo);
				}
			}

			statistics.DeletedFileCount = blobs.Count;

			foreach (var item in blobs)
			{
				provider.HandleFileNotExists(item.Value, directoryPath);
			}

			WriteStatistics(statistics);
		}

		private ISyncronizationProvider GetProvider(SynchronizationAction action)
		{
			switch (action)
			{
				case SynchronizationAction.Download:
					return new DownloadSyncronizationProvider(this._logger, this._fileAccessor);
				case SynchronizationAction.Upload:
					return new UploadSyncronizationProvider(this._logger, this._fileAccessor);
				default:
					return new AnalysisSyncronizationProvider(this._logger, this._fileAccessor);
			}
		}

		private void WriteStatistics(SyncronizationStatistics statistics)
		{
			int length = 30;
			this._logger.WriteLine("");
			this._logger.WriteLine("".PadLeft(length, '-'));
			WriteFixedLine("New files:", statistics.NewFileCount, length);
			WriteFixedLine("Modified files:", statistics.UpdatedFileCount, length);
			WriteFixedLine("Changed date files:", statistics.UpdatedModifiedDateCount, length);
			WriteFixedLine("Deleted files:", statistics.DeletedFileCount, length);
			this._logger.WriteLine("".PadLeft(length, '-'));
			this._logger.WriteLine("");
		}

		private void WriteFixedLine(string s, int n, int length)
		{
			this._logger.WriteLine(s + n.ToString().PadLeft(length - s.Length, ' '));
		}



		private readonly ILogger _logger;
		private readonly IFileAccessor _fileAccessor;
		private static readonly BlobRequestOptions blobRequestOptions = new BlobRequestOptions
		{
			UseFlatBlobListing = true,
			BlobListingDetails = BlobListingDetails.Metadata
		};
		private class SyncronizationStatistics
		{
			public int NewFileCount { get; set; }
			public int UpdatedFileCount { get; set; }
			public int DeletedFileCount { get; set; }
			public int UpdatedModifiedDateCount { get; set; }
		}
	}
}