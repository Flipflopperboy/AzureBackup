namespace Flip.AzureBackup
{
	public static class NumberExtensions
	{
		public static int MBToBytes(this int value)
		{
			return 1024 * 1024 * value;
		}

		public static int KBToBytes(this int value)
		{
			return value * bytesPerKB;
		}

		public static int BytesToMB(this int value)
		{
			return value / bytesPerMB;
		}

		public static int BytesToKB(this int value)
		{
			return value / bytesPerKB;
		}

		public static string ToByteString(this int value)
		{
			if (value >= oneMBInBytes)
			{
				return value.BytesToMB().ToString() + " MB";
			}
			if (value >= oneMBInBytes)
			{
				return value.BytesToKB().ToString() + " kb";
			}
			return value.ToString() + " bytes";
		}



		public static long RoundUpToMultipleOf(this long value, long factor)
		{
			return (value + factor - 1) & ~(factor - 1);
		}

		public static long MBToBytes(this long value)
		{
			return 1024 * 1024 * value;
		}

		public static long KBToBytes(this long value)
		{
			return value * bytesPerKB;
		}

		public static long BytesToMB(this long value)
		{
			return value / bytesPerMB;
		}

		public static long BytesToKB(this long value)
		{
			return value / bytesPerKB;
		}

		public static string ToByteString(this long value)
		{
			if (value >= oneMBInBytes)
			{
				return value.BytesToMB().ToString() + " MB";
			}
			if (value >= oneMBInBytes)
			{
				return value.BytesToKB().ToString() + " kb";
			}
			return value.ToString() + " bytes";
		}



		private const int bytesPerKB = 1024;
		private const int bytesPerMB = 1024 * 1024;
		private static readonly int oneKBInBytes = 1.KBToBytes();
		private static readonly int oneMBInBytes = 1.MBToBytes();
	}
}
