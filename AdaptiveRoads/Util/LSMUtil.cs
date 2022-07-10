namespace AdaptiveRoads.Util {
    using ColossalFramework.Packaging;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using static KianCommons.ReflectionHelpers;
    using static LSMUtil;

    public static class LSMUtil {
        public const string LSM_REVISITED = "LoadingScreenModRevisited";
        public const string LSM_KLYTE = "LoadingScreenModKlyte";
        public const string LSM_TEST = "LoadingScreenModTest";
        public const string LSM = "LoadingScreenMod";

        internal static Assembly GetLSMAssembly() =>
            AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(_asm => _asm.GetName().Name == LSM || _asm.GetName().Name == LSM_TEST);

        // find type without assembly resolve failure log spam.
        internal static Type FindTypeSafe(string typeName, string assemblyName) {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.Name() == assemblyName);
            return asm?.GetType(typeName, throwOnError: false);
        }

        /// <param name="type">full type name minus assembly name and root name space</param>
        /// <returns>corresponding types from LSM or LSMTest or both</returns>
        public static IEnumerable<Type> GetTypeFromLSMs(string type) {
            Type ret;
            ret = FindTypeSafe($"{LSM}.{type}", LSM_KLYTE);
            if (ret != null) yield return ret;

            ret = FindTypeSafe($"{LSM}.{type}", LSM_REVISITED);
            if (ret != null) yield return ret;

            ret = FindTypeSafe($"{LSM_REVISITED}.{type}", LSM_REVISITED);
            if (ret != null) yield return ret;

            ret = FindTypeSafe($"{LSM}.{type}", LSM);
            if (ret != null) yield return ret;

            ret = FindTypeSafe($"{LSM_TEST}.{type}", LSM_TEST);
            if (ret != null) yield return ret;
        }

        public static object GetSharing() {
            foreach (var type in GetTypeFromLSMs("Sharing")) {
                object sharing = AccessTools.Field(type, "inst").GetValue(null);
                if (sharing != null) {
                    Log.DebugOnce($"sharing found in '{type.Assembly.Name()}::{type}'");
                    return sharing;
                } else {
                    Log.DebugOnce($"sharing is empty in '{type.Assembly.Name()}::{type}'");
                }
            }
            Log.DebugOnce("LSM sharing NOT found!");
            return null;
        }

        public static Mesh GetMesh(object sharing, string checksum, IEnumerable<Package> packages, bool isLod) {
            if (checksum.IsNullorEmpty()) return null;
            foreach (var package in packages) {
                try {
                    Assertion.NotNull(package, "package");
                    Assertion.Assert(!checksum.IsNullorEmpty(), "checksum");
                    Mesh ret = null;
                    if (sharing == null) {
                        ret = package.FindByChecksum(checksum)?.Instantiate<Mesh>();
                    } else {
                        bool isMain = !isLod;
                        try {
                            ret = InvokeMethod(sharing, "GetMesh", checksum, package, isMain) as Mesh;
                        } catch (Exception ex) {
                            ex.Log($"sharing={sharing.ToSTR()}checksum={checksum.ToSTR()}, package={package.ToSTR()}, isMain={isMain}", false);
                        }
                        if (ret == null) {
                            ret = package.FindByChecksum(checksum)?.Instantiate<Mesh>();
                            if (ret != null) {
                                Log.Warning("Failed to use LSM to reduce MEMORY SIZE for mesh with checksum: " + checksum);
                            }
                        }
                    }
                    if (ret) {
                        Log.Debug($"loaded {ret} with checksum:({checksum}) from {package}");
                        return ret;
                    }
                } catch (Exception ex) {
                    ex.Log($"sharing={sharing.ToSTR()}checksum={checksum.ToSTR()}, package={package.ToSTR()}", false);
                }
            }
            Log.Error($"could not find mesh with checksum:({checksum}) from {packages.ToSTR()}");
            return null;
        }

        public static Material GetMaterial(object sharing, string checksum, IEnumerable<Package> packages, bool isLod) {
            if (checksum.IsNullorEmpty()) return null;
            foreach (var package in packages) {
                try {
                    Assertion.NotNull(package, "package");
                    Assertion.Assert(!checksum.IsNullorEmpty(), "checksum");
                    Material ret = null;
                    if (sharing == null) {
                        ret = package.FindByChecksum(checksum)?.Instantiate<Material>();
                    } else {
                        bool isMain = !isLod;
                        try {
                            ret = InvokeMethod(sharing, "GetMaterial", checksum, package, isMain) as Material;
                        } catch (Exception ex) {
                            ex.Log($"sharing={sharing.ToSTR()} checksum={checksum.ToSTR()}, package={package.ToSTR()}, isMain={isMain}", false);
                        }
                        if (ret == null) {
                            ret = package.FindByChecksum(checksum)?.Instantiate<Material>();
                            if (ret != null) {
                                Log.Warning("Failed to use LSM to reduce MEMORY SIZE for material with checksum: " + checksum);
                            }
                        }
                    }
                    if (ret) {
                        Log.Debug($"loaded {ret} with checksum:({checksum}) from {package}");
                        return ret;
                    }
                } catch (Exception ex) {
                    ex.Log($"sharing={sharing.ToSTR()}checksum={checksum.ToSTR()}, package={package.ToSTR()}", false);
                }
            }
            Log.Error($"could not find material with checksum:({checksum}) from {packages.ToSTR()}");
            return null;
        }

        public static Material GetMaterial(string checksum, bool isLod) =>
            GetMaterial(GetSharing(), checksum, PackageManagerUtil.GetLoadingPackages(), isLod);

        public static Mesh GetMesh(string checksum, bool isLod) =>
            GetMesh(GetSharing(), checksum, PackageManagerUtil.GetLoadingPackages(), isLod);

    }

    public static class LSMRevisited {
        public static class Delegates {
            static TDelegate CreateDelegate<TDelegate>() where TDelegate : Delegate => DelegateUtil.CreateDelegate<TDelegate>(API);

            public delegate bool get_IsActive();
            public static get_IsActive getIsActive { get; } = CreateDelegate<get_IsActive>();

            public delegate Material GetMaterial(Package package, string checksum, bool isMain);

            public static GetMaterial GetMaterial_ { get; } = CreateDelegate<GetMaterial>();

            public delegate Mesh GetMesh(Package package, string checksum, bool isMain);
            public static GetMesh GetMesh_ { get; } = CreateDelegate<GetMesh>();

            public delegate Package GetPackageOf(NetInfo netInfo);
            public static GetPackageOf GetPackageOf_ { get; } = CreateDelegate<GetPackageOf>();
        }

        public static Type API { get; } = Type.GetType($"{LSM_REVISITED}.API, {LSM_REVISITED}");

        public static bool IsActive => Delegates.getIsActive?.Invoke() ?? false;

        public static object InvokeAPIMethod(string methodName, params object[] args) =>
            API?.GetMethod(methodName).Invoke(null, args);

        public static Package GetPackageOf(NetInfo netInfo) => Delegates.GetPackageOf_?.Invoke(netInfo);

        public static Mesh GetMesh(string checksum, NetInfo netInfo, bool isLod) {
            if (IsActive) {
                Log.DebugOnce("getting mesh from LSMRevisited API");
                return Delegates.GetMesh_?.Invoke(GetPackageOf(netInfo), checksum, !isLod);
            } else {
                return LSMUtil.GetMesh(checksum, isLod);
            }
        }

        public static Material GetMaterial(string checksum, NetInfo netInfo, bool isLod) {
            if (IsActive) {
                // public static Material GetMaterial(Package package, string checksum, bool isMain)
                Log.DebugOnce("getting Material from LSMRevisited API");
                return Delegates.GetMaterial_?.Invoke(GetPackageOf(netInfo), checksum, !isLod);
            } else {
                return LSMUtil.GetMaterial(checksum, isLod);
            }
        }
    }
}
