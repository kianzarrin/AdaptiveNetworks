namespace AdaptiveRoads.Data.NetworkExtensions {
    using ColossalFramework.Math;
    using UnityEngine;

    public struct OutlineData {
        public Bezier3 Center, Left, Right;
        public Vector3 DirA, DirD;
        public bool SmoothA, SmoothD;

        public OutlineData(Vector3 a, Vector3 d, Vector3 dirA, Vector3 dirD, float width, bool smoothA, bool smoothD) {
            {
                SmoothA = smoothA;
                Center.a = a;
                DirA = dirA;
                var normal = new Vector3(-dirA.z, 0, dirA.x);
                normal = VectorUtils.NormalizeXZ(normal);
                Left.a = a + normal * width * 0.5f;
                Right.a = a - normal * width * 0.5f;
            }
            {
                SmoothD = smoothD;
                DirD = dirD;
                Center.d = d;
                var normal = new Vector3(-dirD.z, 0, dirD.x);
                normal = -VectorUtils.NormalizeXZ(normal);
                Left.d = d + normal * width * 0.5f;
                Right.d = d - normal * width * 0.5f;
            }
            NetSegment.CalculateMiddlePoints(Center.a, DirA, Center.d, DirD, SmoothA, SmoothD, out Center.b, out Center.c);
            NetSegment.CalculateMiddlePoints(Left.a, DirA, Left.d, DirD, SmoothA, SmoothD, out Left.b, out Left.c);
            NetSegment.CalculateMiddlePoints(Right.a, DirA, Right.d, DirD, SmoothA, SmoothD, out Right.b, out Right.c);
        }
    }
}
