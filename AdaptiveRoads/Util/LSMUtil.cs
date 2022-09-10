namespace AdaptiveRoads.Util {
    using ColossalFramework.Packaging;
    using KianCommons;
    using KianCommons.Plugins;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using static ColossalFramework.Plugins.PluginManager;
    using static KianCommons.ReflectionHelpers;

    public static class LSMRevisited {
        const string LSM_REVISITED = "LoadingScreenModRevisited";
        const string LSM = "LSM";

        public static class Delegates {
            static TDelegate CreateDelegate<TDelegate>() where TDelegate : Delegate => DelegateUtil.CreateDelegate<TDelegate>(API);

            public delegate bool get_IsActive();
            public static get_IsActive getIsActive { get; } = CreateDelegate<get_IsActive>();

            public delegate Material GetMaterial(Package package, string checksum, bool isMain);
            public static GetMaterial GetMaterial_ { get; } = CreateDelegate<GetMaterial>();

            public delegate Mesh GetMesh(Package package, string checksum);
            public static GetMesh GetMesh_ { get; } = CreateDelegate<GetMesh>();

            public delegate Package GetPackageOf(NetInfo netInfo);
            public static GetPackageOf GetPackageOf_ { get; } = CreateDelegate<GetPackageOf>();
        }

        public static Type API { get; } = Type.GetType($"{LSM}.API, {LSM_REVISITED}");

        public static PluginInfo LSMRMod { get;  } = PluginUtil.GetPlugin(assembly: API?.Assembly);

        public static bool IsEnabled = LSMRMod?.isEnabled ?? false;

        public static bool IsActive => Delegates.getIsActive?.Invoke() ?? false;

        public static object InvokeAPIMethod(string methodName, params object[] args) =>
            API?.GetMethod(methodName).Invoke(null, args);

        public static Package GetPackageOf(NetInfo netInfo) => Delegates.GetPackageOf_?.Invoke(netInfo);

        public static Mesh GetMesh(string checksum, NetInfo netInfo) {
            Assertion.Assert(!checksum.IsNullorEmpty(), "checksum");
            if (checksum.IsNullorEmpty()) {
                return null;
            } else if (IsActive) {
                Log.DebugOnce("getting mesh from LSMRevisited API");
                var ret = Delegates.GetMesh_.Invoke(GetPackageOf(netInfo), checksum);
                if (ret == null) {
                    Log.Error($"failed to get mesh for {netInfo} with checksum={checksum}");
                }
                return ret;
            } else {
                return GetMesh(checksum);
            }
        }

        public static Material GetMaterial(string checksum, NetInfo netInfo, bool isLod) {
            Assertion.Assert(!checksum.IsNullorEmpty(), "checksum");
            if (IsActive) {
                // public static Material GetMaterial(Package package, string checksum)
                Log.DebugOnce("getting Material from LSMRevisited API");
                var ret = Delegates.GetMaterial_.Invoke(GetPackageOf(netInfo), checksum, isMain:!isLod);
                if(ret == null) {
                    Log.Error($"failed to get material for {netInfo} with checksum={checksum}");
                }
                return ret;
            } else {
                return GetMaterial(checksum);
            }
        }

        public static Material GetMaterial(string checksum) =>
            GetMaterial(checksum, PackageManagerUtil.GetLoadingPackages());

        public static Mesh GetMesh(string checksum) =>
            GetMesh(checksum, PackageManagerUtil.GetLoadingPackages());

        public static Mesh GetMesh(string checksum, IEnumerable<Package> packages) {
            if (checksum.IsNullorEmpty()) return null;
            foreach (var package in packages) {
                try {
                    Assertion.NotNull(package, "package");
                    Assertion.Assert(!checksum.IsNullorEmpty(), "checksum");
                    Mesh ret = package.FindByChecksum(checksum)?.Instantiate<Mesh>();
                    if (ret) {
                        Log.Debug($"loaded '{ret}' with checksum:({checksum}) from {package}");
                        return ret;
                    }
                } catch (Exception ex) {
                    ex.Log($"checksum={checksum.ToSTR()}, package={package.ToSTR()}", false);
                }
            }
            Log.Error($"could not find mesh with checksum:({checksum}) from {packages.ToSTR()}");
            return null;
        }

        public static Material GetMaterial(string checksum, IEnumerable<Package> packages) {
            if (checksum.IsNullorEmpty()) return null;
            foreach (var package in packages) {
                try {
                    Assertion.NotNull(package, "package");
                    Assertion.Assert(!checksum.IsNullorEmpty(), "checksum");
                    Material ret = package.FindByChecksum(checksum)?.Instantiate<Material>();
                    if (ret) {
                        Log.Debug($"loaded '{ret}' with checksum:({checksum}) from {package}");
                        return ret;
                    }
                } catch (Exception ex) {
                    ex.Log($"checksum={checksum.ToSTR()}, package={package.ToSTR()}", false);
                }
            }
            Log.Error($"could not find material with checksum:({checksum}) from {packages.ToSTR()}");
            return null;
        }
    }
}
