using Autofac;
using Flip.AzureBackup.Console.Configuration;
using NDesk.Options;



namespace Flip.AzureBackup.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			Arguments arguments;
			if (ParseArguments(args, out arguments))
			{
				IContainer container = ContainerConfiguration.CreateContainer();
				var synchronizer = container.Resolve<ISynchronizer>();
				synchronizer.Synchronize(arguments.CloudConnectionString, arguments.ContainerName, arguments.DirectoryPath);
			}
			System.Console.ReadKey();
		}



		private static bool ParseArguments(string[] args, out Arguments arguments)
		{
			arguments = null;
			var parsedArguments = new Arguments();

			var options = new OptionSet();
			options.Add("h|help", "Shows help page", v => ShowHelp(options))
					.Add("c|connectionString=", "Storage account connection string", v => parsedArguments.CloudConnectionString = v)
				   .Add("b|blobContainer=", "Azure blob container name", v => parsedArguments.ContainerName = v)
				   .Add("d|directory=", "Absolute path to directory to synchronize", v => parsedArguments.DirectoryPath = v);

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

			bool argumentMissing = false;
			if (string.IsNullOrEmpty(parsedArguments.CloudConnectionString))
			{
				System.Console.WriteLine("Missing argument 'c|connectionString'.");
				argumentMissing = true;
			}

			if (string.IsNullOrEmpty(parsedArguments.ContainerName))
			{
				System.Console.WriteLine("Missing argument 'b|blobContainer'.");
				argumentMissing = true;
			}

			if (string.IsNullOrEmpty(parsedArguments.DirectoryPath))
			{
				System.Console.WriteLine("Missing argument 'd|directory'.");
				argumentMissing = true;
			}

			if (argumentMissing)
			{
				ShowTryMessage();
				System.Console.ReadKey();
				return false;
			}

			arguments = parsedArguments;

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



		private class Arguments
		{
			public string ContainerName { get; set; }
			public string DirectoryPath { get; set; }
			public string CloudConnectionString { get; set; }
		}
	}
}