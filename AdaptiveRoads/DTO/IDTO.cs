using System.Collections.Generic;

namespace AdaptiveRoads.DTO {
    public interface IDTO {
        void ReadFromGame(NetInfo gameNetInfo);
        void WriteToGame(NetInfo gameNetInfo);
    }

    public interface ISerialziableDTO {
        void Save();
        void OnLoaded();
    }
}