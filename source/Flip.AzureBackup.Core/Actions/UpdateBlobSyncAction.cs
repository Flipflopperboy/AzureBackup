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
			_blobStorage.UploadFile(_blob, _fileInfo, fraction =>
				ReportProgress(_fileInfo.FullPath, fraction == 0 ? "Updating blob..." : "", fraction));
		}



		private readonly ICloudBlobStorage _blobStorage;
		private readonly FileInformation _fileInfo;
		private readonly CloudBlob _blob;
	}
}
