namespace AdaptiveRoads.Data {
    using UnityEngine;

    public struct CornerPairData {
        public struct CornerData {
            public Vector3 Position;
            public Vector3 Direction;
        }
        public CornerData Left, Right;
        public bool smooth;
    }
}
