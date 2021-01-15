using KianCommons;
using System;
using System.Linq;
using System.Reflection;
using static KianCommons.ReflectionHelpers;
using System.Collections.Generic;

namespace AdaptiveRoads.DTO {
    // TODO move to Kian commons.
    internal static class DTOUtil {
        public static TargetElementT[] CopyArray<TargetElementT>(object []source) {
            return source.Select(item => (TargetElementT)item).ToArray();
        }

        public static void CopyAllMatchingFields<T>(object target, object source) {
            if (source == null || target == null)
                return;
            FieldInfo[] fields = typeof(T).GetFields();
            foreach (FieldInfo fieldInfo in fields) {
                string fieldName = fieldInfo.Name;
                var originFieldInfo = source.GetType().GetField(fieldName, ALL);
                var targetFieldInfo = target.GetType().GetField(fieldName, ALL);
                if (originFieldInfo != null && targetFieldInfo != null) {
                    object value = null;
                    try {
                        value = originFieldInfo.GetValue(source);
                        targetFieldInfo.SetValue(target, value);
                    } catch {
                        try {
                            Type targetType = targetFieldInfo.FieldType;
                            if (TryConvert(value, targetType, out object value2)) {
                                targetFieldInfo.SetValue(target, value2);
                            }
                        } catch (Exception ex) {
                            Log.Exception(ex);
                        }
                    }
                }
            }
        }

        public static bool TryConvert(object sourceValue, Type targetType, out object targetValue) {
            Type sourceType = sourceValue.GetType();
            MethodBase convertor = targetType.GetConstructor(sourceType);
            targetValue = convertor?.Invoke(sourceValue);
            return targetValue != null;
        }

        public static ConstructorInfo GetConstructor(this Type type, params Type[] ParameterTypes) =>
            type.GetConstructor(ALL, null, ParameterTypes, null);
        public static object Invoke(this MethodBase method, params object[] parameters) =>
            method.Invoke(parameters);

        public static object GetConverter(Type sourceType, Type targetType) {
            MethodBase ret = null;
            ret = ret ?? sourceType.GetConverter(sourceType, targetType);
            ret = ret ?? targetType.GetConverter(sourceType, targetType);
            ret = ret ?? targetType.GetConstructor(sourceType);
            return ret;
        }
        public static MethodBase GetConverter(this Type type, Type sourceType, Type targetType) {
            return type.GetMethods(ALL).Where(m =>
               (m.Name == "op_implicit" || m.Name == "op_explicit") &&
               m.ReturnType == targetType &&
               m.GetParameters()[0].ParameterType == sourceType)
                .FirstOrDefault();
        }

        public static RoadAssetInfo LoadAllRoads() {
            throw new NotImplementedException();
        }
    }

}
