using System;
using System.Collections.Generic;



namespace Flip.AzureBackup.IO
{
	public interface IFileAccessor
	{
		IEnumerable<FileInformation> GetFileInfoIncludingSubDirectories(string directoryPath);
		bool DirectoryExists(string directoryPath);
		string GetMD5HashForFile(string path);
		void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);
		void DeleteFile(string path);
	}
}
