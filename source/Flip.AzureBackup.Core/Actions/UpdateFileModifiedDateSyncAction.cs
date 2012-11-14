using Flip.AzureBackup.IO;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class UpdateFileModifiedDateSyncAction : SyncAction
	{
		public UpdateFileModifiedDateSyncAction(IFileSystem fileSystem, FileInformation fileInfo, CloudBlob blob)
		{
			_fileSystem = fileSystem;
			_fileInfo = fileInfo;
			_blob = blob;
		}

		public override void Invoke()
		{
			ReportProgress(_fileInfo.FullPath, "Updating file modification date...", 0);
			_fileSystem.SetLastWriteTimeUtc(_fileInfo.FullPath, _blob.GetFileLastModifiedUtc());
			ReportProgress(_fileInfo.FullPath, "Updated file modification date.", 1);
		}



		private readonly IFileSystem _fileSystem;
		private readonly FileInformation _fileInfo;
		private readonly CloudBlob _blob;
	}
}
