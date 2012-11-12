using System;
using System.Collections.Generic;
using System.Linq;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup
{
	public class Synchronizer : ISynchronizer
	{
		public Synchronizer(ILogger logger, IFileAccessor fileAccessor)
		{
			this._logger = logger;
			this._fileAccessor = fileAccessor;
		}



		public void Synchronize(string cloudConnectionString, string containerName, string directoryPath)
		{
			if (!this._fileAccessor.DirectoryExists(directoryPath))
			{
				System.Console.WriteLine("Directory does not exist '" + directoryPath + "'.");
				return;
			}

			Dictionary<string, FileInformation> files = this._fileAccessor
				.GetFileInfoIncludingSubDirectories(directoryPath)
				.ToDictionary(file => file.FullPath, file => file);

			if (files.Count > 0)
			{
				CloudStorageAccount cloudStorageAccount;
				if (CloudStorageAccount.TryParse(cloudConnectionString, out cloudStorageAccount))
				{
					CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
					CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);

					blobContainer.CreateIfNotExist();

					SynchronizeToCloud(files, blobContainer);
				}
				else
				{
					this._logger.WriteLine("Invalid cloud connection string '" + cloudConnectionString + "'.");
				}
			}
			else
			{
				this._logger.WriteLine("No files to process...");
			}
		}



		private void SynchronizeToCloud(Dictionary<string, FileInformation> files, CloudBlobContainer blobContainer)
		{
			var statistics = new SyncronizationStatistics();

			Dictionary<Uri, CloudBlob> blobs = blobContainer.ListBlobs(blobRequestOptions)
				.Cast<CloudBlob>()
				.ToDictionary(blob => blob.Uri, blob => blob);

			foreach (var pair in files)
			{
				string path = pair.Key;
				FileInformation fileInfo = pair.Value;

				var blobUri = blobContainer.GetBlobUri(path);
				if (blobs.ContainsKey(blobUri))
				{
					CloudBlob blob = blobs[blobUri];
					DateTime blobFileLastModifiedUtc = blob.GetFileLastModifiedUtc();
					if (fileInfo.LastWriteTimeUtc > blobFileLastModifiedUtc)
					{
						string contentMD5 = this._fileAccessor.GetMD5HashForFile(path);
						if (contentMD5 == blob.Properties.ContentMD5)
						{
							statistics.UpdatedModifiedDateCount++;
							blob.SetFileLastModifiedUtc(fileInfo.LastWriteTimeUtc, true);
							this._logger.WriteLine("Updating changed date for " + path + "...");
						}
						else
						{
							statistics.UpdatedFileCount++;
							blob.UploadFile(path, fileInfo.LastWriteTimeUtc);
						}
					}

					//Only keep blobs which should be deleted
					blobs.Remove(blobUri);
				}
				else
				{
					statistics.NewFileCount++;
					CloudBlob blob = blobContainer.GetBlobReference(path);
					blob.UploadFile(path, fileInfo.LastWriteTimeUtc);
				}
			}

			statistics.DeletedFileCount = blobs.Count;
			ProcessBlobDeletions(blobs);

			WriteStatistics(statistics);
			this._logger.WriteLine("Done...");
		}

		private void ProcessBlobDeletions(Dictionary<Uri, CloudBlob> blobs)
		{
			foreach (var item in blobs)
			{
				this._logger.WriteLine("Deleting " + item.Key.AbsolutePath + "...");
				item.Value.DeleteIfExists();
			}
		}

		private void WriteStatistics(SyncronizationStatistics statistics)
		{
			int length = 30;
			this._logger.WriteLine("".PadLeft(length, '-'));
			WriteFixedLine("New files:", statistics.NewFileCount, length);
			WriteFixedLine("Modified files:", statistics.UpdatedFileCount, length);
			WriteFixedLine("Changed date files:", statistics.UpdatedModifiedDateCount, length);
			WriteFixedLine("Deleted files:", statistics.DeletedFileCount, length);			
			this._logger.WriteLine("".PadLeft(length, '-'));
			this._logger.WriteLine("");
		}

		private void WriteFixedLine(string s, int n, int length)
		{
			this._logger.WriteLine(s + n.ToString().PadLeft(length - s.Length, ' '));
		}



		private readonly ILogger _logger;
		private readonly IFileAccessor _fileAccessor;
		private static readonly BlobRequestOptions blobRequestOptions = new BlobRequestOptions
		{
			UseFlatBlobListing = true,
			BlobListingDetails = BlobListingDetails.Metadata
		};
		private class SyncronizationStatistics
		{
			public int NewFileCount { get; set; }
			public int UpdatedFileCount { get; set; }
			public int DeletedFileCount { get; set; }
			public int UpdatedModifiedDateCount { get; set; }
		}
	}
}