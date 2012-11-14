using Flip.AzureBackup.IO;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class DeleteBlobSyncAction : ISyncAction
	{
		public DeleteBlobSyncAction(CloudBlob blob)
		{
			_blob = blob;
		}

		public void Invoke()
		{
			//this._statistics.FileNotExistCount++;
			//this._logger.WriteLine("Deleting blob " + blob.Uri.ToString() + "...");
			_blob.DeleteIfExists();
		}

		private readonly CloudBlob _blob;
	}
}
