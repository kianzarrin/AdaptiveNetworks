namespace AdaptiveRoads.Data.NetworkExtensions {
    using ColossalFramework.Math;
    using UnityEngine;
    using KianCommons.Math;
    using KianCommons.UI;
    using KianCommons;
    using System;

    public struct OutlineData {
        public Bezier3 Center, Left, Right;
        public Vector3 DirA, DirD;
        public bool SmoothA, SmoothD;
        public const float WIRE_HEIGHT = 4.9f;

        public bool Empty => Center.a == Center.d;

        // TODO: should I just raise the lane instead of accepting deltaY
        /// <param name="angle">tilt angle in radians</param>
        public OutlineData(Vector3 a, Vector3 d, Vector3 dirA, Vector3 dirD, float width, bool smoothA, bool smoothD, float angleA, float angleD, bool wire) {
            //Log.Called($"angleA={angleA}", $"angleD={angleD}", $"wire={wire}");
            float hw = 0.5f * width;

            {
                var normal = new Vector3(dirA.z, 0, -dirA.x); // rotate right.
                normal = VectorUtils.NormalizeXZ(normal);

                if(wire) {
                    // move wires sideways to avoid clipping into tilted train
                    a -= normal * (WIRE_HEIGHT * Mathf.Sin(angleA));
                }

                SmoothA = smoothA;
                Center.a = a;
                DirA = dirA;

                Vector3 displacement;
                if(wire) {
                    displacement = normal * hw;
                } else {
                    displacement = normal * (hw * Mathf.Cos(angleA));
                    displacement.y = hw * Mathf.Sin(angleA);
                }
                Right.a = a + displacement;
                Left.a = a - displacement;
            }

            {
                var normal = new Vector3(dirD.z, 0, -dirD.x); // rotate right.
                normal = -VectorUtils.NormalizeXZ(normal); // end dir needs minus

                if(wire) {
                    // move wires sideways to avoid clipping into tilted train
                    d -= normal * (WIRE_HEIGHT * Mathf.Sin(angleD));
                }

                SmoothD = smoothD;
                Center.d = d;
                DirD = dirD;

                Vector3 displacement;
                if(wire) {
                    displacement = normal * hw;
                } else {
                    displacement = normal * (hw * Mathf.Cos(angleD));
                    displacement.y = hw * Mathf.Sin(angleD);
                }
                Right.d = d + displacement;
                Left.d = d - displacement;
            }

            NetSegment.CalculateMiddlePoints(Center.a, DirA, Center.d, DirD, SmoothA, SmoothD, out Center.b, out Center.c);
            NetSegment.CalculateMiddlePoints(Left.a, DirA, Left.d, DirD, SmoothA, SmoothD, out Left.b, out Left.c);
            NetSegment.CalculateMiddlePoints(Right.a, DirA, Right.d, DirD, SmoothA, SmoothD, out Right.b, out Right.c);
        }

        public void RenderTestOverlay(RenderManager.CameraInfo cameraInfo) {
            try {
                bool alphaBlend = false;

                RenderUtil.DrawOverlayCircle(cameraInfo, Color.green, Right.a, 1, alphaBlend);
                RenderUtil.DrawOverlayCircle(cameraInfo, Color.yellow, Left.a, 1, alphaBlend);
                RenderUtil.DrawOverlayCircle(cameraInfo, Color.green / 2, Right.d, 1, alphaBlend);
                RenderUtil.DrawOverlayCircle(cameraInfo, Color.yellow / 2, Left.d, 1, alphaBlend);

                var startArrow = new Bezier3(Center.a, Center.a + DirA, Center.a + 2 * DirA, Center.a + 3 * DirA);
                var endArrow = new Bezier3(Center.d, Center.d + DirD, Center.d + 2 * DirD, Center.d + 3 * DirD);
                startArrow.RenderArrow(cameraInfo, Color.blue, 1, alphaBlend);
                endArrow.RenderArrow(cameraInfo, Color.blue, 1, alphaBlend);
            } catch(Exception ex) { ex.Log(); }
        }
    }
}
