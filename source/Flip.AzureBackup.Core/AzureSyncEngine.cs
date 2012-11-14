using System;
using System.Collections.Generic;
using System.Linq;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.Providers;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup
{
	public class AzureSyncEngine : ISynchronizer
	{
		public AzureSyncEngine(ILogger logger, IFileSystem fileSystem, ICloudBlobStorage storage)
		{
			this._logger = logger;
			this._fileSystem = fileSystem;
			this._storage = storage;
		}



		public void Sync(AzureSyncSettings settings)
		{
			ISyncronizationProvider provider = GetProvider(settings.Action);

			if (!provider.InitializeDirectory(settings.DirectoryPath))
			{
				return;
			}

			List<FileInformation> files = this._fileSystem
				.GetFileInformationIncludingSubDirectories(settings.DirectoryPath).ToList();

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

				Sync(settings.DirectoryPath, files, blobContainer, provider);

				this._logger.WriteLine("Done...");
			}
			else
			{
				this._logger.WriteLine("Invalid cloud connection string '" + settings.CloudConnectionString + "'.");
			}
		}

		private void Sync(
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
				var blobUri = blobContainer.GetBlobUri(fileInfo.BlobName);
				if (blobs.ContainsKey(blobUri))
				{
					CloudBlob blob = blobs[blobUri];
					if (provider.HasBeenModified(blob, fileInfo))
					{
						string contentMD5 = this._fileSystem.GetMD5HashForFile(fileInfo.FullPath);
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

		private ISyncronizationProvider GetProvider(AzureSyncAction action)
		{
			//TODO - Place in container?
			switch (action)
			{
				case AzureSyncAction.Download:
				case AzureSyncAction.DownloadDelete:
					return new DownloadDeleteSyncronizationProvider(this._logger, this._fileSystem, this._storage);
				case AzureSyncAction.DownloadKeep:
					return new DownloadKeepSyncronizationProvider(this._logger, this._fileSystem, this._storage);
				case AzureSyncAction.Upload:
					return new UploadSyncronizationProvider(this._logger, this._fileSystem, this._storage);
				default:
					return new UploadAnalysisSyncronizationProvider(this._logger, this._fileSystem);
			}
		}



		private readonly ILogger _logger;
		private readonly IFileSystem _fileSystem;
		private readonly ICloudBlobStorage _storage;
		private static readonly BlobRequestOptions blobRequestOptions = new BlobRequestOptions
		{
			UseFlatBlobListing = true,
			BlobListingDetails = BlobListingDetails.Metadata
		};
	}
}