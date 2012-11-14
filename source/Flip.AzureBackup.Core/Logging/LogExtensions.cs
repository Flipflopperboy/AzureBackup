namespace Flip.AzureBackup.Logging
{
	public static class LogExtensions
	{
		public static void WriteFixedLine(this ILog logger, string description, int value)
		{
			logger.WriteLine(description + value.ToString().PadLeft(lineLength - description.Length, ' '));
		}

		public static void WriteFixedLine(this ILog logger, char repeatChar)
		{
			logger.WriteLine("".PadLeft(lineLength, repeatChar));
		}



		private const int lineLength = 30;
	}
}
