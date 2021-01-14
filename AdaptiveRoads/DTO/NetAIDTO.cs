namespace AdaptiveRoads.DTO {
    public class RoadAIDTO {
        public bool m_trafficLights;
        public bool m_highwayRules;
        public bool m_accumulateSnow = true;
        public int m_noiseAccumulation = 10;
        public float m_noiseRadius = 40f;
        public float m_centerAreaWidth;
        public int m_constructionCost = 1000;
        public int m_maintenanceCost = 2;
        public string m_outsideConnection = null;
    }

    public class BridgeAIDTO : RoadAIDTO {
        public string m_bridgePillarInfo;
        public string m_middlePillarInfo;
        public int m_elevationCost = 2000;
        public float m_bridgePillarOffset;
        public float m_middlePillarOffset;
        public bool m_doubleLength;
        public bool m_canModify = true;
    }

    public class TunnelAIDTO : RoadAIDTO {
        public bool m_canModify = true;
    }
}
