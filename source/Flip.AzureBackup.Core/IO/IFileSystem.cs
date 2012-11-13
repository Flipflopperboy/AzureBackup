using System;
using System.Collections.Generic;
using System.IO;



namespace Flip.AzureBackup.IO
{
	public interface IFileSystem
	{
		void EnsureFileDirectory(string fullPath);
		bool DirectoryExists(string directoryPath);
		void CreateDirectoryIfNotExists(string path);
		Stream GetReadFileStream(string path);
		Stream GetWriteFileStream(string path);

		string GetMD5HashForFile(string path);
		void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);
		void DeleteFile(string path);
		IEnumerable<FileInformation> GetFileInformationIncludingSubDirectories(string directoryPath);

		string Combine(params string[] paths);		
	}
}
