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



		public void CreateBlob(CloudBlobContainer blobContainer, FileInformation fileInfo, string path)
		{
		}

		public void UpdateBlob(CloudBlob blob, FileInformation fileInfo, string path)
		{
		}

		public void UpdateBlobModifiedDate(CloudBlob blob, FileInformation fileInfo)
		{
		}

		public void DeleteBlob(CloudBlob blob)
		{
		}



		private readonly ILogger _logger;
	}
}
