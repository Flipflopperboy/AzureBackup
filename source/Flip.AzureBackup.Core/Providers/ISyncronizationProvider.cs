using Flip.AzureBackup.Actions;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public interface ISyncronizationProvider
	{
		string Description { get; }
		void WriteStatistics(ILog logger);
		ISyncAction CreateUpdateSyncAction(CloudBlob blob, FileInformation fileInfo);
		ISyncAction CreateUpdateModifiedDateSyncAction(CloudBlob blob, FileInformation fileInfo);
		ISyncAction CreateBlobNotExistsSyncAction(CloudBlobContainer blobContainer, FileInformation fileInfo);
		ISyncAction CreateFileNotExistsSyncAction(CloudBlob blob, string basePath);
		bool InitializeDirectory(string path);		
	}
}
