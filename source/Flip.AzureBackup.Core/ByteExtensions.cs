using System;
using System.Security.Cryptography;



namespace Flip.AzureBackup
{
	public static class ByteExtensions
	{
		public static bool IsAllZero(this byte[] bytes, long rangeOffset, long size)
		{
			for (long offset = 0; offset < size; offset++)
			{
				if (bytes[rangeOffset + offset] != 0)
				{
					return false;
				}
			}
			return true;
		}

		public static string GetMD5Hash(this byte[] bytes)
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] hash = md5.ComputeHash(bytes);
			return Convert.ToBase64String(hash);
		}
	}
}
