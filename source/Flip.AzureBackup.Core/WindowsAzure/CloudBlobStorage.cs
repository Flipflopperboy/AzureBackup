using System;
using System.Collections.Generic;
using System.IO;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.WindowsAzure
{
	/// <summary>
	/// http://blogs.msdn.com/b/windowsazurestorage/archive/2010/04/11/using-windows-azure-page-blobs-and-how-to-efficiently-upload-and-download-page-blobs.aspx
	/// </summary>
	public sealed class CloudBlobStorage : ICloudBlobStorage
	{
		public CloudBlobStorage(ILogger logger, IFileSystem fileSystem)
		{
			this._logger = logger;
			this._fileSystem = fileSystem;
		}



		public void DownloadFile(CloudBlob blob, string path)
		{
			if (blob.Properties.Length > FileSizeThresholdInBytes)
			{
				DownloadFileInChunks(blob.ToPageBlob, path);
			}
			else
			{
				blob.DownloadToFile(path);
			}
		}

		public void UploadFile(CloudBlob blob, FileInformation fileInfo)
		{
			if (fileInfo.SizeInBytes > FileSizeThresholdInBytes)
			{
				UploadFileInChunks(blob.ToPageBlob, fileInfo);
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
				var pageBlob = blobContainer.GetPageBlobReference(fileInfo.RelativePath);
				UploadFileInChunks(pageBlob, fileInfo);
			}
			else
			{
				CloudBlob blob = blobContainer.GetBlobReference(fileInfo.RelativePath);
				blob.UploadFile(fileInfo);
			}
		}



		private void UploadFileInChunks(CloudPageBlob pageBlob, FileInformation fileInfo)
		{
			pageBlob.Create(fileInfo.SizeInBytes.RoundUpToMultipleOf(pageBlobPageFactor)); //Block blob size must be multiple of 512


			using (Stream stream = this._fileSystem.GetReadFileStream(fileInfo.FullPath))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					long totalUploaded = 0;
					long fileOffset = 0;
					int offsetToTransfer = -1;

					while (fileOffset < fileInfo.SizeInBytes)
					{
						byte[] readBytes = reader.ReadBytes(chunkSizeInBytes);

						int offsetInRange = 0;

						// Make sure end is page size aligned
						if ((readBytes.Length % pageBlobPageFactor) > 0)
						{
							int grow = (int)(pageBlobPageFactor - (readBytes.Length % pageBlobPageFactor));
							Array.Resize(ref readBytes, readBytes.Length + grow);
						}

						// Upload groups of continuous non-zero page blob pages.  
						while (offsetInRange <= readBytes.Length)
						{
							if ((offsetInRange == readBytes.Length) || readBytes.IsAllZero(offsetInRange, pageBlobPageFactor))
							{
								if (offsetToTransfer != -1)
								{
									// Transfer up to this point
									int sizeToTransfer = offsetInRange - offsetToTransfer;
									using (MemoryStream memoryStream = new MemoryStream(readBytes, offsetToTransfer, sizeToTransfer, false, false))
									{
										pageBlob.WritePages(memoryStream, fileOffset + offsetToTransfer);
									}
									this._logger.WriteLine("Range ~" + (offsetToTransfer + fileOffset).ToByteString() + " + " + sizeToTransfer.ToByteString());
									totalUploaded += sizeToTransfer;
									offsetToTransfer = -1;
								}
							}
							else
							{
								if (offsetToTransfer == -1)
								{
									offsetToTransfer = offsetInRange;
								}
							}
							offsetInRange += pageBlobPageFactor;
						}
						fileOffset += readBytes.Length;
					}
				}
			}
		}

		private void DownloadFileInChunks(CloudPageBlob pageBlob, string path)
		{
			long blobSizeInBytes = pageBlob.Properties.Length;
			long totalDownloaded = 0;

			// Create a new local file to write into
			using (Stream stream = this._fileSystem.GetWriteFileStream(path))
			{
				stream.SetLength(blobSizeInBytes);

				// Download the valid ranges of the blob, and write them to the file
				IEnumerable<PageRange> pageRanges = pageBlob.GetPageRanges();
				BlobStream blobStream = pageBlob.OpenRead();

				foreach (PageRange range in pageRanges)
				{
					// EndOffset is inclusive... so need to add 1
					int rangeSize = (int)(range.EndOffset + 1 - range.StartOffset);

					// Chop range into chucks, if needed
					for (int subOffset = 0; subOffset < rangeSize; subOffset += chunkSizeInBytes)
					{
						int subRangeSize = Math.Min(rangeSize - subOffset, chunkSizeInBytes);
						blobStream.Seek(range.StartOffset + subOffset, SeekOrigin.Begin);
						stream.Seek(range.StartOffset + subOffset, SeekOrigin.Begin);

						this._logger.WriteLine("Range: ~" + (range.StartOffset + subOffset).ToByteString() + " + " + subRangeSize.ToByteString());

						byte[] buffer = new byte[subRangeSize];
						blobStream.Read(buffer, 0, subRangeSize);
						stream.Write(buffer, 0, subRangeSize);
						totalDownloaded += subRangeSize;
					}
				}
			}
		}



		private readonly ILogger _logger;
		private readonly IFileSystem _fileSystem;
		private static readonly int chunkSizeInBytes = 4.MBToBytes();
		private static readonly long FileSizeThresholdInBytes = (12L).MBToBytes();
		private const int pageBlobPageFactor = 512;
	}
}
