using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class DeleteBlobSyncAction : SyncAction
	{
		public DeleteBlobSyncAction(string fileFullPath, CloudBlob blob)
		{
			_fileFullPath = fileFullPath;
			_blob = blob;
		}

		public override void Invoke()
		{
			ReportProgress(_fileFullPath, "Deleting blob...", 0);
			_blob.DeleteIfExists();
			ReportProgress(_fileFullPath, "Deleted blob.", 1);
		}



		private readonly string _fileFullPath;
		private readonly CloudBlob _blob;
	}
}
