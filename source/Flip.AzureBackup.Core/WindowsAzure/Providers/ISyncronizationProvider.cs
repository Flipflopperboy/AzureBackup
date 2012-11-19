using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.Common.Threading;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.WindowsAzure.Providers
{
	public interface ISyncronizationProvider
	{
		string Description { get; }
		void WriteStatistics(ILog log);
		TaskRunner CreateUpdateTaskRunner(CloudBlob blob, FileInformation fileInfo);
		TaskRunner CreateUpdateModifiedDateTaskRunner(CloudBlob blob, FileInformation fileInfo);
		TaskRunner CreateBlobNotExistsTaskRunner(CloudBlobContainer blobContainer, FileInformation fileInfo);
		TaskRunner CreateFileNotExistsTaskRunner(CloudBlob blob, string basePath);
		bool InitializeDirectory(string path);
	}
}
