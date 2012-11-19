using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Flip.AzureBackup.Console.Configuration;
using Flip.AzureBackup.Messages;
using Flip.Common;
using Flip.Common.Messages;
using NDesk.Options;



namespace Flip.AzureBackup.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			SyncSettings settings;
			if (ParseSettings(args, out settings))
			{
				IContainer container = ContainerConfiguration.CreateContainer();
				var synchronizer = container.Resolve<ISyncEngine>();
				var messageBus = container.Resolve<IMessageBus>();

				bool running = true;
				Task.Factory.StartNew(() =>
					{
						while (true)
						{
							if (System.Console.KeyAvailable)
							{
								ConsoleKey key = System.Console.ReadKey().Key;
								if (key == ConsoleKey.P)
								{
									running = !running;
									if (running)
									{
										messageBus.Publish(new SyncStartedMessage());
										System.Console.WriteLine("");
									}
									else
									{
										messageBus.Publish(new SyncPausedMessage());
										System.Console.WriteLine("");
										System.Console.WriteLine("PAUSED");
									}
								}
								else if (key == ConsoleKey.Q)
								{
									messageBus.Publish(new SyncStoppedMessage());
									System.Console.WriteLine("");
									System.Console.WriteLine("STOPPED");
								}
							}
							Thread.Sleep(50);
						}
					});

				messageBus.Subscribe<FileProgressedMessage>(OnActionProgressed);
				messageBus.Subscribe<FileAnalyzedMessage>(OnFileAnalyzed);
				messageBus.Subscribe<BlobAnalyzedMessage>(OnBlobAnalyzed);

				synchronizer.Sync(settings);
			}
			System.Console.ReadKey();
		}



		private static bool ParseSettings(string[] args, out SyncSettings settings)
		{
			settings = null;
			var parsedSettings = new SyncSettings();
			bool actionOk = false;
			string actionValue = null;

			var options = new OptionSet();
			options.Add("h|help", "Shows help page", v => ShowHelp(options))
					.Add("c|connectionString=", "Storage account connection string", v => parsedSettings.CloudConnectionString = v)
					.Add("b|blobContainer=", "Azure blob container name", v => parsedSettings.ContainerName = v)
					.Add("d|directory=", "Absolute path to directory to synchronize", v => parsedSettings.DirectoryPath = v)
					.Add("a|action=", "Action to perform", v =>
						{
							actionValue = v;
							SyncAction action;
							if (Enum.TryParse<SyncAction>(v, true, out action))
							{
								parsedSettings.Action = action;
								actionOk = true;
							}
						});

			try
			{
				options.Parse(args);
			}
			catch (OptionException e)
			{
				System.Console.WriteLine(e.Message);
				ShowTryMessage();
				System.Console.ReadKey();
				return false;
			}

			bool hasError = false;

			if (string.IsNullOrEmpty(parsedSettings.CloudConnectionString))
			{
				System.Console.WriteLine("Missing argument 'c|connectionString'.");
				hasError = true;
			}

			if (string.IsNullOrEmpty(parsedSettings.ContainerName))
			{
				System.Console.WriteLine("Missing argument 'b|blobContainer'.");
				hasError = true;
			}

			if (string.IsNullOrEmpty(parsedSettings.DirectoryPath))
			{
				System.Console.WriteLine("Missing argument 'd|directory'.");
				hasError = true;
			}

			if (!actionOk)
			{
				System.Console.WriteLine("Invalid action '" + actionValue + "'.");
				System.Console.WriteLine("Valid values are [" + Enum.GetNames(typeof(SyncAction)).ToSeparatedString(n => n, "|") + "].");
				hasError = true;
			}

			if (hasError)
			{
				ShowTryMessage();
				System.Console.ReadKey();
				return false;
			}

			settings = parsedSettings;

			return true;
		}

		private static void OnActionProgressed(FileProgressedMessage message)
		{
			if (message.Fraction == 0)
			{
				System.Console.WriteLine(message.FullFilePath);
				System.Console.WriteLine(message.Fraction.ToString("P0"));
			}
			else if (message.Fraction == 1)
			{
				System.Console.WriteLine(message.Fraction.ToString("P0"));
				System.Console.WriteLine("");
			}
			else
			{
				System.Console.WriteLine(message.Fraction.ToString("P0"));
			}
		}

		private static void OnFileAnalyzed(FileAnalyzedMessage message)
		{
			System.Console.WriteLine(string.Format("Analyzed file {0} of {1}: '{2}'", message.Number, message.Total, message.FullFilePath));
			if (message.Number == message.Total)
			{
				System.Console.WriteLine("");
			}
		}

		private static void OnBlobAnalyzed(BlobAnalyzedMessage message)
		{
			System.Console.WriteLine(string.Format("Analyzed blob {0} of {1}: '{2}'", message.Number, message.Total, message.FileRelativePath));
			if (message.Number == message.Total)
			{
				System.Console.WriteLine("");
			}
		}

		private static void ShowTryMessage()
		{
			System.Console.WriteLine("");
			System.Console.WriteLine("Try --help' for more information.");
		}

		private static void ShowHelp(OptionSet options)
		{
			System.Console.WriteLine("Usage: " + typeof(Program).Assembly.GetName().Name + " [OPTIONS]");
			System.Console.WriteLine();
			System.Console.WriteLine("Options:");
			options.WriteOptionDescriptions(System.Console.Out);
			System.Console.ReadKey();
		}
	}
}