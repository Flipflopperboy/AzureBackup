namespace Flip.AzureBackup
{
	public interface ISyncEngine
	{
		void Sync(SyncSettings settings);
	}
}
