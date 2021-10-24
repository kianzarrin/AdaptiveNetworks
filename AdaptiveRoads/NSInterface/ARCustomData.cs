namespace AdaptiveRoads.NSInterface {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AdaptiveRoads.Manager;

    public class ARCustomData : ICloneable {
        public NetSegmentExt.Flags SegmentExtFlags;
        public object Clone() => MemberwiseClone();

        public override bool Equals(object obj) {
            return obj is ARCustomData data && SegmentExtFlags.Equals(data.SegmentExtFlags);
        }

        public override int GetHashCode() {
            return SegmentExtFlags.GetHashCode();
        }
    }
}
