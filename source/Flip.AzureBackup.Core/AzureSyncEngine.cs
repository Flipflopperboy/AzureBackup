using System;
using System.Collections.Generic;
using System.Linq;
using Flip.AzureBackup.Actions;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.Providers;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup
{
	public class AzureSyncEngine : ISyncEngine
	{
		public AzureSyncEngine(ILogger logger, IFileSystem fileSystem, ICloudBlobStorage storage)
		{
			this._logger = logger;
			this._fileSystem = fileSystem;
			this._storage = storage;
		}



		public void Sync(SyncSettings settings)
		{
			ISyncronizationProvider provider = GetProvider(settings.Action);

			if (!provider.InitializeDirectory(settings.DirectoryPath))
			{
				return;
			}

			CloudStorageAccount cloudStorageAccount;
			if (CloudStorageAccount.TryParse(settings.CloudConnectionString, out cloudStorageAccount))
			{
				CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
				CloudBlobContainer blobContainer = blobClient.GetContainerReference(settings.ContainerName);

				blobContainer.CreateIfNotExist();

				this._logger.WriteLine(provider.Description);
				this._logger.WriteLine("");

				Queue<ISyncAction> actions = GetActions(settings.DirectoryPath, blobContainer, provider);

				while (actions.Count > 0)
				{
					ISyncAction action = actions.Dequeue();
					action.Progress += OnActionProgress;
					action.Invoke();
				}

				this._logger.WriteLine("Done...");
			}
			else
			{
				throw new Exception("Invalid cloud connection string '" + settings.CloudConnectionString + "'.");
			}
		}



		private Queue<ISyncAction> GetActions(
			string directoryPath,
			CloudBlobContainer blobContainer,
			ISyncronizationProvider provider)
		{
			Queue<string> queue = new Queue<string>();

			List<FileInformation> files = this._fileSystem
				.GetFileInformationIncludingSubDirectories(directoryPath).ToList();

			Dictionary<Uri, CloudBlob> blobs = blobContainer.ListBlobs(blobRequestOptions)
				.Cast<CloudBlob>()
				.ToDictionary(blob => blob.Uri, blob => blob);

			Queue<ISyncAction> actions = new Queue<ISyncAction>();

			foreach (var fileInfo in files)
			{
				var blobUri = blobContainer.GetBlobUri(fileInfo.BlobName);
				if (blobs.ContainsKey(blobUri))
				{
					CloudBlob blob = blobs[blobUri];
					if (fileInfo.LastWriteTimeUtc != blob.GetFileLastModifiedUtc())
					{
						if (blob.Properties.BlobType == BlobType.PageBlob) //Block blobs don't have ContentMD5 set
						{
							string contentMD5 = this._fileSystem.GetMD5HashForFile(fileInfo.FullPath);
							if (contentMD5 == blob.Properties.ContentMD5)
							{
								actions.Enqueue(provider.CreateUpdateModifiedDateSyncAction(blob, fileInfo));
							}
							else
							{
								actions.Enqueue(provider.CreateUpdateSyncAction(blob, fileInfo));
							}
						}
						else
						{
							actions.Enqueue(provider.CreateUpdateSyncAction(blob, fileInfo));
						}
					}
					blobs.Remove(blobUri);
				}
				else
				{
					actions.Enqueue(provider.CreateBlobNotExistsSyncAction(blobContainer, fileInfo));
				}
			}

			foreach (var item in blobs)
			{
				actions.Enqueue(provider.CreateFileNotExistsSyncAction(item.Value, directoryPath));
			}

			provider.WriteStatistics(_logger);

			return actions;
		}

		private ISyncronizationProvider GetProvider(SyncAction action)
		{
			//TODO - Place in container?
			switch (action)
			{
				case SyncAction.Download:
					return new DownloadDeleteSyncronizationProvider(this._fileSystem, this._storage);
				case SyncAction.DownloadKeep:
					return new DownloadKeepSyncronizationProvider(this._fileSystem, this._storage);
				case SyncAction.Upload:
					return new UploadSyncronizationProvider(this._fileSystem, this._storage);
				default:
					return new UploadAnalysisSyncronizationProvider(this._fileSystem);
			}
		}

		private void OnActionProgress(object sender, ActionProgressEventArgs e)
		{
			if (e.Fraction == 0)
			{
				this._logger.WriteLine(e.Message);
				this._logger.WriteLine(e.FileFullPath);
				this._logger.WriteLine("");
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