using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;



namespace Flip.AzureBackup.IO
{
	public sealed class FileAccessor : IFileAccessor
	{
		public IEnumerable<FileInformation> GetFileInfoIncludingSubDirectories(string directoryPath)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

			return directoryInfo
				.GetFiles("*", SearchOption.AllDirectories)
				.Select(file => new FileInformation(file.GetFullPath(), file.GetRelativePath(directoryPath), file.LastWriteTimeUtc));
		}

		public bool DirectoryExists(string directoryPath)
		{
			return Directory.Exists(directoryPath);
		}

		public void CreateDirectoryIfNotExists(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		public string GetMD5HashForFile(string path)
		{
			byte[] hash = null;
			using (FileStream file = new FileStream(path, FileMode.Open))
			{
				MD5 md5 = new MD5CryptoServiceProvider();
				hash = md5.ComputeHash(file);
			}
			return Convert.ToBase64String(hash);
		}

		public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		{
			File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
		}

		public void DeleteFile(string path)
		{
			File.Delete(path);
		}

		public string Combine(params string[] paths)
		{
			return Path.Combine(paths);
		}

		public void EnsureDirectories(string fullPath)
		{
			string directory = Path.GetDirectoryName(fullPath);
			CreateDirectoryIfNotExists(directory);
		}
	}
}
