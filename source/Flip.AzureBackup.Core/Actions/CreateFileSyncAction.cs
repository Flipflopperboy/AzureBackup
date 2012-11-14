using Flip.AzureBackup.IO;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class CreateFileSyncAction : SyncAction
	{
		public CreateFileSyncAction(IFileSystem fileSystem, ICloudBlobStorage blobStorage, string fileFullPath, CloudBlob blob)
		{
			_fileSystem = fileSystem;
			_blobStorage = blobStorage;
			_fileFullPath = fileFullPath;
			_blob = blob;
		}

		public override void Invoke()
		{
			ReportProgress(_fileFullPath, "Downloading new file...", 0);
			_fileSystem.EnsureFileDirectory(_fileFullPath);
			_blobStorage.DownloadFile(_blob, _fileFullPath, fraction => ReportProgress(_fileFullPath, "", fraction));
			_fileSystem.SetLastWriteTimeUtc(_fileFullPath, _blob.GetFileLastModifiedUtc());
		}



		private readonly IFileSystem _fileSystem;
		private readonly ICloudBlobStorage _blobStorage;
		private readonly string _fileFullPath;
		private readonly CloudBlob _blob;
	}
}
