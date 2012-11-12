using Autofac;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;



namespace Flip.AzureBackup.Console.Configuration
{
	public static class ContainerConfiguration
	{
		public static IContainer CreateContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance<ILogger>(new TextWriterLogger(System.Console.Out));
			builder.RegisterType<FileAccessor>().As<IFileAccessor>().SingleInstance();
			builder.RegisterType<Synchronizer>().As<ISynchronizer>().SingleInstance();
			return builder.Build();
		}
	}
}
