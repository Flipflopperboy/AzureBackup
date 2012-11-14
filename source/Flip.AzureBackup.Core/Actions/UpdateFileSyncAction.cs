using Flip.AzureBackup.IO;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class UpdateFileSyncAction : SyncAction
	{
		public UpdateFileSyncAction(IFileSystem fileSystem, FileInformation fileInfo, CloudBlob blob)
		{
			_fileSystem = fileSystem;
			_fileInfo = fileInfo;
			_blob = blob;
		}

		public override void Invoke()
		{
			ReportProgress(_fileInfo.FullPath, "Downloading updated file...", 0);
			_blob.DownloadToFile(_fileInfo.FullPath);
			_fileSystem.SetLastWriteTimeUtc(_fileInfo.FullPath, _blob.GetFileLastModifiedUtc());
			ReportProgress(_fileInfo.FullPath, "Downloaded updated file.", 1);
		}

		private readonly IFileSystem _fileSystem;
		private readonly FileInformation _fileInfo;
		private readonly CloudBlob _blob;
	}
}
