using System;
using System.IO;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup
{
	internal static class CloudBlobExtensions
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
}
