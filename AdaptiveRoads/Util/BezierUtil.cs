namespace AdaptiveRoads.Util {
    using AdaptiveRoads.Data.NetworkExtensions;
    using ColossalFramework.Math;
    using KianCommons;
    using UnityEngine;
    public struct Bezier1 {
        public float a, b, c, d;
    }

    internal static class BezierExtensions {
        public static Bezier2 Negative(in this Bezier2 bezier) {
            return new Bezier2(
                -bezier.a,
                -bezier.b,
                -bezier.c,
                -bezier.d);
        }
        public static Bezier2 Mul(in this Bezier2 bezier, float f) {
            return new Bezier2(
                bezier.a *f,
                bezier.b * f,
                bezier.c * f,
                bezier.d * f) ;
        }
    }

    internal static class ShiftBezier3Util {
        public static Vector3 CalcShiftRightAt(this Bezier3 bezier, float t, float shift, float vshift=0) {
            var pos = bezier.Position(t);
            var dir = bezier.Tangent(t);
            var shiftPos = CalcShiftRight(pos, dir, shift, vshift);
            return shiftPos;
        }

        public static Vector3 CalcShiftRight(Vector3 pos, Vector3 dir, float shift, float vshift=0) {
            Vector3 normal = new Vector3(dir.z, 0, -dir.x).normalized; // rotate right
            pos += shift * normal;
            pos.y += vshift;
            return pos;
        }

        public const float T1 = 1f/3;
        public const float T2 = 1 - T1;

        public static Bezier3 ShiftRight(this Bezier3 bezier, float shift) {
            return bezier.ShiftRight(new Bezier1 { a = shift, b = shift, c = shift, d = shift });
        }

        public static Bezier3 ShiftRight(this Bezier3 bezier, Bezier1 shift) {
            Bezier3WithPoints res = default;
            res.a = CalcShiftRight(pos: bezier.a, dir: bezier.b - bezier.a, shift: shift.a);

            res.t1 = T1;
            res.p1 = bezier.CalcShiftRightAt(t: res.t1, shift: shift.b);

            res.t2 = T2;
            res.p2 = bezier.CalcShiftRightAt(t: res.t2, shift: shift.c);

            res.d = CalcShiftRight(pos: bezier.d, dir: bezier.d - bezier.c, shift: shift.d);
            return res.ToBezier3();
        }

        public static Bezier3 ShiftRight(this Bezier3 bezier, Bezier2 shift) {
            Bezier3WithPoints res = default;
            res.a = CalcShiftRight(pos: bezier.a, dir: bezier.b - bezier.a, shift: shift.a.x, vshift: shift.a.y);

            res.t1 = T1;
            res.p1 = bezier.CalcShiftRightAt(t: res.t1, shift: shift.b.x, vshift: shift.b.y);

            res.t2 = T2;
            res.p2 = bezier.CalcShiftRightAt(t: res.t2, shift: shift.c.x, vshift: shift.c.y);

            res.d = CalcShiftRight(pos: bezier.d, dir: bezier.d - bezier.c, shift: shift.d.x, vshift: shift.d.y);
            return res.ToBezier3();
        }
    }

    /// <summary>
    /// bezier representation with points p1/p2 on bezier at offsets t1/t2
    /// </summary>
    public struct Bezier3WithPoints {
        public Vector3 a, p1, p2, d;
        public float t1, t2;

        /// <summary>
        /// convert representation bezier to bezier with control points.
        /// </summary>
        public Bezier3 ToBezier3() {
            CalcMiddlePoints(out Vector3 middle1, out Vector3 middle2);
            return new Bezier3(a, middle1, middle2, d);
        }

        /// <summary>
        /// bezier curve fitting for to the given points.
        /// t1 and t2 are offsets for point1 and point2 on bezier
        /// </summary>
        public void CalcMiddlePoints(out Vector3 middle1, out Vector3 middle2) {
            CalcCoef(t1, out float a1, out float b1, out float c1, out float d1);
            CalcCoef(t2, out float a2, out float b2, out float c2, out float d2);

            Vector3 u1 = CalcU(a, d, p1, a1, d1);
            Vector3 u2 = CalcU(a, d, p2, a2, d2);

            CalcMiddlePoints(b1, c1, u1, b2, c2, u2, out middle1, out middle2);
        }

        public static void CalcCoef(float t, out float a, out float b, out float c, out float d) {
            var mt = 1 - t;
            a = mt * mt * mt;
            b = 3 * t * mt * mt;
            c = 3 * t * t * mt;
            d = t * t * t;
        }

        public static float CalcU(float start, float end, float point, float a, float d) => point - (a * start) - (d * end);

        public static void CalcMiddlePoints(float b1, float c1, float u1, float b2, float c2, float u2, out float m1, out float m2) {
            m2 = (u2 - (b2 / b1 * u1)) / (c2 - (b2 / b1 * c1));
            m1 = (u1 - c1 * m2) / b1;
        }

        public static Vector3 CalcU(Vector3 start, Vector3 end, Vector3 point, float a, float d) {
            return new Vector3(
                CalcU(start.x, end.x, point.x, a, d),
                CalcU(start.y, end.y, point.y, a, d),
                CalcU(start.z, end.z, point.z, a, d));
        }
        public static void CalcMiddlePoints(
            float b1, float c1, Vector3 u1,
            float b2, float c2, Vector3 u2,
            out Vector3 middle1, out Vector3 middle2) {
            CalcMiddlePoints(b1, c1, u1.x, b2, c2, u2.x, out middle1.x, out middle2.x);
            CalcMiddlePoints(b1, c1, u1.y, b2, c2, u2.y, out middle1.y, out middle2.y);
            CalcMiddlePoints(b1, c1, u1.z, b2, c2, u2.z, out middle1.z, out middle2.z);
        }
    }
}
