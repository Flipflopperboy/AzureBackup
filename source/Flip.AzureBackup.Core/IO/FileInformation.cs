using System;

namespace Flip.AzureBackup.IO
{
	public sealed class FileInformation
	{
		public FileInformation(string fullPath, string relativePath, DateTime lastWriteTimeUtc)
		{
			this.FullPath = fullPath;
			this.RelativePath = relativePath;
			this.LastWriteTimeUtc = lastWriteTimeUtc;
		}

		public string FullPath { get; private set; }
		public string RelativePath { get; private set; }
		public DateTime LastWriteTimeUtc { get; private set; }
	}
}
