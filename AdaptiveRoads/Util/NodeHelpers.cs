namespace AdaptiveRoads.Util {
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    public static class NodeHelpers {
        public static NetInfo.Node[] Clone(this NetInfo.Node[] src) =>
            src.Select(node => node.Clone()).ToArray();

        public static NetInfo.Node Clone(this NetInfo.Node node) {
            if (node is ICloneable cloneable)
                return cloneable.Clone() as NetInfo.Node;
            else
                return node.ShalowClone();
        }
    }
}
