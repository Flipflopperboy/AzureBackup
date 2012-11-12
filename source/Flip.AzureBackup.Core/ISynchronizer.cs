namespace Flip.AzureBackup
{
	public interface ISynchronizer
	{
		void Synchronize(string cloudConnectionString, string containerName, string directoryPath);
	}
}
