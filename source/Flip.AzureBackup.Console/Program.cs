using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace AzureBackup
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
				Console.WriteLine("Inga filer");
			}
			Console.ReadKey();
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
							Console.WriteLine("Uppdaterar senast ändrat datum för " + path + "...");
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

			Console.WriteLine("Klart...");
		}

		private static void ProcessBlobDeletions(Dictionary<Uri, CloudBlob> blobs)
		{
			foreach (var item in blobs)
			{
				Console.WriteLine("Deleting " + item.Key.AbsolutePath + "...");
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

	public static class FileInfoExtensions
	{
		public static string GetFullPath(this FileInfo fileInfo)
		{
			return Path.Combine(fileInfo.Directory.ToString(), fileInfo.ToString());
		}
	}
	public static class CloudBlobExtensions
	{
		//public static bool Exists(this CloudBlob blob)
		//{
		//	try
		//	{
		//		blob.FetchAttributes();
		//		return true;
		//	}
		//	catch (StorageClientException e)
		//	{
		//		if (e.ErrorCode == StorageErrorCode.ResourceNotFound)
		//		{
		//			return false;
		//		}
		//		else
		//		{
		//			throw;
		//		}
		//	}
		//}
		public static void SetFileLastModifiedUtc(this CloudBlob blob, DateTime fileLastModifiedUtc, bool update)
		{
			blob.Metadata[fileLastModifiedUtcKey] = fileLastModifiedUtc.Ticks.ToString();
			if (update)
			{
				blob.SetMetadata();
			}
		}
		public static DateTime GetFileLastModifiedUtc(this CloudBlob blob)
		{
			long ticks;
			if (long.TryParse(blob.Metadata[fileLastModifiedUtcKey], out ticks))
			{
				return new DateTime(ticks, DateTimeKind.Utc);
			}
			else
			{
				return DateTime.MinValue.ToUniversalTime();
			}
		}
		public static void UploadFile(this CloudBlob blob, string path, DateTime lastWriteTimeUtc)
		{
			Console.WriteLine("Laddar upp fil " + path + " ...");
			blob.SetFileLastModifiedUtc(lastWriteTimeUtc, false);
			try
			{
				blob.UploadFile(path);
			}
			catch (FileNotFoundException) { /**/ }
		}
		private const string fileLastModifiedUtcKey = "FileLastModifiedUtcTicks";
	}
	public static class CloudBlobContainerExtensions
	{
		public static Uri GetBlobUri(this CloudBlobContainer container, string path)
		{
			var uriBuilder = new UriBuilder(container.Uri);

			string stringToEscape = uriBuilder.Path.EndsWith(CloudBlobContainerExtensions.separator) ?
				path :
				separator + path;

			UriBuilder resultingUriBuilder = uriBuilder;
			resultingUriBuilder.Path += Uri.EscapeUriString(stringToEscape);
			return uriBuilder.Uri;
		}

		private const string separator = "/";
	}
}