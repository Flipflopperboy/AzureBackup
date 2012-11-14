using System;
using System.Security.Cryptography;



namespace Flip.AzureBackup.WindowsAzure
{
	internal static class ByteExtensions
	{
		public static string GetMD5Hash(this byte[] bytes)
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] hash = md5.ComputeHash(bytes);
			return Convert.ToBase64String(hash);
		}
	}
}
