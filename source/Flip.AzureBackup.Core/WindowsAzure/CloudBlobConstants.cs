namespace Flip.AzureBackup.WindowsAzure
{
	public static class CloudBlobConstants
	{
		public static readonly long FileSizeThresholdInBytes = (4L).MBToBytes();
		public static readonly int MaxBlockSize = 4.MBToBytes();
	}
}
