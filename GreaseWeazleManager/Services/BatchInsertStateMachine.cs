using System;
using System.Collections.Generic;

namespace GwCopyPro.Services
{
    /// <summary>Visual state of one group member during the insert phase.</summary>
    public enum MemberInsertState
    {
        /// <summary>Included, queued behind the currently blinking drive.</summary>
        Waiting,
        /// <summary>This drive's LED is blinking — insert a disk here.</summary>
        Blinking,
        /// <summary>A disk was detected in this drive.</summary>
        DiskDetected,
        /// <summary>Excluded from the current batch.</summary>
        Excluded
    }

    /// <summary>
    /// Pure state machine for the batch insert phase: which drive blinks, which are
    /// verified, and when the batch may start. Owns no timers or processes — the
    /// dialog feeds it events and renders its state.
    /// </summary>
    public class BatchInsertStateMachine
    {
        private readonly bool[] _included;
        private readonly bool[] _detected;
        private readonly List<int> _queue = new();

        /// <summary>Raised after every state transition.</summary>
        public event Action? StateChanged;

        /// <summary>
        /// Initialises the machine. Queue order is index order; initially excluded
        /// members are not queued.
        /// </summary>
        /// <param name="initiallyIncluded">Per-member inclusion flags, in group order.</param>
        public BatchInsertStateMachine(IReadOnlyList<bool> initiallyIncluded)
        {
            _included = new bool[initiallyIncluded.Count];
            _detected = new bool[initiallyIncluded.Count];
            for (int i = 0; i < initiallyIncluded.Count; i++)
            {
                _included[i] = initiallyIncluded[i];
                if (_included[i]) _queue.Add(i);
            }
        }

        /// <summary>Index of the member whose drive should blink now, or <see langword="null"/>.</summary>
        public int? CurrentBlink => _queue.Count > 0 ? _queue[0] : null;

        /// <summary>Whether the batch may start: every included member verified, at least one included.</summary>
        public bool CanStart
        {
            get
            {
                if (_queue.Count > 0) return false;
                for (int i = 0; i < _included.Length; i++)
                    if (_included[i]) return true;
                return false;
            }
        }

        /// <summary>Whether the member takes part in this batch.</summary>
        public bool IsIncluded(int i) => _included[i];

        /// <summary>Current visual state of the member.</summary>
        public MemberInsertState State(int i)
        {
            if (!_included[i]) return MemberInsertState.Excluded;
            if (_detected[i])  return MemberInsertState.DiskDetected;
            return CurrentBlink == i ? MemberInsertState.Blinking : MemberInsertState.Waiting;
        }

        /// <summary>
        /// Includes or excludes a member. Excluding removes it from the blink queue;
        /// re-including clears any previous detection and appends it to the queue end.
        /// </summary>
        public void SetIncluded(int i, bool included)
        {
            if (_included[i] == included) return;
            _included[i] = included;
            if (included)
            {
                _detected[i] = false;
                _queue.Add(i);
            }
            else
            {
                _queue.Remove(i);
            }
            StateChanged?.Invoke();
        }

        /// <summary>Records that a disk was detected in the member's drive.</summary>
        public void MarkDetected(int i)
        {
            _detected[i] = true;
            _queue.Remove(i);
            StateChanged?.Invoke();
        }
    }
}
