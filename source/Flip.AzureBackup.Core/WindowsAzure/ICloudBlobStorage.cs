using Flip.AzureBackup.IO;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.WindowsAzure
{
	public interface ICloudBlobStorage
	{
		void DownloadFile(CloudBlob blob, string path);
		void UploadFile(CloudBlob blob, FileInformation fileInfo);
		void UploadFile(CloudBlobContainer blobContainer, FileInformation fileInfo);		
	}
}
