using System.Collections.Generic;
using System.Linq;
using GwCopyPro.Models;
using Xunit;

namespace GreaseWeazleManager.Tests
{
    public class GroupModelsTests
    {
        private static DeviceGroupMember Member(string id, string port, string drive) => new()
        {
            Device = new GreaseWeazleDevice { Id = id, Name = "GW " + id, SerialPort = port },
            Drive  = drive
        };

        private static GroupRepetitiveJob Group(params DeviceGroupMember[] members) => new()
        {
            FilePattern    = "Disk_{n:D3}.scp",
            DateTimeFormat = "yyyyMMdd",
            Members        = new List<DeviceGroupMember>(members)
        };

        [Fact]
        public void Validate_RejectsFewerThanTwoMembers()
        {
            var err = GroupRepetitiveJob.Validate(new[] { Member("a1", "COM3", "0") });
            Assert.Equal("job_dlg.group_min", err);
        }

        [Fact]
        public void Validate_RejectsDuplicateDevice()
        {
            var err = GroupRepetitiveJob.Validate(new[]
                { Member("a1", "COM3", "0"), Member("a1", "COM3", "1") });
            Assert.Equal("job_dlg.group_dup_device", err);
        }

        [Fact]
        public void Validate_AcceptsTwoDistinctDevices()
        {
            var err = GroupRepetitiveJob.Validate(new[]
                { Member("a1", "COM3", "0"), Member("b2", "COM4", "0") });
            Assert.Null(err);
        }

        [Fact]
        public void PrepareBatch_AssignsSequentialNumbersInGroupOrder()
        {
            var g = Group(Member("a1", "COM3", "0"), Member("b2", "COM4", "1"));
            foreach (var m in g.Members) { m.IncludedThisBatch = true; m.Verified = true; }

            var batch = g.PrepareBatch();

            Assert.Equal(2, batch.Count);
            Assert.Equal(1, batch[0].DiskNumber);
            Assert.Equal("Disk_001.scp", batch[0].FileName);
            Assert.Equal(2, batch[1].DiskNumber);
            Assert.Same(g.Members[0], batch[0].Member);
            Assert.Equal(3, g.NextDiskNumber);
            Assert.Equal(1, g.BatchNumber);
        }

        [Fact]
        public void PrepareBatch_SkipsExcludedAndUnverifiedMembers()
        {
            var g = Group(Member("a1", "COM3", "0"), Member("b2", "COM4", "0"),
                          Member("c3", "COM5", "0"));
            g.Members[0].IncludedThisBatch = true; g.Members[0].Verified = true;
            g.Members[1].IncludedThisBatch = false; g.Members[1].Verified = true;
            g.Members[2].IncludedThisBatch = true; g.Members[2].Verified = false;

            var batch = g.PrepareBatch();

            Assert.Single(batch);
            Assert.Same(g.Members[0], batch[0].Member);
            Assert.Equal(2, g.NextDiskNumber);
        }

        [Fact]
        public void PrepareBatch_NeverReusesNumbersAcrossBatches()
        {
            var g = Group(Member("a1", "COM3", "0"), Member("b2", "COM4", "0"));
            foreach (var m in g.Members) { m.IncludedThisBatch = true; m.Verified = true; }

            var b1 = g.PrepareBatch();
            g.Members[1].LastBatchFailed = true;   // failure must not free number 2
            foreach (var m in g.Members) m.Verified = true;
            var b2 = g.PrepareBatch();

            Assert.Equal(new[] { 1, 2 }, b1.Select(a => a.DiskNumber));
            Assert.Equal(new[] { 3, 4 }, b2.Select(a => a.DiskNumber));
            Assert.Equal(2, g.BatchNumber);
        }
    }
}
