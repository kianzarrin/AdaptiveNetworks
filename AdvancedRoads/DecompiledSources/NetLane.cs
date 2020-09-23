using ColossalFramework;
using UnityEngine;
public partial class NetLane2 {
    float m_length;

    public void RenderInstance(RenderManager.CameraInfo cameraInfo, ushort segmentID, uint laneID, NetInfo.Lane laneInfo, NetNode.Flags startFlags, NetNode.Flags endFlags, Color startColor, Color endColor, float startAngle, float endAngle, bool segmentInverted, int layerMask, Vector4 objectIndex1, Vector4 objectIndex2, ref RenderManager.Instance data, ref int propIndex) {
        NetLaneProps laneProps = laneInfo.m_laneProps;
        bool backward = (byte)(laneInfo.m_finalDirection & NetInfo.Direction.Both) == 2 || (byte)(laneInfo.m_finalDirection & NetInfo.Direction.AvoidBoth) == 11;
        bool inverted = backward != segmentInverted;
        if (backward) {
            NetNode.Flags flags = startFlags;
            startFlags = endFlags;
            endFlags = flags;
            NetLaneProps.Prop prop = laneProps.m_props[0];
            float num4 = prop.m_segmentOffset * 0.5f;
            num4 = Mathf.Clamp(num4 + prop.m_position.z / this.m_length, -0.5f, 0.5f);
            if (inverted) num4 = -num4;
        }
    }
}
