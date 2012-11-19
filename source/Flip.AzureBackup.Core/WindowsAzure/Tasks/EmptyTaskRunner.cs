using Flip.Common.Threading;



namespace Flip.AzureBackup.WindowsAzure.Tasks
{
	public sealed class EmptyTaskRunner : TaskRunner
	{
		protected override bool LoopCondition()
		{
			return false;
		}
		protected override void LoopAction()
		{
		}
	}
}
