namespace AdaptiveRoads.LifeCycle
{
    using JetBrains.Annotations;
    using ICities;
    using System;
    using AdaptiveRoads.Manager;
    using KianCommons;

    [UsedImplicitly]
    public class SerializableDataExtension
        : SerializableDataExtensionBase
    {
        private const string DATA_ID = "AdaptiveRoads_V1.0";

        public override void OnLoadData()
        {
            Log.Info("OnLoadData() ...", true);
            byte[] data = serializableDataManager.LoadData(DATA_ID);
            NetworkExtensionManager.Deserialize(data, new Version(1,0));
            Log.Info("OnLoadData() was successful!", true);

        }

        public override void OnSaveData()
        {
            Log.Info("OnSaveData() ...", true);
            byte[] data = NetworkExtensionManager.Serialize();
            serializableDataManager.SaveData(DATA_ID, data);
            Log.Info("OnSaveData() was successful!", true);
        }
    }
}
