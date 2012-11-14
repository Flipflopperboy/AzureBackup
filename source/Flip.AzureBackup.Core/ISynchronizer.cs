namespace Flip.AzureBackup
{
	public interface ISynchronizer
	{
		void Sync(AzureSyncSettings settings);
	}
}
