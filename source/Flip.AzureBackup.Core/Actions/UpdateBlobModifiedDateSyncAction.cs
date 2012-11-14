using Flip.AzureBackup.IO;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class UpdateBlobModifiedDateSyncAction : SyncAction
	{
		public UpdateBlobModifiedDateSyncAction(FileInformation fileInfo, CloudBlob blob)
		{
			_fileInfo = fileInfo;
			_blob = blob;
		}

		public override void Invoke()
		{
			ReportProgress(_fileInfo.FullPath, "Updating blob modification date...", 0);
			_blob.SetFileLastModifiedUtc(_fileInfo.LastWriteTimeUtc, true);
			ReportProgress(_fileInfo.FullPath, "Updated blob modification date.", 1);
		}



		private readonly FileInformation _fileInfo;
		private readonly CloudBlob _blob;
	}
}
