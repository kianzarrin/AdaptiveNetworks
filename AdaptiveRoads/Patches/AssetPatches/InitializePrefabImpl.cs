namespace AdaptiveRoads.Patches.AssetPatches {
    using HarmonyLib;
    using PrefabMetadata.API;
    using PrefabMetadata.Helpers;
    using KianCommons;
    using AdaptiveRoads.Manager;
    using System;
    using System.Linq;
    using AdaptiveRoads.Patches.metadata;
    using ColossalFramework;

    /// <summary>
    /// AssetImporterAssetImport.OnLoad removes original AR metadata from the orginal road.
    /// here we take a snapshot before the operation
    /// then we apply the snapshot after the operation.
    /// </summary>
    //[HarmonyPatch(typeof(PrefabCollection<NetInfo>), "InitializePrefabImpl")] // Generic patch does not work.
    public static class InitializePrefabImpl {
        static AssetData.NetInfoMetaData infoSnapshot_;
        static NetInfo replaceInfo_;

        public static void Prefix(string collection, object prefab, string replace) {
            try {
                var netInfo = prefab as NetInfo;
                if (!netInfo) return; // gaurd against other types of the generic class.
                if (replace.IsNullOrWhiteSpace()) return;
                //Log.Debug($"InitializePrefabImpl.Prefix(prefab={prefab}, replace={replace}) was called");
                replaceInfo_ = PrefabCollection<NetInfo>.FindLoaded(replace);
                if (!replaceInfo_) {
                    //Log.Debug($"replaceInfo does not exist. skip operation");
                    return;
                }

                //Instantiate.LogExtended(replaceInfo);
                //Instantiate.LogExtended(prefab);

                if (!replaceInfo_.IsAdaptive()) {
                    //Log.Debug($"replaceInfo is not AR road. skip operation");
                    return;
                }

                Log.Debug("POINT A");
                infoSnapshot_ = AssetData.NetInfoMetaData.Create(replaceInfo_);
            }catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public static void Postfix(string collection, object prefab, string replace) {
            try {
                var netInfo = prefab as NetInfo;
                if (!netInfo) return; // gaurd against other types of the generic class.
                if (replace.IsNullOrWhiteSpace()) return;
                //Log.Debug($"InitializePrefabImpl.Postfix(prefab={prefab}, replace={replace}) was called");

                if (infoSnapshot_ == null) {
                    //Log.Debug($"snapshot_ does not exist. skip operation");
                    return;
                }
                Assertion.Assert(replaceInfo_, "replaceInfo");

                //Instantiate.LogExtended(replaceInfo);
                //Instantiate.LogExtended(prefab);

                Log.Debug("POINT B");
                infoSnapshot_?.Apply(replaceInfo_);
                // uncomment to copy here too.
                // for the time being OnLoadPatch takes care of this.
                // but for compatiblity with road generator I might need to uncomment this.
                // infoSnapshot_?.Apply(prefab);
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        static void Finalizer(Exception __exception) {
            if (__exception != null)
                Log.Exception(__exception);
        }
    }
}


