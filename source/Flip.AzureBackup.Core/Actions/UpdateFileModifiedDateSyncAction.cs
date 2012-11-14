using Flip.AzureBackup.IO;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.Actions
{
	public sealed class UpdateFileModifiedDateSyncAction : ISyncAction
	{
		public UpdateFileModifiedDateSyncAction(IFileSystem fileSystem, FileInformation fileInfo, CloudBlob blob)
		{
			_fileSystem = fileSystem;
			_fileInfo = fileInfo;
			_blob = blob;
		}

		public void Invoke()
		{
			//this._statistics.UpdatedModifiedDateCount++;
			//this._logger.WriteLine("Updating modification date for file " + fileInfo.FullPath + "...");
			this._fileSystem.SetLastWriteTimeUtc(_fileInfo.FullPath, _blob.GetFileLastModifiedUtc());
		}

		private readonly IFileSystem _fileSystem;
		private readonly FileInformation _fileInfo; 
		private readonly CloudBlob _blob;
	}
}
