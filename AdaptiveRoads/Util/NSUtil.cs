//namespace AdaptiveRoads.Util {
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Text;
//    using UnityEngine;
//    using KianCommons;
//    using static KianCommons.ReflectionHelpers;

//    public static class NSUtil {
//        static Type GetNSManType() => Type.GetType("NetworkSkins.Skins.NetworkSkinManager, NetworkSkins");
//        public static MonoBehaviour GetNSMan() => MonoBehaviour.FindObjectOfType(GetNSManType()) as MonoBehaviour;

//        static MonoBehaviour NSMan_;
//        public static MonoBehaviour NSMan => NSMan_ ??= GetNSMan();

//        static Array GetSegmentSkins() => GetFieldValue(GetNSMan(), "SegmentSkins") as Array;
//        static Array GetNodeSkins() => GetFieldValue(GetNSMan(), "NodeSkins") as Array;
//    }
//}
