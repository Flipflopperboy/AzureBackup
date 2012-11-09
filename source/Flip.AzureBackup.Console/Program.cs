using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			string containerName = "backup";
			string directoryPath = @"C:\temp\backup";
			var directoryInfo = new DirectoryInfo(directoryPath);
			Dictionary<string, FileInfo> files = directoryInfo
				.GetFiles("*", SearchOption.AllDirectories)
				.ToDictionary(info => info.GetFullPath(), info => info);

			if (files.Count > 0)
			{
				CloudStorageAccount cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
				CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
				CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);

				blobContainer.CreateIfNotExist();

				ProcessFileUploads(files, blobContainer);
			}
			else
			{
				System.Console.WriteLine("Inga filer");
			}
			System.Console.ReadKey();
		}

		private static void ProcessFileUploads(Dictionary<string, FileInfo> files, CloudBlobContainer blobContainer)
		{
			Dictionary<Uri, CloudBlob> blobs = blobContainer.ListBlobs(blobRequestOptions)
				.Cast<CloudBlob>()
				.ToDictionary(blob => blob.Uri, blob => blob);

			foreach (KeyValuePair<string, FileInfo> pair in files)
			{
				string path = pair.Key;
				FileInfo fileInfo = pair.Value;

				var blobUri = blobContainer.GetBlobUri(path);
				if (blobs.ContainsKey(blobUri))
				{
					CloudBlob blob = blobs[blobUri];
					DateTime blobFileLastModifiedUtc = blob.GetFileLastModifiedUtc();
					if (fileInfo.LastWriteTimeUtc > blobFileLastModifiedUtc)
					{
						string contentMD5 = GetMD5HashForFile(path);
						if (contentMD5 == blob.Properties.ContentMD5)
						{
							blob.SetFileLastModifiedUtc(fileInfo.LastWriteTimeUtc, true);
							System.Console.WriteLine("Uppdaterar senast ändrat datum för " + path + "...");
						}
						else
						{
							blob.UploadFile(path, fileInfo.LastWriteTimeUtc);
						}
					}

					//Only keep blobs which should be deleted
					blobs.Remove(blobUri);
				}
				else
				{
					CloudBlob blob = blobContainer.GetBlobReference(path);
					blob.UploadFile(path, fileInfo.LastWriteTimeUtc);
				}
			}

			ProcessBlobDeletions(blobs);

			System.Console.WriteLine("Klart...");
		}

		private static void ProcessBlobDeletions(Dictionary<Uri, CloudBlob> blobs)
		{
			foreach (var item in blobs)
			{
				System.Console.WriteLine("Deleting " + item.Key.AbsolutePath + "...");
				item.Value.DeleteIfExists();
			}
		}

		private static string GetMD5HashForFile(string path)
		{
			byte[] hash = null;
			using (FileStream file = new FileStream(path, FileMode.Open))
			{
				MD5 md5 = new MD5CryptoServiceProvider();
				hash = md5.ComputeHash(file);
			}
			return Convert.ToBase64String(hash);
		}



		private static readonly BlobRequestOptions blobRequestOptions = new BlobRequestOptions
		{
			UseFlatBlobListing = true,
			BlobListingDetails = BlobListingDetails.Metadata
		};
	}
}