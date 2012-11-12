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
			return System.Net.WebUtility.HtmlDecode(blob.Metadata[fileRelativePathKey]);
		}

		public static void SetRelativeFilePath(this CloudBlob blob, string relativePath)
		{
			blob.Metadata[fileRelativePathKey] = System.Net.WebUtility.HtmlEncode(relativePath);
		}

		public static void UploadFile(this CloudBlob blob, FileInformation fileInfo)
		{
			blob.SetFileLastModifiedUtc(fileInfo.LastWriteTimeUtc, false);
			blob.SetRelativeFilePath(fileInfo.RelativePath);
			blob.UploadFile(fileInfo.FullPath);
		}



		private const string fileRelativePathKey = "FileRelativePath";
		private const string fileLastModifiedUtcKey = "FileLastModifiedUtcTicks";
	}
}
