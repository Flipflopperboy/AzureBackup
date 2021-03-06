﻿using Autofac;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Logging;
using Flip.AzureBackup.WindowsAzure;
using Flip.Common.Messages;



namespace Flip.AzureBackup.Console.Configuration
{
	public static class ContainerConfiguration
	{
		public static IContainer CreateContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance<ILog>(new TextWriterLog(System.Console.Out));
			builder.RegisterType<MessageBus>().As<IMessageBus>().SingleInstance();
			builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
			builder.RegisterType<CloudSyncEngine>().As<ISyncEngine>().SingleInstance();
			return builder.Build();
		}
	}
}
