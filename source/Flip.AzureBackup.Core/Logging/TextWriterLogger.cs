using System.IO;

namespace Flip.AzureBackup.Logging
{
	public class TextWriterLogger : ILogger
	{
		public TextWriterLogger(TextWriter textWriter)
		{
			this._textWriter = textWriter;
		}

		public void WriteLine(string s)
		{
			this._textWriter.WriteLine(s);
		}

		private readonly TextWriter _textWriter;
	}
}
