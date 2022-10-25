namespace AdaptiveRoads.Data.NetworkExtensions; 
using System;
using UnityEngine;
using ColossalFramework.Math;
using KianCommons;
using KianCommons.Math;
using KianCommons.UI;
using AdaptiveRoads.Util;

public struct OutlineData {
    public Bezier3 Center, Left, Right;
    public Vector3 DirA, DirD;
    public bool SmoothA, SmoothD;
    public bool Empty => Center.a == Center.d;

    /// <param name="angle">tilt angle in radians</param>
    public OutlineData(Bezier3 bezier, float width, bool smoothA, bool smoothD, float angleA, float angleD, float wireHeight) {
        //Log.Called($"angleA={angleA}", $"angleD={angleD}", $"wire={wire}");
        float hw = 0.5f * width;
        float r = (bezier.a - bezier.d).magnitude *(1f/3);
        SmoothA = smoothA;
        DirA = bezier.DirA()/r;
        SmoothD = smoothD;
        DirD = bezier.DirD()/r;

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
    }

    /// <param name="angle">tilt angle in radians</param>
    public OutlineData(Vector3 a, Vector3 d, Vector3 dirA, Vector3 dirD, float width, bool smoothA, bool smoothD, float angleA, float angleD, float wireHeight) {
        //Log.Called($"angleA={angleA}", $"angleD={angleD}", $"wire={wire}");
        float hw = 0.5f * width;
        SmoothA = smoothA;
        DirA = dirA;
        SmoothD = smoothD;
        DirD = dirD;

        {
            Vector2 shiftA = CalShift(angleA, hw, wireHeight, out float centerShiftA);
            Vector3 displacementA = CalcDisplacement(DirA, shiftA);

            Center.a = ApplyShift(a, dirA, centerShiftA);
            Right.a = Center.a + displacementA;
            Left.a = Center.a - displacementA;
        }

        {
            Vector2 shiftD = CalShift(angleD, hw, wireHeight, out float centerShiftD);
            Vector3 displacementD = CalcDisplacement(-DirD, shiftD);

            Center.d = ApplyShift(d, -DirD, centerShiftD);
            Right.d = Center.d + displacementD;
            Left.d = Center.d - displacementD;
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

        static Vector3 ApplyShift(Vector3 pos, Vector3 dir, float shift) {
            var normal = new Vector3(dir.z, 0, -dir.x).normalized; // rotate left.
            return pos + shift * normal;
        }

        static Vector3 CalcDisplacement(Vector3 dir, Vector2 shift) {
            var normal = new Vector3(dir.z, 0, -dir.x).normalized; // rotate left.
            return shift.x * normal + shift.y * Vector3.up;
        }

        NetSegment.CalculateMiddlePoints(Center.a, DirA, Center.d, DirD, SmoothA, SmoothD, out Center.b, out Center.c);
        NetSegment.CalculateMiddlePoints(Left.a, DirA, Left.d, DirD, SmoothA, SmoothD, out Left.b, out Left.c);
        NetSegment.CalculateMiddlePoints(Right.a, DirA, Right.d, DirD, SmoothA, SmoothD, out Right.b, out Right.c);
    }

    public void CalculateMiddlePoints() {
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
        } catch (Exception ex) { ex.Log(); }
    }
}
