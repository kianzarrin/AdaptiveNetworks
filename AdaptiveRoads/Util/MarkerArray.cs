namespace AdaptiveRoads.Util {
    using System.Collections;
    using System.Collections.Generic;

    public class MarkerArray : IEnumerable<ushort>, IEnumerator<ushort> {
        const int POW2_SIZE = 6;
        const int SIZE = 64;

        private ulong[] Updates;
        public bool HasUpdates { get; private set; }
        public readonly int MaxCount;
        bool clearing_ = false;

        public MarkerArray(int maxCount) {
            MaxCount = maxCount;
            HasUpdates = false;
            int n = maxCount >> POW2_SIZE;
            if(maxCount % SIZE != 0) n++;
            Updates = new ulong[n];
        }

        public void Mark(ushort id) {
            Updates[id >> POW2_SIZE] |= 1UL << (int)id;
            HasUpdates = true;
        }

        int maskIndex_;
        int bitIndex_;
        ulong bitMask_;
        public MarkerArray Iterate(bool clear = false) {
            Reset();
            clearing_ = clear;
            return this;
        }

        public bool MoveNext() {
            for(; maskIndex_ < Updates.Length; maskIndex_++) {
                if(bitMask_ == 0) {
                    bitMask_ = Updates[maskIndex_];
                    if(clearing_)
                        Updates[maskIndex_] = 0;
                }
                if(bitMask_ != 0) {
                    for(; bitIndex_ < 64; bitIndex_++) {
                        if((bitMask_ & 1UL << bitIndex_) != 0) {
                            Current = (ushort)(maskIndex_ << 6 | bitIndex_);
                            bitIndex_++;
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        public void Reset() {
            maskIndex_ = 0;
            bitIndex_ = 0;
            clearing_ = false;
        }

        public ushort Current { get; private set; }
        object IEnumerator.Current => Current;
        public MarkerArray GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        IEnumerator<ushort> IEnumerable<ushort>.GetEnumerator() => this;
        public void Dispose() { }

    }
}
