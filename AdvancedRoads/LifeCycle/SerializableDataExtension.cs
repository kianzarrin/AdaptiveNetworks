namespace AdvancedRoads.LifeCycle
{
    using JetBrains.Annotations;
    using ICities;
    using System.IO;
    using UnityEngine;
    using KianCommons;

    [UsedImplicitly]
    public class SerializableDataExtension
        : SerializableDataExtensionBase
    {
        private const string DATA_ID = "AdvancedRoads_V1.0";

        public override void OnLoadData()
        {
            byte[] data = serializableDataManager.LoadData(DATA_ID);
            NetworkExtensionManager.Deserialize(data);
        }

        public override void OnSaveData()
        {
            byte[] data = NetworkExtensionManager.Serialize();
            serializableDataManager.SaveData(DATA_ID, data);
        }
    }
}
