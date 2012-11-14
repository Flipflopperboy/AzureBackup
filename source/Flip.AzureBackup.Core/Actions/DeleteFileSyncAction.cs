using Flip.AzureBackup.IO;



namespace Flip.AzureBackup.Actions
{
	public sealed class DeleteFileSyncAction : ISyncAction
	{
		public DeleteFileSyncAction(IFileSystem fileSystem, FileInformation fileInfo)
		{
			_fileSystem = fileSystem;
			_fileInfo = fileInfo;
		}

		public void Invoke()
		{
			//this._statistics.BlobNotExistCount++;
			//this._logger.WriteLine("Deleting file " + fileInfo.FullPath + "...");
			_fileSystem.DeleteFile(_fileInfo.FullPath);
		}

		private readonly IFileSystem _fileSystem;
		private readonly FileInformation _fileInfo;
	}
}
