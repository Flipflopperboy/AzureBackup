using System;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.WindowsAzure
{
	internal static class CloudBlobContainerExtensions
	{
		public static Uri GetBlobUri(this CloudBlobContainer container, string path)
		{
			var uriBuilder = new UriBuilder(container.Uri);

			string stringToEscape = uriBuilder.Path.EndsWith(CloudBlobContainerExtensions.separator) ?
				path :
				separator + path;

			UriBuilder resultingUriBuilder = uriBuilder;
			resultingUriBuilder.Path += Uri.EscapeUriString(stringToEscape);
			return uriBuilder.Uri;
		}

		private const string separator = "/";
	}
}
