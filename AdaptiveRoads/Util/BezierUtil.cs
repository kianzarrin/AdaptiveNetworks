namespace AdaptiveRoads.Util {
    using ColossalFramework.Math;
    using UnityEngine;

    internal static class GenericBezier3Util {
        public static Vector3 CalcShiftAt(this Bezier3 bezier, float t, float shift, float vshift) {
            var pos = bezier.Position(t);
            var dir = bezier.Tangent(t);
            var shiftPos = CalcShift(pos, dir, shift, vshift);
            return shiftPos;
        }

        public static Vector3 CalcShift(Vector3 pos, Vector3 dir, float shift, float vshift) {
            Vector3 normal = new Vector3(-dir.z, 0, dir.x).normalized;
            pos += shift * normal;
            pos.y += vshift;
            return pos;
        }

        public const float T1 = 1f / 3;
        public const float T2 = 1 - T1;

        public static Bezier3 ShiftRight(this Bezier3 bezier, float shiftA, float shiftD) {
            return ShiftRightImpl(
                bezier,
                new Vector2(shiftA,0),
                new Vector2(shiftD,0)).ToBezier3();
        }

        public static Bezier3 ShiftRight(this Bezier3 bezier, Vector2 shiftA, Vector2 shiftD) {
            return ShiftRightImpl(bezier, shiftA, shiftD).ToBezier3();
        }

        /// <summary>
        /// shift.x is shift in the direction of normal.
        /// shift.y is vertical shift.
        /// TODO: input vertical shift velocity
        public static Bezier3WithPoints ShiftRightImpl(Bezier3 bezier, Vector2 shiftA, Vector2 shiftD) {
            Bezier3WithPoints ret = default;
            ret.a = CalcShift(pos: bezier.a, dir: bezier.b - bezier.a, shift: shiftA.x, vshift: shiftA.y);
            ret.a.y += shiftA.y;

            ret.t1 = T1;
            ret.p1 = bezier.CalcShiftAt(t: ret.t1, shift: shiftA.x, vshift: shiftA.y);

            ret.t2 = T2;
            ret.p2 = bezier.CalcShiftAt(t: ret.t2, shift: shiftD.x, vshift: shiftA.y);

            ret.d = CalcShift(pos: bezier.d, dir: bezier.d - bezier.c, shift: shiftD.x, vshift: shiftA.y);
            return ret;
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
        /// t1 and t2 are offsets for point1 and point2 on bezier
        /// b and c are middle points
        /// </summary>
        public void CalcMiddlePoints(out Vector3 middle1, out Vector3 middle2) {
            CalcCoef(t1, out float a1, out float b1, out float c1, out float d1);
            CalcCoef(t2, out float a2, out float b2, out float c2, out float d2);

            Vector3 u1 = CalcU(a, d, p1, a1, d1);
            Vector3 u2 = CalcU(a, d, p2, a2, d2);

            CalcMiddlePoints(b1, c1, u1, b2, c2, u2, out middle1, out middle2);
        }


        public static Bezier3 CalcPerfict(Vector3 a, Vector3 d, Vector3 point1, Vector3 point2, float t1, float t2) {
            CalcCoef(t1, out float a1, out float b1, out float c1, out float d1);
            CalcCoef(t2, out float a2, out float b2, out float c2, out float d2);

            Vector3 u1 = CalcU(a, d, point1, a1, d1);
            Vector3 u2 = CalcU(a, d, point2, a2, d2);

            CalcMiddlePoints(b1, c1, u1, b2, c2, u2, out Vector3 middle1, out Vector3 middle2);

            var bezier = new Bezier3() {
                a = a,
                b = middle1,
                c = middle2,
                d = d
            };

            return bezier;
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
