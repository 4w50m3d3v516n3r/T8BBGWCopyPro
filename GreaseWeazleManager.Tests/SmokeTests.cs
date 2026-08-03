using GwCopyPro.Models;
using Xunit;

namespace GreaseWeazleManager.Tests
{
    public class SmokeTests
    {
        [Fact]
        public void FilePattern_Expand_ReplacesCounterToken()
        {
            Assert.Equal("Disk_007.scp",
                FilePattern.Expand("Disk_{n:D3}.scp", 7, "yyyyMMdd"));
        }
    }
}
