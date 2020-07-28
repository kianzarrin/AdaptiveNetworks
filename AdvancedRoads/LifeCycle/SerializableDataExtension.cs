
namespace AdvancedRoads.LifeCycle
{
    using JetBrains.Annotations;
    using ICities;
    using AdvancedRoads.Tool;
    using AdvancedRoads.GUI;
    using System.IO;
    using UnityEngine;
    using AdvancedRoads.Util;

    [UsedImplicitly]
    public class SerializableDataExtension
        : SerializableDataExtensionBase
    {
        private const string DATA_ID = "AdvancedRoads_V1.0";
        const string FILE_NAME = "AdvancedRoads.xml";
        static string global_config_path_ = Path.Combine(Application.dataPath, FILE_NAME);

        public override void OnLoadData()
        {
            byte[] data = serializableDataManager.LoadData(DATA_ID) ??
                serializableDataManager.LoadData("RoadTransitionManager_V1.0");
            NetworkExtManager.Deserialize(data);
        }

        public override void OnSaveData()
        {
            byte[] data = NetworkExtManager.Serialize();
            serializableDataManager.SaveData(DATA_ID, data);
        }
    }
}
