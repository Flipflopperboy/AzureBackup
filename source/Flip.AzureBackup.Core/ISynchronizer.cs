namespace Flip.AzureBackup
{
	public interface ISynchronizer
	{
		void Synchronize(SyncronizationSettings settings);
	}
}
