namespace Flip.AzureBackup.Logging
{
	public static class LogExtensions
	{
		public static void WriteFixedLine(this ILog log, string description, int value)
		{
			log.WriteLine(description + value.ToString().PadLeft(lineLength - description.Length, ' '));
		}

		public static void WriteFixedLine(this ILog log, char repeatChar)
		{
			log.WriteLine("".PadLeft(lineLength, repeatChar));
		}



		private const int lineLength = 30;
	}
}
