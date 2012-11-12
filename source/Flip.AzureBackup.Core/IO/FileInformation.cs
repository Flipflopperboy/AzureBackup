using System;

namespace Flip.AzureBackup.IO
{
	public sealed class FileInformation
	{
		public FileInformation(string fullPath, DateTime lastWriteTimeUtc)
		{
			this.FullPath = fullPath;
			this.LastWriteTimeUtc = lastWriteTimeUtc;
		}

		public string FullPath { get; private set; }
		public DateTime LastWriteTimeUtc { get; private set; }
	}
}
