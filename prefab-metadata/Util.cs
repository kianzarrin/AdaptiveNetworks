namespace PrefabMetadata.Utils {
    using System;
    using System.Reflection;
    using UnityEngine;

    internal static class Util {
        internal static string NameOf(this Assembly asm) => asm.GetName().Name;

        internal static Version VersionOf(this Assembly asm) => asm.GetName().Version;
        internal static Version VersionOf(Type t) => t.Assembly.GetName().Version;
        internal static Version VersionOf(this object obj) => VersionOf(obj.GetType());

        internal static Version Take(this Version version, int fieldCount) =>
            new Version(version.ToString(fieldCount));

        internal static void CopyProperties(object target, object origin) {
            Assert(target.GetType().IsSubclassOf(origin.GetType()), $"'{target?.GetType()}' IsSubclassOf '{origin?.GetType()}'");
            FieldInfo[] fields = origin.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fields) {
                //Extensions.Log($"Copying field:<{fieldInfo.Name}> ...>");
                object value = fieldInfo.GetValue(origin);
                string strValue = value?.ToString() ?? "null";
                //Extensions.Log($"Got field value:<{strValue}> ...>");
                fieldInfo.SetValue(target, value);
                //Extensions.Log($"Copied field:<{fieldInfo.Name}> value:<{strValue}>");
            }
        }

        internal static void CopyProperties<T>(object target, object origin) {
            Assert(target is T, "target is T");
            Assert(origin is T, "origin is T");
            FieldInfo[] fields = typeof(T).GetFields();
            foreach (FieldInfo fieldInfo in fields) {
                //Extensions.Log($"Copying field:<{fieldInfo.Name}> ...>");
                object value = fieldInfo.GetValue(origin);
                //string strValue = value?.ToString() ?? "null";
                //Extensions.Log($"Got field value:<{strValue}> ...>");
                fieldInfo.SetValue(target, value);
                //Extensions.Log($"Copied field:<{fieldInfo.Name}> value:<{strValue}>");
            }
        }

        internal static void Assert(bool con, string m = "") {
            if (!con) throw new Exception("Assertion failed: " + m);
        }

        internal static ushort Clamp2U16(int value) => (ushort)Mathf.Clamp(value, 0, ushort.MaxValue);

        // no need to clone dynamic flags because its array is readonly.
        //internal static DynamicFlags Clone(ref this DynamicFlags flags) {
        //    FieldInfo f_flags = flags.GetType().GetField("m_flags", BindingFlags.NonPublic | BindingFlags.Instance);
        //    ulong[] m_flags = f_flags.GetValue(flags) as ulong[];
        //    m_flags = m_flags?.Clone() as ulong[];
        //    var ret = new DynamicFlags();
        //    f_flags.SetValue(ret, m_flags);
        //    return ret;
        //}
    }
}

