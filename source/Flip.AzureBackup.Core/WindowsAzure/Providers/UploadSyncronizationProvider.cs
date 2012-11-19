using System;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.Messages;
using Flip.AzureBackup.WindowsAzure.Tasks;
using Flip.Common.Messages;
using Flip.Common.Threading;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.WindowsAzure.Providers
{
	public sealed class UploadSyncronizationProvider : ISyncronizationProvider
	{
		public UploadSyncronizationProvider(IMessageBus messageBus, IFileSystem fileSystem)
		{
			this._messageBus = messageBus;
			this._fileSystem = fileSystem;
			this._statistics = new SyncronizationStatistics();
		}



		public string Description
		{
			get { return "Upload"; }
		}

		public void WriteStatistics(ILog log)
		{
			log.WriteLine("");
			log.WriteFixedLine('-');
			log.WriteFixedLine("New blobs:", this._statistics.BlobNotExistCount);
			log.WriteFixedLine("Blobs updated:", this._statistics.UpdatedCount);
			log.WriteFixedLine("Blob dates updated:", this._statistics.UpdatedModifiedDateCount);
			log.WriteFixedLine("Blobs deleted:", this._statistics.FileNotExistCount);
			log.WriteFixedLine('-');
			log.WriteLine("");
		}

		public bool InitializeDirectory(string path)
		{
			if (!this._fileSystem.DirectoryExists(path))
			{
				throw new Exception("Directory does not exist '" + path + "'.");
			}
			return true;
		}

		public TaskRunner CreateUpdateTaskRunner(CloudBlob blob, FileInformation fileInfo)
		{
			this._statistics.UpdatedCount++;
			if (fileInfo.SizeInBytes > CloudBlobConstants.FileSizeThresholdInBytes)
			{
				return new UploadLargeFileTaskRunner(_messageBus, _fileSystem, fileInfo, blob);
			}
			else
			{
				return new SingleActionTaskRunner(() =>
				{
					_messageBus.Publish(new FileProgressedMessage(fileInfo.FullPath, 0));
					blob.UploadFile(fileInfo);
					_messageBus.Publish(new FileProgressedMessage(fileInfo.FullPath, 1));
				});
			}
		}

		public TaskRunner CreateUpdateModifiedDateTaskRunner(CloudBlob blob, FileInformation fileInfo)
		{
			this._statistics.UpdatedModifiedDateCount++;
			return new SingleActionTaskRunner(() =>
			{
				_messageBus.Publish(new FileProgressedMessage(fileInfo.FullPath, 0));
				blob.SetFileLastModifiedUtc(fileInfo.LastWriteTimeUtc, true);
				_messageBus.Publish(new FileProgressedMessage(fileInfo.FullPath, 1));
			});
		}

		public TaskRunner CreateBlobNotExistsTaskRunner(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			this._statistics.BlobNotExistCount++;
			if (fileInfo.SizeInBytes > CloudBlobConstants.FileSizeThresholdInBytes)
			{
				return new UploadLargeFileTaskRunner(_messageBus, _fileSystem, fileInfo, blobContainer);
			}
			else
			{
				return new SingleActionTaskRunner(() =>
				{
					_messageBus.Publish(new FileProgressedMessage(fileInfo.FullPath, 0));
					CloudBlob blob = blobContainer.GetBlobReference(fileInfo.BlobName);
					blob.UploadFile(fileInfo);
					_messageBus.Publish(new FileProgressedMessage(fileInfo.FullPath, 1));
				});
			}
		}

		public TaskRunner CreateFileNotExistsTaskRunner(CloudBlob blob, string basePath)
		{
			this._statistics.FileNotExistCount++;
			return new SingleActionTaskRunner(() =>
			{
				string relativePath = blob.GetRelativeFilePath();
				string fullFilePath = this._fileSystem.Combine(basePath, relativePath);

				_messageBus.Publish(new FileProgressedMessage(fullFilePath, 0));
				blob.DeleteIfExists();
				_messageBus.Publish(new FileProgressedMessage(fullFilePath, 1));
			});
		}



		private readonly IMessageBus _messageBus;
		private readonly IFileSystem _fileSystem;
		private readonly SyncronizationStatistics _statistics;
	}
}
