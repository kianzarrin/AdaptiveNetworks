namespace AdaptiveRoads.Data {
    using ColossalFramework.Math;
    using UnityEngine;
    using KianCommons.Math;
    public struct CornerTripleData {
        public Vector3 Left, Center, Right;
        public Vector3 Direction;

        public void Set(Vector3 centerPos, Vector3 dir, float width, bool start) {
            Direction = dir;
            Center = centerPos;

            dir = VectorUtils.NormalizeXZ(dir);
            var normalLeft = new Vector3(-dir.z, 0, dir.x);
            if(!start) normalLeft = -normalLeft;

            Left = centerPos + normalLeft * width * 0.5f;
            Right = centerPos - normalLeft * width * 0.5f;
        }
    }

    public struct CornerPairData {
        public struct CornerData {
            public Vector3 Position;
            public Vector3 Direction;
        }
        public CornerData Left, Right;
        public bool smooth;
    }


    
}
