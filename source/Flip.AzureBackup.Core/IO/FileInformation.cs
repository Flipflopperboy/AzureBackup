using System;

namespace Flip.AzureBackup.IO
{
	public sealed class FileInformation
	{
		public FileInformation(string fullPath, string relativePath, DateTime lastWriteTimeUtc, long sizeInBytes)
		{
			this.FullPath = fullPath;
			this.RelativePath = relativePath;
			this.BlobName = Uri.EscapeDataString(this.RelativePath);
			this.LastWriteTimeUtc = lastWriteTimeUtc;
			this.SizeInBytes = sizeInBytes;
		}

		public string FullPath { get; private set; }
		public string RelativePath { get; private set; }
		public string BlobName { get; private set; }
		public DateTime LastWriteTimeUtc { get; private set; }
		public long SizeInBytes { get; private set; }
	}
}
