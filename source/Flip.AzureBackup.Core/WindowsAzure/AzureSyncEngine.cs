using System;
using System.Collections.Generic;
using System.Linq;
using Flip.AzureBackup.Actions;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.Messages;
using Flip.AzureBackup.Providers;
using Flip.Common.Messages;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.WindowsAzure
{
	public class AzureSyncEngine : ISyncEngine
	{
		public AzureSyncEngine(ILog log, IFileSystem fileSystem, ICloudBlobStorage storage, IMessageBus messageBus)
		{
			this._log = log;
			this._fileSystem = fileSystem;
			this._blobStorage = storage;
			this._messageBus = messageBus;

			messageBus.Subscribe<SyncStateChangedMessage>(message => _running = message.Running);
		}



		public void RegisterPauseHandler()
		{
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

				this._log.WriteLine(provider.Description);
				this._log.WriteLine("");

				Queue<ISyncAction> actions = GetActions(settings.DirectoryPath, blobContainer, provider);

				while (actions.Count > 0)
				{
					ISyncAction action = actions.Dequeue();
					action.Invoke();
				}

				this._log.WriteLine("Done...");
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

			provider.WriteStatistics(_log);

			return actions;
		}

		private ISyncronizationProvider GetProvider(SyncAction action)
		{
			//TODO - Place in container?
			switch (action)
			{
				case SyncAction.Download:
					return new DownloadDeleteSyncronizationProvider(_messageBus, _fileSystem, _blobStorage);
				case SyncAction.DownloadKeep:
					return new DownloadKeepSyncronizationProvider(_messageBus, _fileSystem, _blobStorage);
				case SyncAction.Upload:
					return new UploadSyncronizationProvider(_messageBus, _fileSystem, _blobStorage);
				default:
					return new UploadAnalysisSyncronizationProvider(this._fileSystem);
			}
		}



		private bool _running = true;
		private readonly ILog _log;
		private readonly IFileSystem _fileSystem;
		private readonly ICloudBlobStorage _blobStorage;
		private readonly IMessageBus _messageBus;
		private static readonly BlobRequestOptions blobRequestOptions = new BlobRequestOptions
		{
			UseFlatBlobListing = true,
			BlobListingDetails = BlobListingDetails.Metadata
		};
	}
}