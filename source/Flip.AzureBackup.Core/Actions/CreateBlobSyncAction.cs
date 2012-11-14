using Flip.AzureBackup.IO;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class CreateBlobSyncAction : ISyncAction
	{
		public CreateBlobSyncAction(ICloudBlobStorage blobStorage, CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			_blobStorage = blobStorage;
			_blobContainer = blobContainer;
			_fileInfo = fileInfo;
		}

		public void Invoke()
		{
			//this._logger.WriteLine("Uploading file " + fileInfo.FullPath + "...");
			_blobStorage.UploadFile(_blobContainer, _fileInfo);
			//this._statistics.BlobNotExistCount++;
		}

		private readonly ICloudBlobStorage _blobStorage;
		private readonly CloudBlobContainer _blobContainer;
		private readonly FileInformation _fileInfo;
	}
}
