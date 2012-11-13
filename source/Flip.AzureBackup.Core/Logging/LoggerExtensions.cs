namespace Flip.AzureBackup.Logging
{
	public static class LoggerExtensions
	{
		public static void WriteFixedLine(this ILogger logger, string description, int value)
		{
			logger.WriteLine(description + value.ToString().PadLeft(lineLength - description.Length, ' '));
		}

		public static void WriteFixedLine(this ILogger logger, char repeatChar)
		{
			logger.WriteLine("".PadLeft(lineLength, repeatChar));
		}



		private const int lineLength = 30;
	}
}
