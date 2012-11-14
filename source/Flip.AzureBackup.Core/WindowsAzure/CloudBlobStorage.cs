using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.StorageClient.Protocol;



namespace Flip.AzureBackup.WindowsAzure
{
	/// <summary>
	/// http://blogs.msdn.com/b/windowsazurestorage/archive/2010/04/11/using-windows-azure-page-blobs-and-how-to-efficiently-upload-and-download-page-blobs.aspx
	/// </summary>
	public sealed class CloudBlobStorage : ICloudBlobStorage
	{
		public CloudBlobStorage(IFileSystem fileSystem)
		{
			this._fileSystem = fileSystem;
		}



		public void DownloadFile(CloudBlob blob, string path, Action<decimal> progressCallback)
		{
			if (blob.Properties.Length > FileSizeThresholdInBytes)
			{
				DownloadFileInChunks(blob.ToBlockBlob, path, progressCallback);
			}
			else
			{
				blob.DownloadToFile(path);
				progressCallback(1);
			}
		}

		public void UploadFile(CloudBlob blob, FileInformation fileInfo)
		{
			if (fileInfo.SizeInBytes > FileSizeThresholdInBytes)
			{
				UploadFileInChunks(blob.ToBlockBlob, fileInfo);
			}
			else
			{
				blob.UploadFile(fileInfo);
			}
		}

		public void UploadFile(CloudBlobContainer blobContainer, FileInformation fileInfo)
		{
			if (fileInfo.SizeInBytes > FileSizeThresholdInBytes)
			{
				var blockBlob = blobContainer.GetBlockBlobReference(fileInfo.BlobName);
				blockBlob.SetFileLastModifiedUtc(fileInfo.LastWriteTimeUtc, false);
				UploadFileInChunks(blockBlob, fileInfo);
			}
			else
			{
				CloudBlob blob = blobContainer.GetBlobReference(fileInfo.BlobName);
				blob.UploadFile(fileInfo);
			}
		}

		private void UploadFileInChunks(CloudBlockBlob blockBlob, FileInformation fileInfo)
		{
			List<string> blockIds = new List<string>();

			using (Stream stream = this._fileSystem.GetReadFileStream(fileInfo.FullPath))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					int blockCounter = 0;
					int bytesSent = 0;
					int currentBlockSize = MaxBlockSize;
					int exceptionCount = 0;

					try
					{
						while (bytesSent < fileInfo.SizeInBytes)
						{
							if ((bytesSent + currentBlockSize) > fileInfo.SizeInBytes)
							{
								currentBlockSize = (int)fileInfo.SizeInBytes - bytesSent;
							}

							string blockId = blockCounter.ToString().PadLeft(32, '0');
							byte[] bytes = reader.ReadBytes(currentBlockSize);
							using (var memoryStream = new MemoryStream(bytes))
							{
								blockIds.Add(blockId);
								blockBlob.PutBlock(blockId, memoryStream, bytes.GetMD5Hash());
							}
							bytesSent += currentBlockSize;
							blockCounter++;
						}
					}
					catch (Exception ex)
					{
						exceptionCount++;
						if (exceptionCount >= 100)
						{
							throw new Exception("Could not upload file", ex);
						}
					}
				}
			}
			//Commit the block list
			blockBlob.PutBlockList(blockIds);
		}

		private void DownloadFileInChunks(CloudBlob blob, string path, Action<decimal> progressCallback)
		{
			int counter = 0;
			int exceptionCount = 0;

			int rangeStart = 0;
			int rangeEnd = 0;

			long blobLength = blob.Properties.Length;
			int parts = (int)Math.Ceiling((decimal)blobLength / (decimal)MaxBlockSize);
			decimal rangeFraction = 1 / (decimal)parts;

			using (Stream fileStream = this._fileSystem.GetWriteFileStream(path))
			{
				try
				{
					while (counter < parts)
					{
						rangeStart = counter * MaxBlockSize;
						rangeEnd = rangeStart + MaxBlockSize - 1;
						rangeEnd = rangeEnd > blobLength ? rangeStart + (int)blobLength - rangeStart - 1 : rangeEnd;

						HttpWebRequest blobGetRequest = BlobRequest.Get(blob.Uri, 60, null, null);
						blobGetRequest.Headers.Add("x-ms-range", string.Format(System.Globalization.CultureInfo.InvariantCulture, "bytes={0}-{1}", rangeStart, rangeEnd));

						StorageCredentials credentials = blob.ServiceClient.Credentials;
						credentials.SignRequest(blobGetRequest);

						using (HttpWebResponse response = blobGetRequest.GetResponse() as HttpWebResponse)
						{
							using (Stream stream = response.GetResponseStream())
							{
								stream.CopyTo(fileStream);
							}
						}
						counter++;
						progressCallback(counter < parts ? rangeFraction * counter : 1);
					}
				}
				catch (Exception ex)
				{
					exceptionCount++;
					if (exceptionCount >= 100)
					{
						throw new Exception("Could not download file '" + path + "'.", ex);
					}
				}
			}
		}



		private readonly IFileSystem _fileSystem;
		private static readonly int MaxBlockSize = 4.MBToBytes();
		private static readonly long FileSizeThresholdInBytes = (4L).MBToBytes();
	}
}
