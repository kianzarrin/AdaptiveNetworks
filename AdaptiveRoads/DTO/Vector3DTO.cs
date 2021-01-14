namespace AdaptiveRoads.DTO {
    public class Vector3DTO {
        public float x, y, z;

        public static explicit operator Vector3DTO(UnityEngine.Vector3 v) =>
            new Vector3DTO { x = v.x, y = v.y, z = v.z };

        public static explicit operator UnityEngine.Vector3(Vector3DTO v) =>
            new UnityEngine.Vector3(v.x, v.y, v.z);
    }
}
