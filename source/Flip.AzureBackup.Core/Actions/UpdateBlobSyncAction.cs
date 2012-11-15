using Flip.AzureBackup.IO;
using Flip.AzureBackup.WindowsAzure;
using Flip.Common.Messages;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class UpdateBlobSyncAction : SyncAction
	{
		public UpdateBlobSyncAction(IMessageBus messageBus, ICloudBlobStorage blobStorage, FileInformation fileInfo, CloudBlob blob)
			: base(messageBus)
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
