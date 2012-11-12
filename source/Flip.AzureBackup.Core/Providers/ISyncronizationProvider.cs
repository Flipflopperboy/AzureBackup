using Flip.AzureBackup.IO;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public interface ISyncronizationProvider
	{
		void WriteStart();
		void CreateBlob(CloudBlobContainer blobContainer, FileInformation fileInfo, string path);
		void UpdateBlob(CloudBlob blob, FileInformation fileInfo, string path);
		void UpdateBlobModifiedDate(CloudBlob blob, FileInformation fileInfo);
		void DeleteBlob(CloudBlob blob);
	}
}
