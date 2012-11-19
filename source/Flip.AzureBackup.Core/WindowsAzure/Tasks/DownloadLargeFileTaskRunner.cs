using System;
using System.IO;
using System.Net;
using Flip.AzureBackup.IO;
using Flip.AzureBackup.Messages;
using Flip.Common.Messages;
using Flip.Common.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.StorageClient.Protocol;



namespace Flip.AzureBackup.WindowsAzure.Tasks
{
	public sealed class DownloadLargeFileTaskRunner : MessageBasedTaskRunner<SyncStartedMessage, SyncPausedMessage, SyncStoppedMessage>
	{
		public DownloadLargeFileTaskRunner(IMessageBus messageBus, IFileSystem fileSystem, CloudBlob blob, string path)
			: base(messageBus)
		{
			_fileSystem = fileSystem;
			_blob = blob;
			_fullFilePath = path;
		}



		protected override void OnFirstStarting()
		{
			base.OnFirstStarting();

			_messageBus.Publish(new FileProgressedMessage(_fullFilePath, 0));

			_loopCounter = 0;
			_rangeStart = 0;
			_rangeEnd = 0;
			_blobLength = _blob.Properties.Length;
			_numberOfParts = (int)Math.Ceiling((decimal)_blobLength / (decimal)CloudBlobConstants.MaxBlockSize);
			_rangeFraction = 1 / (decimal)_numberOfParts;

			_fileSystem.EnsureFileDirectory(_fullFilePath);
			_fileStream = this._fileSystem.GetWriteFileStream(_fullFilePath);
		}

		protected override bool LoopCondition()
		{
			return _loopCounter < _numberOfParts;
		}

		protected override void LoopAction()
		{
			_rangeStart = _loopCounter * CloudBlobConstants.MaxBlockSize;
			_rangeEnd = _rangeStart + CloudBlobConstants.MaxBlockSize - 1;
			_rangeEnd = _rangeEnd > _blobLength ? _rangeStart + (int)_blobLength - _rangeStart - 1 : _rangeEnd;

			HttpWebRequest blobGetRequest = BlobRequest.Get(_blob.Uri, 60, null, null);
			blobGetRequest.Headers.Add("x-ms-range", string.Format(System.Globalization.CultureInfo.InvariantCulture, "bytes={0}-{1}", _rangeStart, _rangeEnd));

			StorageCredentials credentials = _blob.ServiceClient.Credentials;
			credentials.SignRequest(blobGetRequest);

			using (HttpWebResponse response = blobGetRequest.GetResponse() as HttpWebResponse)
			{
				using (Stream stream = response.GetResponseStream())
				{
					stream.CopyTo(_fileStream);
				}
			}
			_loopCounter++;
			_messageBus.Publish(new FileProgressedMessage(_fullFilePath, _loopCounter < _numberOfParts ? _rangeFraction * _loopCounter : 1));
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				try { _fileStream.Dispose(); }
				catch { }
				try { _fileSystem.SetLastWriteTimeUtc(_fullFilePath, _blob.GetFileLastModifiedUtc()); }
				catch { }
			}
		}




		private int _loopCounter;
		private int _rangeStart;
		private int _rangeEnd;
		private long _blobLength;
		private int _numberOfParts;
		private decimal _rangeFraction;
		private Stream _fileStream;
		private readonly IFileSystem _fileSystem;
		private readonly CloudBlob _blob;
		private readonly string _fullFilePath;
	}
}
