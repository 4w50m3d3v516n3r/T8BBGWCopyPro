using GwCopyPro.Services;
using Xunit;

namespace GreaseWeazleManager.Tests
{
    public class DriveProberTests
    {
        [Fact]
        public void InterpretProbeOutput_RpmLine_MeansDiskPresent()
        {
            var r = DriveProber.InterpretProbeOutput(0,
                "Opened /dev/COM3\nDrive 0: Motor spun up\nDrive 0: 300.12 RPM");
            Assert.Equal(DiskProbeResult.DiskPresent, r);
        }

        [Fact]
        public void InterpretProbeOutput_NoIndex_MeansNoDisk()
        {
            var r = DriveProber.InterpretProbeOutput(1,
                "Drive 0: No index pulses detected");
            Assert.Equal(DiskProbeResult.NoDisk, r);
        }

        [Fact]
        public void InterpretProbeOutput_NonZeroExitWithoutOutput_MeansDeviceError()
        {
            var r = DriveProber.InterpretProbeOutput(1, "Cannot open serial port COM3");
            Assert.Equal(DiskProbeResult.DeviceError, r);
        }

        [Fact]
        public void InterpretProbeOutput_ZeroExitWithoutRpm_MeansNoDisk()
        {
            var r = DriveProber.InterpretProbeOutput(0, "Drive 0: Motor spun up");
            Assert.Equal(DiskProbeResult.NoDisk, r);
        }
    }
}
