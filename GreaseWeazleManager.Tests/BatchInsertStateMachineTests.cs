using GwCopyPro.Services;
using Xunit;

namespace GreaseWeazleManager.Tests
{
    public class BatchInsertStateMachineTests
    {
        private static BatchInsertStateMachine Sm(params bool[] included)
            => new(included);

        [Fact]
        public void FirstIncludedMemberBlinksInitially()
        {
            var sm = Sm(true, true, true);
            Assert.Equal(0, sm.CurrentBlink);
            Assert.Equal(MemberInsertState.Blinking, sm.State(0));
            Assert.Equal(MemberInsertState.Waiting,  sm.State(1));
            Assert.False(sm.CanStart);
        }

        [Fact]
        public void InitiallyExcludedMemberIsSkipped()
        {
            var sm = Sm(false, true);
            Assert.Equal(1, sm.CurrentBlink);
            Assert.Equal(MemberInsertState.Excluded, sm.State(0));
        }

        [Fact]
        public void MarkDetected_AdvancesBlinkToNextMember()
        {
            var sm = Sm(true, true);
            sm.MarkDetected(0);
            Assert.Equal(MemberInsertState.DiskDetected, sm.State(0));
            Assert.Equal(1, sm.CurrentBlink);
        }

        [Fact]
        public void AllDetected_EnablesStart()
        {
            var sm = Sm(true, true);
            sm.MarkDetected(0);
            sm.MarkDetected(1);
            Assert.Null(sm.CurrentBlink);
            Assert.True(sm.CanStart);
        }

        [Fact]
        public void ExcludingBlinkingMember_AdvancesImmediately()
        {
            var sm = Sm(true, true, true);
            sm.SetIncluded(0, false);
            Assert.Equal(1, sm.CurrentBlink);
            Assert.Equal(MemberInsertState.Excluded, sm.State(0));
        }

        [Fact]
        public void ExcludingAllMembers_DisablesStart()
        {
            var sm = Sm(true, true);
            sm.SetIncluded(0, false);
            sm.SetIncluded(1, false);
            Assert.Null(sm.CurrentBlink);
            Assert.False(sm.CanStart);
        }

        [Fact]
        public void ReIncludedMember_AppendsToQueueEnd()
        {
            var sm = Sm(true, true, true);
            sm.SetIncluded(0, false);       // queue: 1, 2
            sm.MarkDetected(1);             // queue: 2
            sm.SetIncluded(0, true);        // queue: 2, 0
            Assert.Equal(2, sm.CurrentBlink);
            sm.MarkDetected(2);
            Assert.Equal(0, sm.CurrentBlink);
            Assert.Equal(MemberInsertState.Blinking, sm.State(0));
        }

        [Fact]
        public void ReIncludingDetectedThenExcludedMember_RequiresNewDetection()
        {
            var sm = Sm(true, true);
            sm.MarkDetected(0);
            sm.SetIncluded(0, false);
            sm.SetIncluded(0, true);
            Assert.NotEqual(MemberInsertState.DiskDetected, sm.State(0));
            Assert.False(sm.CanStart);
        }

        [Fact]
        public void StateChanged_FiresOnTransitions()
        {
            var sm = Sm(true, true);
            int fired = 0;
            sm.StateChanged += () => fired++;
            sm.MarkDetected(0);
            sm.SetIncluded(1, false);
            Assert.Equal(2, fired);
        }
    }
}
