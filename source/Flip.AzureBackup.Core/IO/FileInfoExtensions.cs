using System;
using System.IO;



namespace Flip.AzureBackup.IO
{
	public static class FileInfoExtensions
	{
		public static string GetFullPath(this FileInfo fileInfo)
		{
			return Path.Combine(fileInfo.Directory.ToString(), fileInfo.ToString());
		}
		public static string GetRelativePath(this FileInfo fileInfo, string basePath)
		{
			if(!basePath.EndsWith(@"\"))
			{
				basePath += @"\";
			}

			return fileInfo.GetFullPath().Substring(basePath.Length);
		}
	}
}
