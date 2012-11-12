using System;
using Autofac;
using Flip.AzureBackup.Console.Configuration;
using NDesk.Options;



namespace Flip.AzureBackup.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			SyncronizationSettings settings;
			if (ParseSettings(args, out settings))
			{
				IContainer container = ContainerConfiguration.CreateContainer();
				var synchronizer = container.Resolve<ISynchronizer>();
				synchronizer.Synchronize(settings);
			}
			System.Console.ReadKey();
		}



		private static bool ParseSettings(string[] args, out SyncronizationSettings settings)
		{
			settings = null;
			var parsedSettings = new SyncronizationSettings();
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
						   SynchronizationAction action;
						   if (Enum.TryParse<SynchronizationAction>(v, out action))
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
				//System.Console.WriteLine("Valid values are [" + Enum.GetNames(typeof(SynchronizationAction)).ToSeparatedString("|") + "].");
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

		private static void ShowTryMessage()
		{
			System.Console.WriteLine("Try AzureBackup --help' for more information.");
		}

		private static void ShowHelp(OptionSet options)
		{
			System.Console.WriteLine("Usage: AzureBackup [OPTIONS]");
			System.Console.WriteLine();
			System.Console.WriteLine("Options:");
			options.WriteOptionDescriptions(System.Console.Out);
			System.Console.ReadKey();
		}
	}
}