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
				this._logger.WriteLine("");

				Synchronize(settings.DirectoryPath, files, blobContainer, provider);

				this._logger.WriteLine("Done...");
			}
			else
			{
				this._logger.WriteLine("Invalid cloud connection string '" + settings.CloudConnectionString + "'.");
			}
		}

		private void Synchronize(
			string directoryPath,
			List<FileInformation> files,
			CloudBlobContainer blobContainer,
			ISyncronizationProvider provider)
		{
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
							provider.HandleUpdateModifiedDate(blob, fileInfo);
						}
						else
						{
							provider.HandleUpdate(blob, fileInfo);
						}
					}
					blobs.Remove(blobUri);
				}
				else
				{
					provider.HandleBlobNotExists(blobContainer, fileInfo);
				}
			}

			foreach (var item in blobs)
			{
				provider.HandleFileNotExists(item.Value, directoryPath);
			}

			provider.WriteStatistics();
		}

		private ISyncronizationProvider GetProvider(SynchronizationAction action)
		{
			switch (action)
			{
				case SynchronizationAction.DownloadKeep:
					return new DownloadKeepSyncronizationProvider(this._logger, this._fileAccessor);
				case SynchronizationAction.DownloadDelete:
					return new DownloadDeleteSyncronizationProvider(this._logger, this._fileAccessor);
				case SynchronizationAction.Upload:
					return new UploadSyncronizationProvider(this._logger, this._fileAccessor);
				default:
					return new UploadAnalysisSyncronizationProvider(this._logger, this._fileAccessor);
			}
		}



		private readonly ILogger _logger;
		private readonly IFileAccessor _fileAccessor;
		private static readonly BlobRequestOptions blobRequestOptions = new BlobRequestOptions
		{
			UseFlatBlobListing = true,
			BlobListingDetails = BlobListingDetails.Metadata
		};
	}
}