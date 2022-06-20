namespace AdaptiveRoads.Util {
    using KianCommons;
    using PrefabMetadata.API;
    using PrefabMetadata.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class GenericUtil {
        public static IInfoExtended Extend(object item) {
            return item switch {
                NetInfo.Node node => node.Extend(),
                NetInfo.Segment segment => segment.Extend(),
                NetLaneProps.Prop prop => prop.Extend(),
                _ => throw new ArgumentException("bad item:" + item.ToSTR())
            };
        }
    }
}
