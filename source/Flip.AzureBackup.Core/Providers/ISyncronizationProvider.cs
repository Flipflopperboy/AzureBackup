using System.Collections.Generic;
using Flip.AzureBackup.IO;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public interface ISyncronizationProvider
	{
		void WriteStart();
		void WriteStatistics();
		bool NeedToCheckCloud(List<FileInformation> files);
		bool HasBeenModified(CloudBlob blob, FileInformation fileInfo);
		void HandleUpdate(CloudBlob blob, FileInformation fileInfo);
		void HandleUpdateModifiedDate(CloudBlob blob, FileInformation fileInfo);
		void HandleBlobNotExists(CloudBlobContainer blobContainer, FileInformation fileInfo);
		void HandleFileNotExists(CloudBlob blob, string basePath);
		bool InitializeDirectory(string path);		
	}
}
