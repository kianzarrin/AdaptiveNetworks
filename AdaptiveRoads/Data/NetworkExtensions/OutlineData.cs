namespace AdaptiveRoads.Data.NetworkExtensions;
using AdaptiveRoads.Util;
using ColossalFramework.Math;
using KianCommons;
using KianCommons.Math;
using KianCommons.UI;
using System;
using System.Diagnostics;
using UnityEngine;

public struct OutlineData {
    public Bezier3 Center, Left, Right;
    public bool Empty => Center.a == Center.d;

#if DEBUG
    public static Stopwatch timer1 = new Stopwatch();
    public static Stopwatch timer2 = new Stopwatch();
#endif 

    /// <param name="angle">tilt angle in radians</param>
    public OutlineData(Bezier3 bezier, float width, bool smoothA, bool smoothD, float angleA, float angleD, float wireHeight) {
        try {
#if DEBUG
            timer1.Start();
#endif
            float hw = 0.5f * width;

            Vector2 shiftA = CalShift(angleA, hw, wireHeight, out float centerShiftA);
            Vector2 shiftD = CalShift(angleD, hw, wireHeight, out float centerShiftD);
            Center = bezier.ShiftRight(centerShiftA, centerShiftD);
            Right = Center.ShiftRight(shiftA, shiftD);
            Left = Center.ShiftRight(-shiftA, -shiftD);

            static Vector2 CalShift(float angle, float hw, float wireHeight, out float centerShift) {
                Vector2 shift = default;
                if (wireHeight != 0) {
                    // no need to tilt wires. move them sideways to avoid clipping into tilted train
                    centerShift = wireHeight * Mathf.Sin(angle);
                    shift.x = hw;
                } else {
                    centerShift = 0;
                    shift.x = hw * Mathf.Cos(angle);
                    shift.y = hw * Mathf.Sin(angle);
                }
                return shift;
            }
        } finally {
#if DEBUG
            timer1.Stop();
#endif
        }
    }

    // transition:
    public OutlineData(Bezier3 bezierA, Bezier3 bezierD, float width, float angleA, float angleD, float wireHeight) {
        try {
#if DEBUG
            timer2.Start();
#endif
            float hw = width * .5f;
            var dirA = -bezierA.DirA().normalized;
            var dirD = -bezierD.DirA().normalized;

            {
                Vector2 shiftA = CalShift(angleA, hw, wireHeight, out float centerShiftA);
                Vector3 displacementA = CalcDisplacement(dirA, shiftA);

                Center.a = ApplyShift(bezierA.a, dirA, centerShiftA);
                Right.a = Center.a + displacementA;
                Left.a = Center.a - displacementA;
            }

            {
                Vector2 shiftD = CalShift(angleD, hw, wireHeight, out float centerShiftD);
                Vector3 displacementD = CalcDisplacement(-dirD, shiftD);

                Center.d = ApplyShift(bezierD.a, -dirD, centerShiftD);
                Right.d = Center.d + displacementD;
                Left.d = Center.d - displacementD;
            }
            NetSegment.CalculateMiddlePoints(Center.a, dirA, Center.d, dirD, true, true, out Center.b, out Center.c);
            NetSegment.CalculateMiddlePoints(Left.a, dirA, Left.d, dirD, true, true, out Left.b, out Left.c);
            NetSegment.CalculateMiddlePoints(Right.a, dirA, Right.d, dirD, true, true, out Right.b, out Right.c);
        } finally {
#if DEBUG
            timer2.Stop();
#endif
        }

        static Vector2 CalShift(float angle, float hw, float wireHeight, out float centerShift) {
            Vector2 shift = default;
            if (wireHeight != 0) {
                // no need to tilt wires. move them sideways to avoid clipping into tilted train
                centerShift = wireHeight * Mathf.Sin(angle);
                shift.x = hw;
            } else {
                centerShift = 0;
                shift.x = hw * Mathf.Cos(angle);
                shift.y = hw * Mathf.Sin(angle);
            }
            return shift;
        }

        static Vector3 ApplyShift(Vector3 pos, Vector3 tan, float shift) {
            var normal = new Vector3(tan.z, 0, -tan.x).normalized; // rotate left.
            return pos + shift * normal;
        }

        static Vector3 CalcDisplacement(Vector3 tan, Vector2 shift) {
            var normal = new Vector3(tan.z, 0, -tan.x).normalized; // rotate left.
            return shift.x * normal + shift.y * Vector3.up;
        }
    }

    // on the fly
    public OutlineData(Vector3 a, Vector3 d, Vector3 dirA, Vector3 dirD, float width, bool smoothA, bool smoothD) {
        float hw = 0.5f * width;
        {
            Vector3 displacementA = CalcDisplacement(dirA, hw);
            Center.a = a;
            Right.a = a + displacementA;
            Left.a = a - displacementA;
        }

        {
            Vector3 displacementD = CalcDisplacement(-dirD, hw);
            Center.d = d;
            Right.d = d + displacementD;
            Left.d = d - displacementD;
        }

        static Vector3 CalcDisplacement(Vector3 tan, float shift) {
            var normal = new Vector3(tan.z, 0, -tan.x).normalized; // rotate left.
            return shift * normal;
        }

        NetSegment.CalculateMiddlePoints(Center.a, dirA, Center.d, dirD, smoothA, smoothD, out Center.b, out Center.c);
        NetSegment.CalculateMiddlePoints(Left.a, dirA, Left.d, dirD, smoothA, smoothD, out Left.b, out Left.c);
        NetSegment.CalculateMiddlePoints(Right.a, dirA, Right.d, dirD, smoothA, smoothD, out Right.b, out Right.c);
    }

    public void RenderTestOverlay(RenderManager.CameraInfo cameraInfo) {
        try {
            bool alphaBlend = false;

            RenderUtil.DrawOverlayCircle(cameraInfo, Color.green, Right.a, 1, alphaBlend);
            RenderUtil.DrawOverlayCircle(cameraInfo, Color.yellow, Left.a, 1, alphaBlend);
            RenderUtil.DrawOverlayCircle(cameraInfo, Color.green / 2, Right.d, 1, alphaBlend);
            RenderUtil.DrawOverlayCircle(cameraInfo, Color.yellow / 2, Left.d, 1, alphaBlend);

            Vector3 dirA = Center.DirA();
            Vector3 dirD = Center.DirD();
            var startArrow = new Bezier3(Center.a, Center.a + dirA, Center.a + 2 * dirA, Center.a + 3 * dirA);
            var endArrow = new Bezier3(Center.d, Center.d + dirD, Center.d + 2 * dirD, Center.d + 3 * dirD);
            startArrow.RenderArrow(cameraInfo, Color.blue, 1, alphaBlend);
            endArrow.RenderArrow(cameraInfo, Color.blue, 1, alphaBlend);
        } catch (Exception ex) { ex.Log(); }
    }
}
