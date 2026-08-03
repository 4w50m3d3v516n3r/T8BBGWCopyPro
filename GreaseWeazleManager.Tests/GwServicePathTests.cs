using System;
using System.IO;
using GwCopyPro.Services;
using Xunit;

namespace GreaseWeazleManager.Tests
{
    public class GwServicePathTests
    {
        [Fact]
        public void AbsoluteOutputFolder_IsUsedDirectly()
        {
            string f = GwService.ResolveOutputFile(@"C:\images", null, "d1.scp");
            Assert.Equal(@"C:\images\d1.scp", f);
        }

        [Fact]
        public void RelativeOutputFolder_ResolvesAgainstBaseDirectory()
        {
            string f = GwService.ResolveOutputFile("out", null, "d1.scp");
            Assert.Equal(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out", "d1.scp"), f);
        }

        [Fact]
        public void EmptyFolderWithRootedImageFile_UsesImageFileDirectory()
        {
            string f = GwService.ResolveOutputFile("", @"C:\old\prev.scp", "d2.scp");
            Assert.Equal(@"C:\old\d2.scp", f);
        }

        [Fact]
        public void EmptyFolderWithoutImageFile_FallsBackToDesktop()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string f = GwService.ResolveOutputFile("", null, "d3.scp");
            Assert.Equal(Path.Combine(desktop, "d3.scp"), f);
        }

        [Fact]
        public void RootedFileName_IsReturnedUnchanged()
        {
            string f = GwService.ResolveOutputFile(@"C:\images", null, @"D:\direct\d4.scp");
            Assert.Equal(@"D:\direct\d4.scp", f);
        }
    }
}
