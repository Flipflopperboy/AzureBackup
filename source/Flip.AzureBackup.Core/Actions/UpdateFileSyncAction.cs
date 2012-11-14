using Flip.AzureBackup.IO;
using Flip.AzureBackup.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class UpdateFileSyncAction : ISyncAction
	{
		public UpdateFileSyncAction(IFileSystem fileSystem, FileInformation fileInfo, CloudBlob blob)
		{
			_fileSystem = fileSystem;
			_fileInfo = fileInfo;
			_blob = blob;
		}

		public void Invoke()
		{
			//this._statistics.UpdatedCount++;
			//this._logger.WriteLine("Downloading updated file " + blob.Uri.ToString() + "...");
			_blob.DownloadToFile(_fileInfo.FullPath);
			_fileSystem.SetLastWriteTimeUtc(_fileInfo.FullPath, _blob.GetFileLastModifiedUtc());
		}

		private readonly IFileSystem _fileSystem;
		private readonly FileInformation _fileInfo;
		private readonly CloudBlob _blob;
	}
}
