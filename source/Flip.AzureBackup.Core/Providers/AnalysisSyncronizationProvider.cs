using System;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Providers
{
	public class AnalysisSyncronizationProvider : ISyncronizationProvider
	{
		public AnalysisSyncronizationProvider(ILogger logger)
		{
			this._logger = logger;
		}



		public void WriteStart()
		{
			this._logger.WriteLine("ANALYSIS");
		}



		public bool HasBeenModified(CloudBlob blob, FileInformation fileInfo)
		{
			return fileInfo.LastWriteTimeUtc > blob.GetFileLastModifiedUtc();
		}

		public void HandleUpdate(CloudBlob blob, FileInformation fileInfo)
		{
		}

		public void HandleUpdateModifiedDate(CloudBlob blob, FileInformation fileInfo)
		{
		}

		public void HandleBlobNotExists(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
		}

		public void HandleFileNotExists(CloudBlob blob)
		{
		}



		private readonly ILogger _logger;
	}
}
