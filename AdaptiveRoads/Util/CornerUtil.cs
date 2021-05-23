namespace AdaptiveRoads.Util {
    using UnityEngine;

    public static class CornerUtil {
        /// <summary>
        /// all directions going away fromt he junction
        /// </summary>
        public static void CalculateTransformVectors(Vector3 dir, bool left, out Vector3 outward, out Vector3 forward) {
            Vector3 rightward = Vector3.Cross(Vector3.up, dir).normalized; // going away from the junction
            Vector3 leftward = -rightward;
            forward = new Vector3(dir.x, 0, dir.z).normalized; // going away from the junction
            outward = left ? leftward : rightward;
        }

        /// <summary>
        /// tranforms input vector from relative (to x y z inputs) coordinate to absulute coodinate.
        /// </summary>
        public static Vector3 TransformCoordinates(Vector3 v, Vector3 x, Vector3 y, Vector3 z)
            => v.x * x + v.y * y + v.z * z;

        /// <summary>
        /// reverse transformed coordinates.
        /// </summary>
        public static Vector3 ReverseTransformCoordinats(Vector3 v, Vector3 x, Vector3 y, Vector3 z) {
            Vector3 ret = default;
            ret.x = Vector3.Dot(v, x);
            ret.y = Vector3.Dot(v, y);
            ret.z = Vector3.Dot(v, z);
            return ret;
        }
    }
}
