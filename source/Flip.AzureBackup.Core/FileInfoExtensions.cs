using System.IO;



namespace Flip.AzureBackup
{
	public static class FileInfoExtensions
	{
		public static string GetFullPath(this FileInfo fileInfo)
		{
			return Path.Combine(fileInfo.Directory.ToString(), fileInfo.ToString());
		}
	}
}
