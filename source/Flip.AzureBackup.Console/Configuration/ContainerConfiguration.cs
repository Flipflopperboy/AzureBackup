using Autofac;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.WindowsAzure;



namespace Flip.AzureBackup.Console.Configuration
{
	public static class ContainerConfiguration
	{
		public static IContainer CreateContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance<ILog>(new TextWriterLog(System.Console.Out));
			builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
			builder.RegisterType<AzureSyncEngine>().As<ISyncEngine>().SingleInstance();
			builder.RegisterType<CloudBlobStorage>().As<ICloudBlobStorage>().SingleInstance();
			return builder.Build();
		}
	}
}
