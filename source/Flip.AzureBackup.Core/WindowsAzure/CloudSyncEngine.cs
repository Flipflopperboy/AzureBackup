using System;
using System.Collections.Generic;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.WindowsAzure.Providers;
using Flip.AzureBackup.WindowsAzure.Tasks;
using Flip.Common.Messages;
using Flip.Common.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.WindowsAzure
{
	public class CloudSyncEngine : ISyncEngine
	{
		public CloudSyncEngine(ILog log, IFileSystem fileSystem, IMessageBus messageBus)
		{
			this._log = log;
			this._fileSystem = fileSystem;
			this._messageBus = messageBus;
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

				using (var worker = new CreateWorkTaskRunner(_messageBus, _fileSystem, provider, settings.DirectoryPath, blobContainer))
				{
					worker.Start()
						  .Wait();

					Queue<TaskRunner> taskRunners = worker.GetSubTaskRunnerQueue();
					while (taskRunners.Count > 0)
					{
						using (var taskRunner = taskRunners.Dequeue())
						{
							taskRunner
								.Start()
								.Wait();
						}
					}
				}

				this._log.WriteLine("Done...");
			}
			else
			{
				throw new Exception("Invalid cloud connection string '" + settings.CloudConnectionString + "'.");
			}
		}





		private ISyncronizationProvider GetProvider(SyncAction action)
		{
			//TODO - Place in container?
			switch (action)
			{
				case SyncAction.Download:
					return new DownloadDeleteSyncronizationProvider(_messageBus, _fileSystem);
				case SyncAction.DownloadKeep:
					return new DownloadKeepSyncronizationProvider(_messageBus, _fileSystem);
				case SyncAction.Upload:
					return new UploadSyncronizationProvider(_messageBus, _fileSystem);
				default:
					return new UploadAnalysisSyncronizationProvider(this._fileSystem);
			}
		}



		private readonly ILog _log;
		private readonly IFileSystem _fileSystem;
		private readonly IMessageBus _messageBus;

	}
}