using System;
using System.Collections.Generic;
using System.IO;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Messages;
using Flip.Common.Messages;
using Flip.Common.Threading;
using Microsoft.WindowsAzure.StorageClient;



namespace Flip.AzureBackup.WindowsAzure.Tasks
{
	public class UploadLargeFileTaskRunner : MessageBasedTaskRunner<SyncStartedMessage, SyncPausedMessage, SyncStoppedMessage>
	{
		public UploadLargeFileTaskRunner(IMessageBus messageBus, IFileSystem fileSystem, FileInformation fileInfo, CloudBlobContainer blobContainer)
			: base(messageBus)
		{
			_fileSystem = fileSystem;
			_fileInfo = fileInfo;
			_blobContainer = blobContainer;
			_numberOfParts = (int)Math.Ceiling((decimal)fileInfo.SizeInBytes / (decimal)CloudBlobConstants.MaxBlockSize);
			_rangeFraction = 1 / (decimal)_numberOfParts;
		}

		public UploadLargeFileTaskRunner(IMessageBus messageBus, IFileSystem fileSystem, FileInformation fileInfo, CloudBlob blob)
			: base(messageBus)
		{
			_fileSystem = fileSystem;
			_fileInfo = fileInfo;
			_blockBlob = blob.ToBlockBlob;
			_numberOfParts = (int)Math.Ceiling((decimal)fileInfo.SizeInBytes / (decimal)CloudBlobConstants.MaxBlockSize);
			_rangeFraction = 1 / (decimal)_numberOfParts;
		}



		protected override void OnFirstStarting()
		{
			base.OnFirstStarting();

			if (_blockBlob == null)
			{
				_blockBlob = _blobContainer.GetBlockBlobReference(_fileInfo.BlobName);
			}
			_blockBlob.SetFileLastModifiedUtc(_fileInfo.LastWriteTimeUtc, false);

			_messageBus.Publish(new FileProgressedMessage(_fileInfo.FullPath, 0));
		}

		protected override void OnStarting()
		{
			base.OnStarting();
			_stream = _fileSystem.OpenReadFileStream(_fileInfo.FullPath);
			_reader = new BinaryReader(_stream);
			_stream.Seek(_bytesSent, SeekOrigin.Begin);
		}

		protected override void OnPaused()
		{
			base.OnPaused();
			DisposeReaderAndStream();
		}

		protected override bool LoopCondition()
		{
			return _bytesSent < _fileInfo.SizeInBytes;
		}

		protected override void LoopAction()
		{
			if ((_bytesSent + _currentBlockSize) > _fileInfo.SizeInBytes)
			{
				_currentBlockSize = (int)_fileInfo.SizeInBytes - _bytesSent;
			}

			string blockId = _loopCounter.ToString().PadLeft(32, '0');
			byte[] bytes = _reader.ReadBytes(_currentBlockSize);
			using (var memoryStream = new MemoryStream(bytes))
			{
				_blockIds.Add(blockId);
				_blockBlob.PutBlock(blockId, memoryStream, bytes.GetMD5Hash());
			}
			_bytesSent += _currentBlockSize;
			_loopCounter++;
			_messageBus.Publish(new FileProgressedMessage(_fileInfo.FullPath, _loopCounter < _numberOfParts ? _rangeFraction * _loopCounter : 1));
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				DisposeReaderAndStream();
				try { _blockBlob.PutBlockList(_blockIds); }
				catch { }
			}
		}



		private void DisposeReaderAndStream()
		{
			try { _reader.Dispose(); }
			catch { }
			try { _stream.Dispose(); }
			catch { }
		}



		private int _loopCounter = 0;
		private int _bytesSent = 0;
		private int _numberOfParts = 0;
		private decimal _rangeFraction = 0;
		private List<string> _blockIds = new List<string>();
		private int _currentBlockSize = CloudBlobConstants.MaxBlockSize;
		private Stream _stream;
		private BinaryReader _reader;
		private CloudBlockBlob _blockBlob;

		private readonly IFileSystem _fileSystem;
		private readonly FileInformation _fileInfo;
		private readonly CloudBlobContainer _blobContainer;
	}
}
