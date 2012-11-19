using System;
using System.Collections.Generic;
using System.Linq;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Messages;
using Flip.AzureBackup.WindowsAzure.Providers;
using Flip.Common.Messages;
using Flip.Common.Threading;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.WindowsAzure.Tasks
{
	public class CreateWorkTaskRunner : MessageBasedTaskRunner<SyncStartedMessage, SyncPausedMessage, SyncStoppedMessage>
	{
		public CreateWorkTaskRunner(IMessageBus messageBus, IFileSystem fileSystem, ISyncronizationProvider provider,
			string directoryPath, CloudBlobContainer blobContainer)
			: base(messageBus)
		{
			_fileSystem = fileSystem;
			_directoryPath = directoryPath;
			_blobContainer = blobContainer;
			_provider = provider;
		}



		public Queue<TaskRunner> GetSubTaskRunnerQueue()
		{
			return _tasks;
		}



		protected override void OnFirstStarting()
		{
			base.OnFirstStarting();

			_files = this._fileSystem
				.GetFileInformationIncludingSubDirectories(_directoryPath).ToList();

			_blobs = _blobContainer.ListBlobs()
				.Cast<CloudBlob>()
				.ToDictionary(blob => blob.Uri, blob => blob);

			_tasks = new Queue<TaskRunner>();
		}

		protected override bool LoopCondition()
		{
			if (_fileCounter < _files.Count)
			{
				return true;
			}
			else
			{
				return _blobs.Count > 0;
			}
		}

		protected override void LoopAction()
		{
			if (_fileCounter < _files.Count)
			{
				FileInformation fileInfo = _files[_fileCounter];
				var blobUri = _blobContainer.GetBlobUri(fileInfo.BlobName);
				if (_blobs.ContainsKey(blobUri))
				{
					CloudBlob blob = _blobs[blobUri];
					if (fileInfo.LastWriteTimeUtc != blob.GetFileLastModifiedUtc())
					{
						if (blob.Properties.BlobType == BlobType.PageBlob) //Block blobs don't have ContentMD5 set
						{
							string contentMD5 = this._fileSystem.GetMD5HashForFile(fileInfo.FullPath);
							if (contentMD5 == blob.Properties.ContentMD5)
							{
								_tasks.Enqueue(_provider.CreateUpdateModifiedDateTaskRunner(blob, fileInfo));
							}
							else
							{
								_tasks.Enqueue(_provider.CreateUpdateTaskRunner(blob, fileInfo));
							}
						}
						else
						{
							_tasks.Enqueue(_provider.CreateUpdateTaskRunner(blob, fileInfo));
						}
					}
					_blobs.Remove(blobUri);
				}
				else
				{
					_tasks.Enqueue(_provider.CreateBlobNotExistsTaskRunner(_blobContainer, fileInfo));
				}
				_fileCounter++;
			}
			else
			{
				if (_blobs.Count > 0)
				{
					KeyValuePair<Uri, CloudBlob> pair = _blobs.First();
					_tasks.Enqueue(_provider.CreateFileNotExistsTaskRunner(pair.Value, _directoryPath));
					_blobs.Remove(pair.Key);
				}
			}
		}



		private int _fileCounter;
		private List<FileInformation> _files;
		private Dictionary<Uri, CloudBlob> _blobs;
		private Queue<TaskRunner> _tasks;
		private readonly IFileSystem _fileSystem;
		private readonly string _directoryPath;
		private readonly CloudBlobContainer _blobContainer;
		private readonly ISyncronizationProvider _provider;
		private static readonly BlobRequestOptions blobRequestOptions = new BlobRequestOptions
		{
			UseFlatBlobListing = true,
			BlobListingDetails = BlobListingDetails.Metadata
		};
	}
}
