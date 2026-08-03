using System.IO;
using GwCopyPro.Models;
using Xunit;

namespace GreaseWeazleManager.Tests
{
    public class JobPresetGroupTests
    {
        [Fact]
        public void GroupFields_SurviveSaveAndLoad()
        {
            var preset = new JobPreset
            {
                PresetName     = "Group",
                UseDeviceGroup = true
            };
            preset.GroupMembers.Add(new GroupMemberPreset
                { DeviceId = "a1b2c3d4", DeviceName = "GW Left", Drive = "0" });
            preset.GroupMembers.Add(new GroupMemberPreset
                { DeviceId = "e5f6a7b8", DeviceName = "GW Right", Drive = "b" });

            string path = Path.Combine(Path.GetTempPath(),
                Path.GetRandomFileName() + ".gwpreset");
            try
            {
                preset.SaveToFile(path);
                var loaded = JobPreset.LoadFromFile(path);

                Assert.True(loaded.UseDeviceGroup);
                Assert.Equal(2, loaded.GroupMembers.Count);
                Assert.Equal("a1b2c3d4", loaded.GroupMembers[0].DeviceId);
                Assert.Equal("GW Left",  loaded.GroupMembers[0].DeviceName);
                Assert.Equal("b",        loaded.GroupMembers[1].Drive);
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void OldPresetWithoutGroupFields_LoadsWithDefaults()
        {
            string path = Path.Combine(Path.GetTempPath(),
                Path.GetRandomFileName() + ".gwpreset");
            try
            {
                File.WriteAllText(path, "{\"PresetName\":\"Legacy\"}");
                var loaded = JobPreset.LoadFromFile(path);

                Assert.False(loaded.UseDeviceGroup);
                Assert.Empty(loaded.GroupMembers);
            }
            finally { File.Delete(path); }
        }
    }
}
