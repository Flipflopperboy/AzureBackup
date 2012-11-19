using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;



namespace Flip.AzureBackup.IO
{
	public sealed class FileSystem : IFileSystem
	{
		public IEnumerable<FileInformation> GetFileInformationIncludingSubDirectories(string directoryPath)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

			return directoryInfo
				.GetFiles("*", SearchOption.AllDirectories)
				.Select(file => new FileInformation(GetFullPath(file), GetRelativePath(file, directoryPath), file.LastWriteTimeUtc, file.Length));
		}

		public Stream OpenReadFileStream(string path)
		{
			return new FileStream(path, FileMode.Open, FileAccess.Read);
		}
		
		public Stream OpenOrCreateWriteFileStream(string path)
		{
			return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
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

		public void EnsureFileDirectory(string fullPath)
		{
			string directory = Path.GetDirectoryName(fullPath);
			CreateDirectoryIfNotExists(directory);
		}

		private static string GetFullPath(FileInfo fileInfo)
		{
			return Path.Combine(fileInfo.Directory.ToString(), fileInfo.ToString());
		}

		private static string GetRelativePath(FileInfo fileInfo, string basePath)
		{
			if (!basePath.EndsWith(@"\"))
			{
				basePath += @"\";
			}

			return GetFullPath(fileInfo).Substring(basePath.Length);
		}
	}
}
