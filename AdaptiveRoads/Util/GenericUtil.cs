namespace AdaptiveRoads.Util {
    using AdaptiveRoads.Manager;
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


        public static IMetaData GetMetaData(object item) {
            return item switch {
                NetInfo.Node node => node.GetMetaData(),
                NetInfo.Segment segment => segment.GetMetaData(),
                NetLaneProps.Prop prop => prop.GetMetaData(),
                NetInfo netInfo => netInfo.GetMetaData(),
                NetAI netAI => netAI.m_info?.GetMetaData(),
                _ => null,
            };
        }
    }
}
