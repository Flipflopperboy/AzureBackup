using Flip.AzureBackup.IO;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class UpdateBlobSyncAction : ISyncAction
	{
		public UpdateBlobSyncAction(ICloudBlobStorage blobStorage, FileInformation fileInfo, CloudBlob blob)
		{
			_blobStorage = blobStorage;
			_fileInfo = fileInfo;
			_blob = blob;			
		}

		public void Invoke()
		{
			//this._statistics.UpdatedCount++;
			//this._logger.WriteLine("Updating blob " + blob.Uri.ToString() + "...");
			_blobStorage.UploadFile(_blob, _fileInfo);
		}

		private readonly ICloudBlobStorage _blobStorage;
		private readonly FileInformation _fileInfo;
		private readonly CloudBlob _blob;		
	}
}
