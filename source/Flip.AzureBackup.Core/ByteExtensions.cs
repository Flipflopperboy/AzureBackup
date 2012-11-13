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
	}
}
