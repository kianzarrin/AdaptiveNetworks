namespace PrefabMetadata.Helpers {
    using PrefabMetadata.API;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using UnityEngine;

    public static class PrefabMetadataHelpers {
        /// <summary>
        /// returns the latest version of PrefabMetadata.dll in the app domain
        /// </summary>
        public static Assembly GetLatestAssembly(bool throwOnError=true) {
            Assembly ret = null;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies) {
                if (assembly.GetName().Name != "PrefabMetadata")
                    continue;
                if (ret == null || ret.GetName().Version < assembly.GetName().Version) {
                    ret = assembly;
                }
            }
            if (ret == null && throwOnError) {
                string sAssemblies = string.Join("\n", assemblies.Select(asm => asm.ToString()).ToArray());
                throw new Exception("failed to get latest PrefabMetadata. assemblies are:\n" + sAssemblies);
            }
            return ret;
        }

        /// <summary>
        /// returns an extended clone of <paramref name="info"/>
        /// that can accept metadata.
        /// </summary>
        public static IInfoExtended<NetInfo.Segment> Extend(this NetInfo.Segment info) {
            MethodInfo m =
                 GetLatestAssembly()
                .GetType(typeof(NetInfoMetaDataExtension.Segment).FullName, throwOnError: true)
                .GetMethod(nameof(NetInfoMetaDataExtension.Segment.Extend));
            if (m == null) throw new Exception("could not get NetInfoMetaDataExtension.Segment.Extend()");
            return m.Invoke(null, new[] { info }) as IInfoExtended<NetInfo.Segment>;
            //return NetInfoMetaDataExtension.Segment.Extend(info);
        }

        /// <summary>
        /// returns an extended clone of <paramref name="info"/>
        /// that can accept metadata.
        /// </summary>
        public static IInfoExtended<NetInfo.Node> Extend(this NetInfo.Node info) {
            MethodInfo m = GetLatestAssembly()
                .GetType(typeof(NetInfoMetaDataExtension.Node).FullName, throwOnError: true)
                .GetMethod(nameof(NetInfoMetaDataExtension.Node.Extend));
            return m.Invoke(null, new[] { info }) as IInfoExtended<NetInfo.Node>;
            //return NetInfoMetaDataExtension.Node.Extend(info);
        }

        /// <summary>
        /// returns an extended clone of <paramref name="info"/>
        /// that can accept metadata.
        /// </summary>
        public static IInfoExtended<NetLaneProps.Prop> Extend(this NetLaneProps.Prop info) {
            MethodInfo m =
                GetLatestAssembly()
               .GetType(typeof(NetInfoMetaDataExtension.LaneProp).FullName, throwOnError: true)
               .GetMethod(nameof(NetInfoMetaDataExtension.LaneProp.Extend));
                return m.Invoke(null, new[] { info }) as IInfoExtended<NetLaneProps.Prop>;
            //return NetInfoMetaDataExtension.LaneProp.Extend(info);
        }

        /// <summary>
        /// returns an extended clone of <paramref name="info"/> that can accept metadata.
        /// returns null if info is unsupported type.
        /// </summary>
        public static IInfoExtended Extend(object info) {
            if (info.GetType() == typeof(NetInfo.Segment))
                return Extend(info as NetInfo.Segment);
            if (info.GetType() == typeof(NetInfo.Node))
                return Extend(info as NetInfo.Node);
            if (info.GetType() == typeof(NetLaneProps.Prop))
                return Extend(info as NetLaneProps.Prop);
            return null;
        }

        public static MetaDataType GetMetaData<MetaDataType>(this IInfoExtended info)
            where MetaDataType : class {
            if (info.MetaData != null) {
                foreach (var item in info.MetaData) {
                    if (item is MetaDataType ret)
                        return ret;
                }
            }
            return null;
        }

        /// <summary>
        /// addas <paramref name="data"/> to <paramref name="info"/>.
        /// if <paramref name="info"/> already has a meta data of the same type, it will be replaced.
        /// </summary>
        public static void SetMetaData<MetaDataType>(this IInfoExtended info, MetaDataType data)
            where MetaDataType : class, ICloneable {
            if (data == null)
                return;
            if (info.MetaData == null)
                info.MetaData = new List<ICloneable>();
            var list = info.MetaData;
            for (int i = 0; i < list.Count; ++i) {
                if (list[i] is MetaDataType) {
                    list[i] = data;
                }
            }
            list.Add(data);
        }

        public static List<ICloneable> Clone(this List<ICloneable> list) {
            var ret = new List<ICloneable>(list);
            for (int i = 0; i < list.Count; ++i) {
                list[i] = list[i].Clone() as ICloneable;
            }
            return ret;
        }

        public static int IndexOf(Array array, object value)
            => (array as IList).IndexOf(value);
    }
}

