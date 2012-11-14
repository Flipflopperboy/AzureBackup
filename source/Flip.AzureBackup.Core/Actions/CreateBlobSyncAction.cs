using Flip.AzureBackup.IO;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class CreateBlobSyncAction : SyncAction
	{
		public CreateBlobSyncAction(ICloudBlobStorage blobStorage, CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			_blobStorage = blobStorage;
			_blobContainer = blobContainer;
			_fileInfo = fileInfo;
		}

		public override void Invoke()
		{
			ReportProgress(_fileInfo.FullPath, "Uploading file...", 0);
			_blobStorage.UploadFile(_blobContainer, _fileInfo);
			ReportProgress(_fileInfo.FullPath, "Uploaded file.", 1);
		}



		private readonly ICloudBlobStorage _blobStorage;
		private readonly CloudBlobContainer _blobContainer;
		private readonly FileInformation _fileInfo;
	}
}
