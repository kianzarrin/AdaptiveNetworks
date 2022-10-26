namespace AdaptiveRoads.Data.NetworkExtensions;
using AdaptiveRoads.Util;
using ColossalFramework.Math;
using KianCommons;
using KianCommons.Math;
using KianCommons.UI;
using System;
using System.Diagnostics;
using UnityEngine;
using static TimeMilestone;

public struct TiltData {
    public float a, b, c, d; // angles
    public TiltData(float startAngle, float startVelocity, float endAngle, float endVelocity) {
        a = startAngle;
        b = startAngle + startVelocity * (1f/3);
        c = endAngle + endVelocity * (1f/ 3);
        d = endAngle;
    }

    public float this[int index] => index switch {
        0 => a,
        1 => b,
        2 => c,
        3 => d,
        _ => throw new IndexOutOfRangeException(index.ToString()),
    };

    public bool IsAproxZero =>
        MathUtil.EqualAprox(a, 0) &&
        MathUtil.EqualAprox(b, 0) &&
        MathUtil.EqualAprox(c, 0) &&
        MathUtil.EqualAprox(d, 0);

    public override string ToString() => $"a:{a*Mathf.Rad2Deg:f} b:{b * Mathf.Rad2Deg:f} c:{c * Mathf.Rad2Deg:f} d:{d * Mathf.Rad2Deg:f}";

    public static float CalcCenterShift(float angle, float wireHeight) {
        if (wireHeight != 0) {
            return wireHeight * Mathf.Sin(angle);
        } else {
            return 0;
        }
    }

    public static Vector2 CalcSideShift(float angle, float wireHeight) {
        if (wireHeight != 0) {
            return default;
        } else {
            return new Vector2 {
                x = Mathf.Cos(angle),
                y = Mathf.Sin(angle),
            };
        }
    }
}

public struct OutlineData {
    public Bezier3 Center, Left, Right;
    public bool Empty => Center.a == Center.d;

#if DEBUG
    public static Stopwatch timer1 = new Stopwatch();
    public static Stopwatch timer2 = new Stopwatch();
    public static int counter1, counter2;
#endif 

    /// <param name="angle">tilt angle in radians</param>
    public OutlineData(Bezier3 bezier, float width, TiltData tiltData, float wireHeight) {
        try {
#if DEBUG
            timer1.Start();
            counter1++;
#endif
            float hw = 0.5f * width;

            Bezier1 centerShift;
            centerShift.a = TiltData.CalcCenterShift(tiltData.a, wireHeight);
            centerShift.b = TiltData.CalcCenterShift(tiltData.b, wireHeight);
            centerShift.c = TiltData.CalcCenterShift(tiltData.c, wireHeight);
            centerShift.d = TiltData.CalcCenterShift(tiltData.d, wireHeight);

            Bezier2 sideShift;
            sideShift.a = hw * TiltData.CalcSideShift(tiltData.a, wireHeight);
            sideShift.b = hw * TiltData.CalcSideShift(tiltData.b, wireHeight);
            sideShift.c = hw * TiltData.CalcSideShift(tiltData.c, wireHeight);
            sideShift.d = hw * TiltData.CalcSideShift(tiltData.d, wireHeight);

            Center = bezier.ShiftRight(centerShift);
            Right = Center.ShiftRight(sideShift);
            Left = Center.ShiftRight(sideShift.Negative());
        } finally {
#if DEBUG
            timer1.Stop();
#endif
        }
    }

    // transition:
    public OutlineData(Bezier3 bezierA, in Bezier3 bezierD, float width, TiltData tiltData, float wireHeight) {
        try {
#if DEBUG
            timer2.Start();
            counter2++;
#endif
            float hw = width * .5f;
            var dirA = -bezierA.DirA().normalized;
            var dirD = -bezierD.DirA().normalized;

            {
                float centerShift = TiltData.CalcCenterShift(tiltData.a, wireHeight);
                Vector2 sideShift = hw * TiltData.CalcSideShift(tiltData.a, wireHeight);
                Vector3 displacement = CalcDisplacement(dirA, sideShift);

                Center.a = ApplyShift(bezierA.a, dirA, centerShift);
                Right.a = Center.a + displacement;
                Left.a = Center.a - displacement;
            }

            {
                float centerShift = TiltData.CalcCenterShift(tiltData.d, wireHeight);
                Vector2 sideShift = hw * TiltData.CalcSideShift(tiltData.d, wireHeight);
                Vector3 displacement = CalcDisplacement(-dirD, sideShift);

                Center.d = ApplyShift(bezierD.a, -dirD, centerShift);
                Right.d = Center.d + displacement;
                Left.d = Center.d - displacement;
            }

            NetSegment.CalculateMiddlePoints(Center.a, dirA, Center.d, dirD, true, true, out Center.b, out Center.c);
            NetSegment.CalculateMiddlePoints(Left.a, dirA, Left.d, dirD, true, true, out Left.b, out Left.c);
            NetSegment.CalculateMiddlePoints(Right.a, dirA, Right.d, dirD, true, true, out Right.b, out Right.c);
        } finally {
#if DEBUG
            timer2.Stop();
#endif
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
            Vector3 displacement = CalcDisplacement(dirA, hw);
            Center.a = a;
            Right.a = a + displacement;
            Left.a = a - displacement;
        }

        {
            Vector3 displacement = CalcDisplacement(-dirD, hw);
            Center.d = d;
            Right.d = d + displacement;
            Left.d = d - displacement;
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
