﻿using Flip.Common.Messages;



namespace Flip.AzureBackup.Messages
{
	public sealed class FileProgressedMessage : MessageBase
	{
		public FileProgressedMessage(string fullFilePath, decimal fraction)
		{
			this.FullFilePath = fullFilePath;
			this.Fraction = fraction;
		}



		public string FullFilePath { get; private set; }
		public decimal Fraction { get; private set; }
	}
}
