using Xunit;



namespace Flip.AzureBackup.WindowsAzure.Tests
{
	public sealed class LongExtensionTests
	{
		[Fact]
		public void TestRoundUpToMultipleOf()
		{
			Assert.Equal(512, (10L).RoundUpToMultipleOf(512));
			Assert.Equal(512, (512L).RoundUpToMultipleOf(512));
			Assert.Equal(1024, (513L).RoundUpToMultipleOf(512));
			Assert.Equal(1024, (1024L).RoundUpToMultipleOf(512));
		}

		[Fact]
		public void MBToBytes()
		{
			Assert.Equal(1048576, (1L).MBToBytes());
			Assert.Equal(2097152, (2L).MBToBytes());
		}
	}
}
