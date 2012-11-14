using Flip.AzureBackup.IO;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class UpdateBlobSyncAction : SyncAction
	{
		public UpdateBlobSyncAction(ICloudBlobStorage blobStorage, FileInformation fileInfo, CloudBlob blob)
		{
			_blobStorage = blobStorage;
			_fileInfo = fileInfo;
			_blob = blob;
		}

		public override void Invoke()
		{
			ReportProgress(_fileInfo.FullPath, "Updating blob...", 0);
			_blobStorage.UploadFile(_blob, _fileInfo);
			ReportProgress(_fileInfo.FullPath, "Updated blob.", 1);
		}



		private readonly ICloudBlobStorage _blobStorage;
		private readonly FileInformation _fileInfo;
		private readonly CloudBlob _blob;
	}
}
