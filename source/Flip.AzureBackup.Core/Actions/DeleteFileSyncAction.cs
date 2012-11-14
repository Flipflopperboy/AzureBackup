﻿using Flip.AzureBackup.IO;



namespace Flip.AzureBackup.Actions
{
	public sealed class DeleteFileSyncAction : SyncAction
	{
		public DeleteFileSyncAction(IFileSystem fileSystem, FileInformation fileInfo)
		{
			_fileSystem = fileSystem;
			_fileInfo = fileInfo;
		}

		public override void Invoke()
		{
			ReportProgress(_fileInfo.FullPath, "Deleting file...", 0);
			_fileSystem.DeleteFile(_fileInfo.FullPath);
			ReportProgress(_fileInfo.FullPath, "File deleted.", 1);
		}

		private readonly IFileSystem _fileSystem;
		private readonly FileInformation _fileInfo;
	}
}