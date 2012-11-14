using Flip.AzureBackup.IO;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class UpdateBlobModifiedDateSyncAction : ISyncAction
	{
		public UpdateBlobModifiedDateSyncAction(FileInformation fileInfo, CloudBlob blob)
		{
			_fileInfo = fileInfo;
			_blob = blob;
		}

		public void Invoke()
		{
			//this._statistics.UpdatedModifiedDateCount++;
			//this._logger.WriteLine("Updating modification date for blob " + blob.Uri.ToString() + "...");
			_blob.SetFileLastModifiedUtc(_fileInfo.LastWriteTimeUtc, true);
		}

		private readonly FileInformation _fileInfo;
		private readonly CloudBlob _blob;
	}
}
