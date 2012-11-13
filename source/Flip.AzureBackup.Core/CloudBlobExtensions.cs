using System;
using Flip.AzureBackup.IO;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup
{
	internal static class CloudBlobExtensions
	{
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

		public static string GetRelativeFilePath(this CloudBlob blob)
		{
			string filePath = blob.Uri.OriginalString;
			filePath = filePath.Substring(blob.Container.Uri.OriginalString.Length + 1);
			filePath = filePath.Replace('/', '\\');
			filePath = Uri.UnescapeDataString(filePath);
			return filePath;
		}

		public static void UploadFile(this CloudBlob blob, FileInformation fileInfo)
		{
			blob.SetFileLastModifiedUtc(fileInfo.LastWriteTimeUtc, false);
			blob.UploadFile(fileInfo.FullPath);
		}



		private const string fileLastModifiedUtcKey = "FileLastModifiedUtcTicks";
	}
}
