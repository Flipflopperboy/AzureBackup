using Flip.AzureBackup.IO;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class CreateFileSyncAction : ISyncAction
	{
		public CreateFileSyncAction(IFileSystem fileSystem, ICloudBlobStorage blobStorage, string basePath, CloudBlob blob)
		{
			_fileSystem = fileSystem;
			_blobStorage = blobStorage;
			_basePath = basePath;
			_blob = blob;
		}

		public void Invoke()
		{
			//this._statistics.FileNotExistCount++;

			string relativePath = _blob.GetRelativeFilePath();
			string fullPath = this._fileSystem.Combine(_basePath, relativePath);

			//this._logger.WriteLine("Downloading new file " + fullPath + "...");
			_fileSystem.EnsureFileDirectory(fullPath);

			_blobStorage.DownloadFile(_blob, fullPath);
		}

		private readonly IFileSystem _fileSystem;
		private readonly ICloudBlobStorage _blobStorage;
		private readonly string _basePath;
		private readonly CloudBlob _blob;
	}
}
