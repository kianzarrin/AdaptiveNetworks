// Token: 0x02000467 RID: 1127
using AdvancedRoads.Util;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

public partial struct NetNode2
{

    public void PopulateGroupData(ushort nodeID, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps)
    {
        NetInfo info = this.Info;
        if (this.m_problems != Notification.Problem.None && layer == Singleton<NotificationManager>.instance.m_notificationLayer && (this.m_flags & NetNode.Flags.Temporary) == NetNode.Flags.None)
        {
            Vector3 position = this.m_position;
            position.y += info.m_maxHeight;
            Notification.PopulateGroupData(this.m_problems, position, 1f, groupX, groupZ, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
        }
        bool flag = false;
        if ((this.m_flags & NetNode.Flags.Junction) != NetNode.Flags.None)
        {
            NetManager instance = Singleton<NetManager>.instance;
            Vector3 a = this.m_position;
            for (int i1 = 0; i1 < 8; i1++)
            {
                ushort segmentID1 = this.GetSegment(i1);
                if (segmentID1 != 0)
                {
                    NetInfo info1 = instance.m_segments.m_buffer[(int)segmentID1].Info;
                    ItemClass connectionClass = info1.GetConnectionClass();
                    Vector3 a2 = (nodeID != instance.m_segments.m_buffer[(int)segmentID1].m_startNode) ? instance.m_segments.m_buffer[(int)segmentID1].m_endDirection : instance.m_segments.m_buffer[(int)segmentID1].m_startDirection;
                    float num = -1f;
                    for (int i2 = 0; i2 < 8; i2++)
                    {
                        ushort segmentID2 = this.GetSegment(i2);
                        if (segmentID2 != 0 && segmentID2 != segmentID1)
                        {
                            NetInfo info2 = instance.m_segments.m_buffer[(int)segmentID2].Info;
                            ItemClass connectionClass2 = info2.GetConnectionClass();
                            if (((info.m_netLayers | info1.m_netLayers | info2.m_netLayers) & 1 << layer) != 0 && (connectionClass.m_service == connectionClass2.m_service || (info1.m_nodeConnectGroups & info2.m_connectGroup) != NetInfo.ConnectGroup.None || (info2.m_nodeConnectGroups & info1.m_connectGroup) != NetInfo.ConnectGroup.None))
                            {
                                Vector3 vector = (nodeID != instance.m_segments.m_buffer[(int)segmentID2].m_startNode) ? instance.m_segments.m_buffer[(int)segmentID2].m_endDirection : instance.m_segments.m_buffer[(int)segmentID2].m_startDirection;
                                float num2 = a2.x * vector.x + a2.z * vector.z;
                                num = Mathf.Max(num, num2);
                                bool flag2 = info1.m_requireDirectRenderers && (info1.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (info1.m_nodeConnectGroups & info2.m_connectGroup) != NetInfo.ConnectGroup.None);
                                bool flag3 = info2.m_requireDirectRenderers && (info2.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (info2.m_nodeConnectGroups & info1.m_connectGroup) != NetInfo.ConnectGroup.None);
                                if (i2 > i1 && (flag2 || flag3))
                                {
                                    float num3 = 0.01f - Mathf.Min(info1.m_maxTurnAngleCos, info2.m_maxTurnAngleCos);
                                    if (num2 < num3)
                                    {
                                        float num4;
                                        if (flag2)
                                        {
                                            num4 = info1.m_netAI.GetNodeInfoPriority(segmentID1, ref instance.m_segments.m_buffer[(int)segmentID1]);
                                        }
                                        else
                                        {
                                            num4 = -1E+08f;
                                        }
                                        float num5;
                                        if (flag3)
                                        {
                                            num5 = info2.m_netAI.GetNodeInfoPriority(segmentID2, ref instance.m_segments.m_buffer[(int)segmentID2]);
                                        }
                                        else
                                        {
                                            num5 = -1E+08f;
                                        }
                                        if (num4 >= num5)
                                        {
                                            if (info1.m_nodes != null && info1.m_nodes.Length != 0)
                                            {
                                                flag = true;
                                                float vscale = info1.m_netAI.GetVScale();
                                                Vector3 zero = Vector3.zero;
                                                Vector3 zero2 = Vector3.zero;
                                                Vector3 vector2 = Vector3.zero;
                                                Vector3 vector3 = Vector3.zero;
                                                Vector3 zero3 = Vector3.zero;
                                                Vector3 zero4 = Vector3.zero;
                                                Vector3 zero5 = Vector3.zero;
                                                Vector3 zero6 = Vector3.zero;
                                                bool start = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID1].m_startNode == nodeID;
                                                bool flag4;
                                                Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID1].CalculateCorner(segmentID1, true, start, false, out zero, out zero3, out flag4);
                                                Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID1].CalculateCorner(segmentID1, true, start, true, out zero2, out zero4, out flag4);
                                                start = (Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID2].m_startNode == nodeID);
                                                Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID2].CalculateCorner(segmentID2, true, start, true, out vector2, out zero5, out flag4);
                                                Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID2].CalculateCorner(segmentID2, true, start, false, out vector3, out zero6, out flag4);
                                                Vector3 b = (vector3 - vector2) * (info1.m_halfWidth / info2.m_halfWidth * 0.5f - 0.5f);
                                                vector2 -= b;
                                                vector3 += b;
                                                Vector3 vector4;
                                                Vector3 vector5;
                                                NetSegment.CalculateMiddlePoints(zero, -zero3, vector2, -zero5, true, true, out vector4, out vector5);
                                                Vector3 vector6;
                                                Vector3 vector7;
                                                NetSegment.CalculateMiddlePoints(zero2, -zero4, vector3, -zero6, true, true, out vector6, out vector7);
                                                Matrix4x4 leftMatrix = NetSegment.CalculateControlMatrix(zero, vector4, vector5, vector2, zero2, vector6, vector7, vector3, groupPosition, vscale);
                                                Matrix4x4 rightMatrix = NetSegment.CalculateControlMatrix(zero2, vector6, vector7, vector3, zero, vector4, vector5, vector2, groupPosition, vscale);
                                                Vector4 vector8 = new Vector4(0.5f / info1.m_halfWidth, 1f / info1.m_segmentLength, 1f, 1f);
                                                Vector4 colorLocation;
                                                Vector4 vector9;
                                                if (NetNode.BlendJunction(nodeID))
                                                {
                                                    colorLocation = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                                                    vector9 = colorLocation;
                                                }
                                                else
                                                {
                                                    colorLocation = RenderManager.GetColorLocation((uint)(49152 + segmentID1));
                                                    vector9 = RenderManager.GetColorLocation((uint)(49152 + segmentID2));
                                                }
                                                Vector4 vector10 = new Vector4(colorLocation.x, colorLocation.y, vector9.x, vector9.y);
                                                for (int k = 0; k < info1.m_nodes.Length; k++)
                                                {
                                                    NetInfo.Node node1 = info1.m_nodes[k];
                                                    if ((node1.m_connectGroup == NetInfo.ConnectGroup.None || (node1.m_connectGroup & info2.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None) &&
                                                        node1.m_layer == layer && node1.CheckFlags(this.m_flags) && node1.m_combinedLod != null && node1.m_directConnect)
                                                    {
                                                        Vector4 objectIndex = vector10;
                                                        Vector4 meshScale = vector8;
                                                        if (node1.m_requireWindSpeed)
                                                        {
                                                            objectIndex.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                                                        }
                                                        if ((node1.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None)
                                                        {
                                                            bool flag5 = instance.m_segments.m_buffer[(int)segmentID1].m_startNode == nodeID == ((instance.m_segments.m_buffer[(int)segmentID1].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                            if (info2.m_hasBackwardVehicleLanes != info2.m_hasForwardVehicleLanes || (node1.m_connectGroup & NetInfo.ConnectGroup.Directional) != NetInfo.ConnectGroup.None)
                                                            {
                                                                bool flag6 = instance.m_segments.m_buffer[(int)segmentID2].m_startNode == nodeID == ((instance.m_segments.m_buffer[(int)segmentID2].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                                if (flag5 == flag6)
                                                                {
                                                                    goto IL_7A7;
                                                                }
                                                            }
                                                            if (flag5)
                                                            {
                                                                if ((node1.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None)
                                                                {
                                                                    goto IL_7A7;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if ((node1.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None)
                                                                {
                                                                    goto IL_7A7;
                                                                }
                                                                meshScale.x = -meshScale.x;
                                                                meshScale.y = -meshScale.y;
                                                            }
                                                        }
                                                        NetNode.PopulateGroupData(info1, node1, leftMatrix, rightMatrix, meshScale, objectIndex, ref vertexIndex, ref triangleIndex, data, ref requireSurfaceMaps);
                                                    }
                                                IL_7A7:;
                                                }
                                            }
                                        }
                                        else if (info2.m_nodes != null && info2.m_nodes.Length != 0)
                                        {
                                            flag = true;
                                            float vscale2 = info2.m_netAI.GetVScale();
                                            Vector3 vector11 = Vector3.zero;
                                            Vector3 vector12 = Vector3.zero;
                                            Vector3 zero7 = Vector3.zero;
                                            Vector3 zero8 = Vector3.zero;
                                            Vector3 zero9 = Vector3.zero;
                                            Vector3 zero10 = Vector3.zero;
                                            Vector3 zero11 = Vector3.zero;
                                            Vector3 zero12 = Vector3.zero;
                                            bool start2 = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID1].m_startNode == nodeID;
                                            bool flag7;
                                            Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID1].CalculateCorner(segmentID1, true, start2, false, out vector11, out zero9, out flag7);
                                            Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID1].CalculateCorner(segmentID1, true, start2, true, out vector12, out zero10, out flag7);
                                            start2 = (Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID2].m_startNode == nodeID);
                                            Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID2].CalculateCorner(segmentID2, true, start2, true, out zero7, out zero11, out flag7);
                                            Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID2].CalculateCorner(segmentID2, true, start2, false, out zero8, out zero12, out flag7);
                                            Vector3 b2 = (vector12 - vector11) * (info2.m_halfWidth / info1.m_halfWidth * 0.5f - 0.5f);
                                            vector11 -= b2;
                                            vector12 += b2;
                                            Vector3 vector13;
                                            Vector3 vector14;
                                            NetSegment.CalculateMiddlePoints(vector11, -zero9, zero7, -zero11, true, true, out vector13, out vector14);
                                            Vector3 vector15;
                                            Vector3 vector16;
                                            NetSegment.CalculateMiddlePoints(vector12, -zero10, zero8, -zero12, true, true, out vector15, out vector16);
                                            Matrix4x4 leftMatrix2 = NetSegment.CalculateControlMatrix(vector11, vector13, vector14, zero7, vector12, vector15, vector16, zero8, groupPosition, vscale2);
                                            Matrix4x4 rightMatrix2 = NetSegment.CalculateControlMatrix(vector12, vector15, vector16, zero8, vector11, vector13, vector14, zero7, groupPosition, vscale2);
                                            Vector4 vector17 = new Vector4(0.5f / info2.m_halfWidth, 1f / info2.m_segmentLength, 1f, 1f);
                                            Vector4 colorLocation2;
                                            Vector4 vector18;
                                            if (NetNode.BlendJunction(nodeID))
                                            {
                                                colorLocation2 = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                                                vector18 = colorLocation2;
                                            }
                                            else
                                            {
                                                colorLocation2 = RenderManager.GetColorLocation((uint)(49152 + segmentID1));
                                                vector18 = RenderManager.GetColorLocation((uint)(49152 + segmentID2));
                                            }
                                            Vector4 vector19 = new Vector4(colorLocation2.x, colorLocation2.y, vector18.x, vector18.y);
                                            for (int l = 0; l < info2.m_nodes.Length; l++)
                                            {
                                                NetInfo.Node node2 = info2.m_nodes[l];
                                                if ((node2.m_connectGroup == NetInfo.ConnectGroup.None || (node2.m_connectGroup & info1.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None) &&
                                                    node2.m_layer == layer && node2.CheckFlags(this.m_flags) && node2.m_combinedLod != null && node2.m_directConnect)
                                                {
                                                    Vector4 objectIndex2 = vector19;
                                                    Vector4 meshScale2 = vector17;
                                                    if (node2.m_requireWindSpeed)
                                                    {
                                                        objectIndex2.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                                                    }
                                                    if ((node2.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None)
                                                    {
                                                        bool flag8 = instance.m_segments.m_buffer[(int)segmentID2].m_startNode == nodeID == ((instance.m_segments.m_buffer[(int)segmentID2].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                        if (info1.m_hasBackwardVehicleLanes != info1.m_hasForwardVehicleLanes || (node2.m_connectGroup & NetInfo.ConnectGroup.Directional) != NetInfo.ConnectGroup.None)
                                                        {
                                                            bool flag9 = instance.m_segments.m_buffer[(int)segmentID1].m_startNode == nodeID == ((instance.m_segments.m_buffer[(int)segmentID1].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                            if (flag9 == flag8)
                                                            {
                                                                goto IL_C08;
                                                            }
                                                        }
                                                        if (flag8)
                                                        {
                                                            if ((node2.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None)
                                                            {
                                                                goto IL_C08;
                                                            }
                                                            meshScale2.x = -meshScale2.x;
                                                            meshScale2.y = -meshScale2.y;
                                                        }
                                                        else if ((node2.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None)
                                                        {
                                                            goto IL_C08;
                                                        }
                                                    }
                                                    NetNode.PopulateGroupData(info2, node2, leftMatrix2, rightMatrix2, meshScale2, objectIndex2, ref vertexIndex, ref triangleIndex, data, ref requireSurfaceMaps);
                                                }
                                            IL_C08:;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    a += a2 * (2f + num * 2f);
                }
            }
            a.y = this.m_position.y + (float)this.m_heightOffset * 0.015625f;
            if ((info.m_netLayers & 1 << layer) != 0 && info.m_requireSegmentRenderers)
            {
                for (int m = 0; m < 8; m++)
                {
                    ushort segment3 = this.GetSegment(m);
                    if (segment3 != 0)
                    {
                        NetInfo info3 = instance.m_segments.m_buffer[(int)segment3].Info;
                        if (info3.m_nodes != null && info3.m_nodes.Length != 0)
                        {
                            flag = true;
                            float vscale3 = info3.m_netAI.GetVScale();
                            Vector3 zero13 = Vector3.zero;
                            Vector3 zero14 = Vector3.zero;
                            Vector3 zero15 = Vector3.zero;
                            Vector3 zero16 = Vector3.zero;
                            Vector3 vector20 = Vector3.zero;
                            Vector3 vector21 = Vector3.zero;
                            Vector3 a3 = Vector3.zero;
                            Vector3 a4 = Vector3.zero;
                            Vector3 zero17 = Vector3.zero;
                            Vector3 zero18 = Vector3.zero;
                            Vector3 zero19 = Vector3.zero;
                            Vector3 zero20 = Vector3.zero;
                            NetSegment netSegment = instance.m_segments.m_buffer[(int)segment3];
                            ItemClass connectionClass3 = info3.GetConnectionClass();
                            Vector3 vector22 = (nodeID != netSegment.m_startNode) ? netSegment.m_endDirection : netSegment.m_startDirection;
                            float num6 = -4f;
                            float num7 = -4f;
                            ushort num8 = 0;
                            ushort num9 = 0;
                            for (int n = 0; n < 8; n++)
                            {
                                ushort segment4 = this.GetSegment(n);
                                if (segment4 != 0 && segment4 != segment3)
                                {
                                    NetInfo info4 = instance.m_segments.m_buffer[(int)segment4].Info;
                                    ItemClass connectionClass4 = info4.GetConnectionClass();
                                    if (connectionClass3.m_service == connectionClass4.m_service)
                                    {
                                        NetSegment netSegment2 = instance.m_segments.m_buffer[(int)segment4];
                                        Vector3 vector23 = (nodeID != netSegment2.m_startNode) ? netSegment2.m_endDirection : netSegment2.m_startDirection;
                                        float num10 = vector22.x * vector23.x + vector22.z * vector23.z;
                                        if (vector23.z * vector22.x - vector23.x * vector22.z < 0f)
                                        {
                                            if (num10 > num6)
                                            {
                                                num6 = num10;
                                                num8 = segment4;
                                            }
                                            num10 = -2f - num10;
                                            if (num10 > num7)
                                            {
                                                num7 = num10;
                                                num9 = segment4;
                                            }
                                        }
                                        else
                                        {
                                            if (num10 > num7)
                                            {
                                                num7 = num10;
                                                num9 = segment4;
                                            }
                                            num10 = -2f - num10;
                                            if (num10 > num6)
                                            {
                                                num6 = num10;
                                                num8 = segment4;
                                            }
                                        }
                                    }
                                }
                            }
                            bool start3 = netSegment.m_startNode == nodeID;
                            bool flag10;
                            netSegment.CalculateCorner(segment3, true, start3, false, out zero13, out zero15, out flag10);
                            netSegment.CalculateCorner(segment3, true, start3, true, out zero14, out zero16, out flag10);
                            Matrix4x4 leftMatrix3;
                            Matrix4x4 rightMatrix3;
                            Matrix4x4 leftMatrixB;
                            Matrix4x4 rightMatrixB;
                            Vector4 meshScale3;
                            Vector4 centerPos;
                            Vector4 sideScale;
                            if (num8 != 0 && num9 != 0)
                            {
                                float num11 = info3.m_pavementWidth / info3.m_halfWidth * 0.5f;
                                float y = 1f;
                                if (num8 != 0)
                                {
                                    NetSegment netSegment3 = instance.m_segments.m_buffer[(int)num8];
                                    NetInfo info6 = netSegment3.Info;
                                    start3 = (netSegment3.m_startNode == nodeID);
                                    netSegment3.CalculateCorner(num8, true, start3, true, out vector20, out a3, out flag10);
                                    netSegment3.CalculateCorner(num8, true, start3, false, out vector21, out a4, out flag10);
                                    float num12 = info6.m_pavementWidth / info6.m_halfWidth * 0.5f;
                                    num11 = (num11 + num12) * 0.5f;
                                    y = 2f * info3.m_halfWidth / (info3.m_halfWidth + info6.m_halfWidth);
                                }
                                float num13 = info3.m_pavementWidth / info3.m_halfWidth * 0.5f;
                                float w = 1f;
                                if (num9 != 0)
                                {
                                    NetSegment netSegment4 = instance.m_segments.m_buffer[(int)num9];
                                    NetInfo info7 = netSegment4.Info;
                                    start3 = (netSegment4.m_startNode == nodeID);
                                    netSegment4.CalculateCorner(num9, true, start3, true, out zero17, out zero19, out flag10);
                                    netSegment4.CalculateCorner(num9, true, start3, false, out zero18, out zero20, out flag10);
                                    float num14 = info7.m_pavementWidth / info7.m_halfWidth * 0.5f;
                                    num13 = (num13 + num14) * 0.5f;
                                    w = 2f * info3.m_halfWidth / (info3.m_halfWidth + info7.m_halfWidth);
                                }
                                Vector3 vector24;
                                Vector3 vector25;
                                NetSegment.CalculateMiddlePoints(zero13, -zero15, vector20, -a3, true, true, out vector24, out vector25);
                                Vector3 vector26;
                                Vector3 vector27;
                                NetSegment.CalculateMiddlePoints(zero14, -zero16, vector21, -a4, true, true, out vector26, out vector27);
                                Vector3 vector28;
                                Vector3 vector29;
                                NetSegment.CalculateMiddlePoints(zero13, -zero15, zero17, -zero19, true, true, out vector28, out vector29);
                                Vector3 vector30;
                                Vector3 vector31;
                                NetSegment.CalculateMiddlePoints(zero14, -zero16, zero18, -zero20, true, true, out vector30, out vector31);
                                leftMatrix3 = NetSegment.CalculateControlMatrix(zero13, vector24, vector25, vector20, zero13, vector24, vector25, vector20, groupPosition, vscale3);
                                rightMatrix3 = NetSegment.CalculateControlMatrix(zero14, vector26, vector27, vector21, zero14, vector26, vector27, vector21, groupPosition, vscale3);
                                leftMatrixB = NetSegment.CalculateControlMatrix(zero13, vector28, vector29, zero17, zero13, vector28, vector29, zero17, groupPosition, vscale3);
                                rightMatrixB = NetSegment.CalculateControlMatrix(zero14, vector30, vector31, zero18, zero14, vector30, vector31, zero18, groupPosition, vscale3);
                                meshScale3 = new Vector4(0.5f / info3.m_halfWidth, 1f / info3.m_segmentLength, 0.5f - info3.m_pavementWidth / info3.m_halfWidth * 0.5f, info3.m_pavementWidth / info3.m_halfWidth * 0.5f);
                                centerPos = a - groupPosition;
                                centerPos.w = (leftMatrix3.m33 + rightMatrix3.m33 + leftMatrixB.m33 + rightMatrixB.m33) * 0.25f;
                                sideScale = new Vector4(num11, y, num13, w);
                            }
                            else
                            {
                                a.x = (zero13.x + zero14.x) * 0.5f;
                                a.z = (zero13.z + zero14.z) * 0.5f;
                                vector20 = zero14;
                                vector21 = zero13;
                                a3 = zero16;
                                a4 = zero15;
                                float d = info.m_netAI.GetEndRadius() * 1.33333337f;
                                Vector3 vector32 = zero13 - zero15 * d;
                                Vector3 vector33 = vector20 - a3 * d;
                                Vector3 vector34 = zero14 - zero16 * d;
                                Vector3 vector35 = vector21 - a4 * d;
                                Vector3 vector36 = zero13 + zero15 * d;
                                Vector3 vector37 = vector20 + a3 * d;
                                Vector3 vector38 = zero14 + zero16 * d;
                                Vector3 vector39 = vector21 + a4 * d;
                                leftMatrix3 = NetSegment.CalculateControlMatrix(zero13, vector32, vector33, vector20, zero13, vector32, vector33, vector20, groupPosition, vscale3);
                                rightMatrix3 = NetSegment.CalculateControlMatrix(zero14, vector38, vector39, vector21, zero14, vector38, vector39, vector21, groupPosition, vscale3);
                                leftMatrixB = NetSegment.CalculateControlMatrix(zero13, vector36, vector37, vector20, zero13, vector36, vector37, vector20, groupPosition, vscale3);
                                rightMatrixB = NetSegment.CalculateControlMatrix(zero14, vector34, vector35, vector21, zero14, vector34, vector35, vector21, groupPosition, vscale3);
                                leftMatrix3.SetRow(3, leftMatrix3.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                                rightMatrix3.SetRow(3, rightMatrix3.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                                leftMatrixB.SetRow(3, leftMatrixB.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                                rightMatrixB.SetRow(3, rightMatrixB.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                                meshScale3 = new Vector4(0.5f / info3.m_halfWidth, 1f / info3.m_segmentLength, 0.5f - info3.m_pavementWidth / info3.m_halfWidth * 0.5f, info3.m_pavementWidth / info3.m_halfWidth * 0.5f);
                                centerPos = a - groupPosition;
                                centerPos.w = (leftMatrix3.m33 + rightMatrix3.m33 + leftMatrixB.m33 + rightMatrixB.m33) * 0.25f;
                                sideScale = new Vector4(info3.m_pavementWidth / info3.m_halfWidth * 0.5f, 1f, info3.m_pavementWidth / info3.m_halfWidth * 0.5f, 1f);
                            }
                            Vector4 colorLocation3;
                            Vector4 vector40;
                            if (NetNode.BlendJunction(nodeID))
                            {
                                colorLocation3 = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                                vector40 = colorLocation3;
                            }
                            else
                            {
                                colorLocation3 = RenderManager.GetColorLocation((uint)(49152 + segment3));
                                vector40 = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                            }
                            Vector4 vector41 = new Vector4(colorLocation3.x, colorLocation3.y, vector40.x, vector40.y);
                            for (int num15 = 0; num15 < info3.m_nodes.Length; num15++)
                            {
                                NetInfo.Node node3 = info3.m_nodes[num15];
                                if (node3.m_layer == layer && node3.CheckFlags(this.m_flags) && node3.m_combinedLod != null && !node3.m_directConnect)
                                {
                                    Vector4 objectIndex3 = vector41;
                                    if (node3.m_requireWindSpeed)
                                    {
                                        objectIndex3.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                                    }
                                    NetNode.PopulateGroupData(info3, node3, leftMatrix3, rightMatrix3, leftMatrixB, rightMatrixB, meshScale3, centerPos, sideScale, objectIndex3, ref vertexIndex, ref triangleIndex, data, ref requireSurfaceMaps);
                                }
                            }
                        }
                    }
                }
            }
        }
        else if ((info.m_netLayers & 1 << layer) != 0)
        {
            if ((this.m_flags & NetNode.Flags.End) != NetNode.Flags.None)
            {
                if (info.m_nodes != null && info.m_nodes.Length != 0)
                {
                    flag = true;
                    float vScale = info.m_netAI.GetVScale() / 1.5f;
                    Vector3 zero21 = Vector3.zero;
                    Vector3 zero22 = Vector3.zero;
                    Vector3 vector42 = Vector3.zero;
                    Vector3 vector43 = Vector3.zero;
                    Vector3 zero23 = Vector3.zero;
                    Vector3 zero24 = Vector3.zero;
                    Vector3 a5 = Vector3.zero;
                    Vector3 a6 = Vector3.zero;
                    bool flag11 = false;
                    ushort num16 = 0;
                    for (int num17 = 0; num17 < 8; num17++)
                    {
                        ushort segment5 = this.GetSegment(num17);
                        if (segment5 != 0)
                        {
                            NetSegment netSegment5 = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment5];
                            bool start4 = netSegment5.m_startNode == nodeID;
                            bool flag12;
                            netSegment5.CalculateCorner(segment5, true, start4, false, out zero21, out zero23, out flag12);
                            netSegment5.CalculateCorner(segment5, true, start4, true, out zero22, out zero24, out flag12);
                            if (flag11)
                            {
                                a5 = -zero23;
                                a6 = -zero24;
                                zero23.y = 0.25f;
                                zero24.y = 0.25f;
                                a5.y = -5f;
                                a6.y = -5f;
                                vector42 = zero21 - zero23 * 10f + a5 * 10f;
                                vector43 = zero22 - zero24 * 10f + a6 * 10f;
                            }
                            else
                            {
                                vector42 = zero22;
                                vector43 = zero21;
                                a5 = zero24;
                                a6 = zero23;
                            }
                            num16 = segment5;
                        }
                    }
                    if (flag11)
                    {
                        Vector3 vector44;
                        Vector3 vector45;
                        NetSegment.CalculateMiddlePoints(zero21, -zero23, vector42, -a5, true, true, out vector44, out vector45);
                        Vector3 vector46;
                        Vector3 vector47;
                        NetSegment.CalculateMiddlePoints(zero22, -zero24, vector43, -a6, true, true, out vector46, out vector47);
                        Matrix4x4 leftMatrix4 = NetSegment.CalculateControlMatrix(zero21, vector44, vector45, vector42, zero22, vector46, vector47, vector43, groupPosition, vScale);
                        Matrix4x4 rightMatrix4 = NetSegment.CalculateControlMatrix(zero22, vector46, vector47, vector43, zero21, vector44, vector45, vector42, groupPosition, vScale);
                        Vector4 meshScale4 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);
                        Vector4 colorLocation4 = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                        Vector4 vector48 = new Vector4(colorLocation4.x, colorLocation4.y, colorLocation4.x, colorLocation4.y);
                        if (info.m_segments != null && info.m_segments.Length != 0)
                        {
                            for (int num18 = 0; num18 < info.m_segments.Length; num18++)
                            {
                                NetInfo.Segment segment6 = info.m_segments[num18];
                                bool flag13;
                                if (segment6.m_layer == layer && segment6.CheckFlags(NetSegment.Flags.Bend | (Singleton<NetManager>.instance.m_segments.m_buffer[(int)num16].m_flags & NetSegment.Flags.Collapsed), out flag13) && segment6.m_combinedLod != null)
                                {
                                    Vector4 objectIndex4 = vector48;
                                    if (segment6.m_requireWindSpeed)
                                    {
                                        objectIndex4.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                                    }
                                    NetSegment.PopulateGroupData(info, segment6, leftMatrix4, rightMatrix4, meshScale4, objectIndex4, ref vertexIndex, ref triangleIndex, groupPosition, data, ref requireSurfaceMaps);
                                }
                            }
                        }
                    }
                    else
                    {
                        float d2 = info.m_netAI.GetEndRadius() * 1.33333337f;
                        Vector3 vector49 = zero21 - zero23 * d2;
                        Vector3 vector50 = vector42 - a5 * d2;
                        Vector3 vector51 = zero22 - zero24 * d2;
                        Vector3 vector52 = vector43 - a6 * d2;
                        Vector3 vector53 = zero21 + zero23 * d2;
                        Vector3 vector54 = vector42 + a5 * d2;
                        Vector3 vector55 = zero22 + zero24 * d2;
                        Vector3 vector56 = vector43 + a6 * d2;
                        Matrix4x4 leftMatrix5 = NetSegment.CalculateControlMatrix(zero21, vector49, vector50, vector42, zero21, vector49, vector50, vector42, groupPosition, vScale);
                        Matrix4x4 rightMatrix5 = NetSegment.CalculateControlMatrix(zero22, vector55, vector56, vector43, zero22, vector55, vector56, vector43, groupPosition, vScale);
                        Matrix4x4 leftMatrixB2 = NetSegment.CalculateControlMatrix(zero21, vector53, vector54, vector42, zero21, vector53, vector54, vector42, groupPosition, vScale);
                        Matrix4x4 rightMatrixB2 = NetSegment.CalculateControlMatrix(zero22, vector51, vector52, vector43, zero22, vector51, vector52, vector43, groupPosition, vScale);
                        leftMatrix5.SetRow(3, leftMatrix5.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                        rightMatrix5.SetRow(3, rightMatrix5.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                        leftMatrixB2.SetRow(3, leftMatrixB2.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                        rightMatrixB2.SetRow(3, rightMatrixB2.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                        Vector4 meshScale5 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 0.5f - info.m_pavementWidth / info.m_halfWidth * 0.5f, info.m_pavementWidth / info.m_halfWidth * 0.5f);
                        Vector4 centerPos2 = new Vector4(this.m_position.x - groupPosition.x, this.m_position.y - groupPosition.y + (float)this.m_heightOffset * 0.015625f, this.m_position.z - groupPosition.z, 0f);
                        centerPos2.w = (leftMatrix5.m33 + rightMatrix5.m33 + leftMatrixB2.m33 + rightMatrixB2.m33) * 0.25f;
                        Vector4 sideScale2 = new Vector4(info.m_pavementWidth / info.m_halfWidth * 0.5f, 1f, info.m_pavementWidth / info.m_halfWidth * 0.5f, 1f);
                        Vector4 colorLocation5 = RenderManager.GetColorLocation((uint)(49152 + num16));
                        Vector4 vector57 = new Vector4(colorLocation5.x, colorLocation5.y, colorLocation5.x, colorLocation5.y);
                        for (int num19 = 0; num19 < info.m_nodes.Length; num19++)
                        {
                            NetInfo.Node node4 = info.m_nodes[num19];
                            if (node4.m_layer == layer && node4.CheckFlags(this.m_flags) && node4.m_combinedLod != null && !node4.m_directConnect)
                            {
                                Vector4 objectIndex5 = vector57;
                                if (node4.m_requireWindSpeed)
                                {
                                    objectIndex5.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                                }
                                NetNode.PopulateGroupData(info, node4, leftMatrix5, rightMatrix5, leftMatrixB2, rightMatrixB2, meshScale5, centerPos2, sideScale2, objectIndex5, ref vertexIndex, ref triangleIndex, data, ref requireSurfaceMaps);
                            }
                        }
                    }
                }
            }
            else if ((this.m_flags & NetNode.Flags.Bend) != NetNode.Flags.None && ((info.m_segments != null && info.m_segments.Length != 0) || (info.m_nodes != null && info.m_nodes.Length != 0)))
            {
                float vscale4 = info.m_netAI.GetVScale();
                Vector3 zero25 = Vector3.zero;
                Vector3 zero26 = Vector3.zero;
                Vector3 zero27 = Vector3.zero;
                Vector3 zero28 = Vector3.zero;
                Vector3 zero29 = Vector3.zero;
                Vector3 zero30 = Vector3.zero;
                Vector3 zero31 = Vector3.zero;
                Vector3 zero32 = Vector3.zero;
                ushort num20 = 0;
                ushort num21 = 0;
                bool flag14 = false;
                int num22 = 0;
                for (int num23 = 0; num23 < 8; num23++)
                {
                    ushort segment7 = this.GetSegment(num23);
                    if (segment7 != 0)
                    {
                        NetSegment netSegment6 = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment7];
                        bool flag15 = ++num22 == 1;
                        bool flag16 = netSegment6.m_startNode == nodeID;
                        if ((!flag15 && !flag14) || (flag15 && !flag16))
                        {
                            bool flag17;
                            netSegment6.CalculateCorner(segment7, true, flag16, false, out zero25, out zero29, out flag17);
                            netSegment6.CalculateCorner(segment7, true, flag16, true, out zero26, out zero30, out flag17);
                            flag14 = true;
                            num20 = segment7;
                        }
                        else
                        {
                            bool flag17;
                            netSegment6.CalculateCorner(segment7, true, flag16, true, out zero27, out zero31, out flag17);
                            netSegment6.CalculateCorner(segment7, true, flag16, false, out zero28, out zero32, out flag17);
                            num21 = segment7;
                        }
                    }
                }
                Vector3 vector58;
                Vector3 vector59;
                NetSegment.CalculateMiddlePoints(zero25, -zero29, zero27, -zero31, true, true, out vector58, out vector59);
                Vector3 vector60;
                Vector3 vector61;
                NetSegment.CalculateMiddlePoints(zero26, -zero30, zero28, -zero32, true, true, out vector60, out vector61);
                Matrix4x4 leftMatrix6 = NetSegment.CalculateControlMatrix(zero25, vector58, vector59, zero27, zero26, vector60, vector61, zero28, groupPosition, vscale4);
                Matrix4x4 rightMatrix6 = NetSegment.CalculateControlMatrix(zero26, vector60, vector61, zero28, zero25, vector58, vector59, zero27, groupPosition, vscale4);
                Vector4 vector62 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);
                Vector4 colorLocation6 = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                Vector4 vector63 = new Vector4(colorLocation6.x, colorLocation6.y, colorLocation6.x, colorLocation6.y);
                if (info.m_segments != null && info.m_segments.Length != 0)
                {
                    for (int num24 = 0; num24 < info.m_segments.Length; num24++)
                    {
                        NetInfo.Segment segment8 = info.m_segments[num24];
                        bool flag18;
                        if (segment8.m_layer == layer && segment8.CheckFlags(info.m_netAI.GetBendFlags(nodeID, ref this), out flag18) && segment8.m_combinedLod != null && !segment8.m_disableBendNodes)
                        {
                            Vector4 objectIndex6 = vector63;
                            Vector4 meshScale6 = vector62;
                            if (segment8.m_requireWindSpeed)
                            {
                                objectIndex6.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                            }
                            if (flag18)
                            {
                                meshScale6.x = -meshScale6.x;
                                meshScale6.y = -meshScale6.y;
                            }
                            flag = true;
                            NetSegment.PopulateGroupData(info, segment8, leftMatrix6, rightMatrix6, meshScale6, objectIndex6, ref vertexIndex, ref triangleIndex, groupPosition, data, ref requireSurfaceMaps);
                        }
                    }
                }
                if (info.m_nodes != null && info.m_nodes.Length != 0)
                {
                    for (int num25 = 0; num25 < info.m_nodes.Length; num25++)
                    {
                        NetInfo.Node node5 = info.m_nodes[num25];
                        if ((node5.m_connectGroup == NetInfo.ConnectGroup.None || (node5.m_connectGroup & info.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None) && node5.m_layer == layer && node5.CheckFlags(this.m_flags) && node5.m_combinedLod != null && node5.m_directConnect)
                        {
                            Vector4 objectIndex7 = vector63;
                            Vector4 meshScale7 = vector62;
                            if (node5.m_requireWindSpeed)
                            {
                                objectIndex7.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                            }
                            if ((node5.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None)
                            {
                                NetManager instance2 = Singleton<NetManager>.instance;
                                bool flag19 = instance2.m_segments.m_buffer[(int)num20].m_startNode == nodeID == ((instance2.m_segments.m_buffer[(int)num20].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                bool flag20 = instance2.m_segments.m_buffer[(int)num21].m_startNode == nodeID == ((instance2.m_segments.m_buffer[(int)num21].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                if (flag19 == flag20)
                                {
                                    goto IL_21EC;
                                }
                                if (flag19)
                                {
                                    if ((node5.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None)
                                    {
                                        goto IL_21EC;
                                    }
                                }
                                else
                                {
                                    if ((node5.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None)
                                    {
                                        goto IL_21EC;
                                    }
                                    meshScale7.x = -meshScale7.x;
                                    meshScale7.y = -meshScale7.y;
                                }
                            }
                            flag = true;
                            NetNode.PopulateGroupData(info, node5, leftMatrix6, rightMatrix6, meshScale7, objectIndex7, ref vertexIndex, ref triangleIndex, data, ref requireSurfaceMaps);
                        }
                    IL_21EC:;
                    }
                }
            }
        }
        if (flag)
        {
            min = Vector3.Min(min, this.m_bounds.min);
            max = Vector3.Max(max, this.m_bounds.max);
            maxRenderDistance = Mathf.Max(maxRenderDistance, 30000f);
            maxInstanceDistance = Mathf.Max(maxInstanceDistance, 1000f);
        }
    }

    public bool CalculateGroupData(ushort nodeID, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
    {
        bool result = false;
        NetInfo info = this.Info;
        if (this.m_problems != Notification.Problem.None && layer == Singleton<NotificationManager>.instance.m_notificationLayer && (this.m_flags & NetNode.Flags.Temporary) == NetNode.Flags.None && Notification.CalculateGroupData(ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays))
        {
            result = true;
        }
        if ((this.m_flags & NetNode.Flags.Junction) != NetNode.Flags.None)
        {
            NetManager instance = Singleton<NetManager>.instance;
            Vector3 a = this.m_position;
            for (int i1 = 0; i1 < 8; i1++)
            {
                ushort segmentID1 = this.GetSegment(i1);
                if (segmentID1 != 0)
                {
                    NetInfo info1 = instance.m_segments.m_buffer[(int)segmentID1].Info;
                    ItemClass connectionClass = info1.GetConnectionClass();
                    Vector3 a2 = (nodeID != instance.m_segments.m_buffer[(int)segmentID1].m_startNode) ? instance.m_segments.m_buffer[(int)segmentID1].m_endDirection : instance.m_segments.m_buffer[(int)segmentID1].m_startDirection;
                    float num = -1f;
                    for (int i2 = 0; i2 < 8; i2++)
                    {
                        ushort segmentID2 = this.GetSegment(i2);
                        if (segmentID2 != 0 && segmentID2 != segmentID1)
                        {
                            NetInfo info2 = instance.m_segments.m_buffer[(int)segmentID2].Info;
                            ItemClass connectionClass2 = info2.GetConnectionClass();
                            if (((info.m_netLayers | info1.m_netLayers | info2.m_netLayers) & 1 << layer) != 0 && (connectionClass.m_service == connectionClass2.m_service || (info1.m_nodeConnectGroups & info2.m_connectGroup) != NetInfo.ConnectGroup.None || (info2.m_nodeConnectGroups & info1.m_connectGroup) != NetInfo.ConnectGroup.None))
                            {
                                Vector3 vector = (nodeID != instance.m_segments.m_buffer[(int)segmentID2].m_startNode) ? instance.m_segments.m_buffer[(int)segmentID2].m_endDirection : instance.m_segments.m_buffer[(int)segmentID2].m_startDirection;
                                float num2 = a2.x * vector.x + a2.z * vector.z;
                                num = Mathf.Max(num, num2);
                                bool flag = info1.m_requireDirectRenderers && (info1.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (info1.m_nodeConnectGroups & info2.m_connectGroup) != NetInfo.ConnectGroup.None);
                                bool flag2 = info2.m_requireDirectRenderers && (info2.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (info2.m_nodeConnectGroups & info1.m_connectGroup) != NetInfo.ConnectGroup.None);
                                if (i2 > i1 && (flag || flag2))
                                {
                                    float num3 = 0.01f - Mathf.Min(info1.m_maxTurnAngleCos, info2.m_maxTurnAngleCos);
                                    if (num2 < num3)
                                    {
                                        float num4;
                                        if (flag)
                                        {
                                            num4 = info1.m_netAI.GetNodeInfoPriority(segmentID1, ref instance.m_segments.m_buffer[(int)segmentID1]);
                                        }
                                        else
                                        {
                                            num4 = -1E+08f;
                                        }
                                        float num5;
                                        if (flag2)
                                        {
                                            num5 = info2.m_netAI.GetNodeInfoPriority(segmentID2, ref instance.m_segments.m_buffer[(int)segmentID2]);
                                        }
                                        else
                                        {
                                            num5 = -1E+08f;
                                        }
                                        if (num4 >= num5)
                                        {
                                            if (info1.m_nodes != null && info1.m_nodes.Length != 0)
                                            {
                                                result = true;
                                                for (int k = 0; k < info1.m_nodes.Length; k++)
                                                {
                                                    NetInfo.Node node1 = info1.m_nodes[k];
                                                    if ((node1.m_connectGroup == NetInfo.ConnectGroup.None ||(node1.m_connectGroup & info2.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None) &&
                                                        node1.m_layer == layer && node1.CheckFlags(this.m_flags) && node1.m_combinedLod != null && node1.m_directConnect)
                                                    {
                                                        if ((node1.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None)
                                                        {
                                                            bool flag3 = instance.m_segments.m_buffer[(int)segmentID1].m_startNode == nodeID == ((instance.m_segments.m_buffer[(int)segmentID1].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                            if (info2.m_hasBackwardVehicleLanes != info2.m_hasForwardVehicleLanes || (node1.m_connectGroup & NetInfo.ConnectGroup.Directional) != NetInfo.ConnectGroup.None)
                                                            {
                                                                bool flag4 = instance.m_segments.m_buffer[(int)segmentID2].m_startNode == nodeID == ((instance.m_segments.m_buffer[(int)segmentID2].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                                if (flag3 == flag4)
                                                                {
                                                                    goto IL_4C3;
                                                                }
                                                            }
                                                            if (flag3)
                                                            {
                                                                if ((node1.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None)
                                                                {
                                                                    goto IL_4C3;
                                                                }
                                                            }
                                                            else if ((node1.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None)
                                                            {
                                                                goto IL_4C3;
                                                            }
                                                        }
                                                        NetNode.CalculateGroupData(node1, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                                                    }
                                                IL_4C3:;
                                                }
                                            }
                                        }
                                        else if (info2.m_nodes != null && info2.m_nodes.Length != 0)
                                        {
                                            result = true;
                                            for (int l = 0; l < info2.m_nodes.Length; l++)
                                            {
                                                NetInfo.Node node2 = info2.m_nodes[l];
                                                if ((node2.m_connectGroup == NetInfo.ConnectGroup.None || (node2.m_connectGroup & info1.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None) &&
                                                    node2.m_layer == layer && node2.CheckFlags(this.m_flags) && node2.m_combinedLod != null && node2.m_directConnect)
                                                {
                                                    if ((node2.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None)
                                                    {
                                                        bool flag5 = instance.m_segments.m_buffer[(int)segmentID2].m_startNode == nodeID == ((instance.m_segments.m_buffer[(int)segmentID2].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                        if (info1.m_hasBackwardVehicleLanes != info1.m_hasForwardVehicleLanes || (node2.m_connectGroup & NetInfo.ConnectGroup.Directional) != NetInfo.ConnectGroup.None)
                                                        {
                                                            bool flag6 = instance.m_segments.m_buffer[(int)segmentID1].m_startNode == nodeID == ((instance.m_segments.m_buffer[(int)segmentID1].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                            if (flag6 == flag5)
                                                            {
                                                                goto IL_66E;
                                                            }
                                                        }
                                                        if (flag5)
                                                        {
                                                            if ((node2.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None)
                                                            {
                                                                goto IL_66E;
                                                            }
                                                        }
                                                        else if ((node2.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None)
                                                        {
                                                            goto IL_66E;
                                                        }
                                                    }
                                                    NetNode.CalculateGroupData(node2, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                                                }
                                            IL_66E:;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    a += a2 * (2f + num * 2f);
                }
            }
            a.y = this.m_position.y + (float)this.m_heightOffset * 0.015625f;
            if ((info.m_netLayers & 1 << layer) != 0 && info.m_requireSegmentRenderers)
            {
                for (int i3 = 0; i3 < 8; i3++)
                {
                    ushort segmentID3 = this.GetSegment(i3);
                    if (segmentID3 != 0)
                    {
                        NetInfo info3 = instance.m_segments.m_buffer[(int)segmentID3].Info;
                        if (info3.m_nodes != null && info3.m_nodes.Length != 0)
                        {
                            result = true;
                            for (int n = 0; n < info3.m_nodes.Length; n++)
                            {
                                NetInfo.Node node3 = info3.m_nodes[n];
                                if (node3.m_layer == layer && node3.CheckFlags(this.m_flags) && node3.m_combinedLod != null && !node3.m_directConnect)
                                {
                                    NetNode.CalculateGroupData(node3, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                                }
                            }
                        }
                    }
                }
            }
        }
        else if ((info.m_netLayers & 1 << layer) != 0)
        {
            if ((this.m_flags & NetNode.Flags.End) != NetNode.Flags.None)
            {
                if (info.m_nodes != null && info.m_nodes.Length != 0)
                {
                    result = true;
                    for (int num6 = 0; num6 < info.m_nodes.Length; num6++)
                    {
                        NetInfo.Node node = info.m_nodes[num6];
                        if (node.m_layer == layer && node.CheckFlags(this.m_flags) && node.m_combinedLod != null && !node.m_directConnect)
                        {
                            NetNode.CalculateGroupData(node, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                        }
                    }
                }
            }
            else if ((this.m_flags & NetNode.Flags.Bend) != NetNode.Flags.None)
            {
                if (info.m_segments != null && info.m_segments.Length != 0)
                {
                    result = true;
                    for (int num7 = 0; num7 < info.m_segments.Length; num7++)
                    {
                        NetInfo.Segment segment4 = info.m_segments[num7];
                        bool flag7;
                        if (segment4.m_layer == layer && segment4.CheckFlags(info.m_netAI.GetBendFlags(nodeID, ref this), out flag7) && segment4.m_combinedLod != null && !segment4.m_disableBendNodes)
                        {
                            NetSegment.CalculateGroupData(segment4, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                        }
                    }
                }
                if (info.m_nodes != null && info.m_nodes.Length != 0)
                {
                    result = true;
                    for (int num8 = 0; num8 < info.m_nodes.Length; num8++)
                    {
                        NetInfo.Node node5 = info.m_nodes[num8];
                        if ((node5.m_connectGroup == NetInfo.ConnectGroup.None || (node5.m_connectGroup & info.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None) && node5.m_layer == layer && node5.CheckFlags(this.m_flags) && node5.m_combinedLod != null && node5.m_directConnect)
                        {
                            if ((node5.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None)
                            {
                                NetManager instance2 = Singleton<NetManager>.instance;
                                ushort num9 = 0;
                                ushort num10 = 0;
                                bool flag8 = false;
                                int num11 = 0;
                                for (int num12 = 0; num12 < 8; num12++)
                                {
                                    ushort segment5 = this.GetSegment(num12);
                                    if (segment5 != 0)
                                    {
                                        NetSegment netSegment = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment5];
                                        bool flag9 = ++num11 == 1;
                                        bool flag10 = netSegment.m_startNode == nodeID;
                                        if ((!flag9 && !flag8) || (flag9 && !flag10))
                                        {
                                            flag8 = true;
                                            num9 = segment5;
                                        }
                                        else
                                        {
                                            num10 = segment5;
                                        }
                                    }
                                }
                                bool flag11 = instance2.m_segments.m_buffer[(int)num9].m_startNode == nodeID == ((instance2.m_segments.m_buffer[(int)num9].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                bool flag12 = instance2.m_segments.m_buffer[(int)num10].m_startNode == nodeID == ((instance2.m_segments.m_buffer[(int)num10].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                if (flag11 == flag12)
                                {
                                    goto IL_B19;
                                }
                                if (flag11)
                                {
                                    if ((node5.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None)
                                    {
                                        goto IL_B19;
                                    }
                                }
                                else if ((node5.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None)
                                {
                                    goto IL_B19;
                                }
                            }
                            NetNode.CalculateGroupData(node5, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                        }
                    IL_B19:;
                    }
                }
            }
        }
        return result;
    }

    private void RenderInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, NetInfo info, int iter, NetNode.Flags flags, ref uint instanceIndex, ref RenderManager.Instance data)
    {
        if (data.m_dirty)
        {
            data.m_dirty = false;
            if (iter == 0)
            {
                if ((flags & NetNode.Flags.Junction) != NetNode.Flags.None)
                {
                    this.RefreshJunctionData(nodeID, info, instanceIndex);
                }
                else if ((flags & NetNode.Flags.Bend) != NetNode.Flags.None)
                {
                    this.RefreshBendData(nodeID, info, instanceIndex, ref data);
                }
                else if ((flags & NetNode.Flags.End) != NetNode.Flags.None)
                {
                    this.RefreshEndData(nodeID, info, instanceIndex, ref data);
                }
            }
        }
        if (data.m_initialized)
        {
            if ((flags & NetNode.Flags.Junction) != NetNode.Flags.None)
            {
                if ((data.m_dataInt0 & 8) != 0)
                {
                    ushort segment = this.GetSegment(data.m_dataInt0 & 7);
                    ushort segment2 = this.GetSegment(data.m_dataInt0 >> 4);
                    if (segment != 0 && segment2 != 0)
                    {
                        NetManager instance = Singleton<NetManager>.instance;
                        info = instance.m_segments.m_buffer[(int)segment].Info;
                        NetInfo info2 = instance.m_segments.m_buffer[(int)segment2].Info;
                        NetNode.Flags flags2 = flags;
                        if (((instance.m_segments.m_buffer[(int)segment].m_flags | instance.m_segments.m_buffer[(int)segment2].m_flags) & NetSegment.Flags.Collapsed) != NetSegment.Flags.None)
                        {
                            flags2 |= NetNode.Flags.Collapsed;
                        }
                        for (int i = 0; i < info.m_nodes.Length; i++)
                        {
                            NetInfo.Node node = info.m_nodes[i];
                            if (node.CheckFlags(flags2) && node.m_directConnect && (node.m_connectGroup == NetInfo.ConnectGroup.None || (node.m_connectGroup & info2.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None))
                            {
                                Vector4 dataVector = data.m_dataVector3;
                                Vector4 dataVector2 = data.m_dataVector0;
                                if (node.m_requireWindSpeed)
                                {
                                    dataVector.w = data.m_dataFloat0;
                                }
                                if ((node.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None)
                                {
                                    bool flag = instance.m_segments.m_buffer[(int)segment].m_startNode == nodeID == ((instance.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                    if (info2.m_hasBackwardVehicleLanes != info2.m_hasForwardVehicleLanes || (node.m_connectGroup & NetInfo.ConnectGroup.Directional) != NetInfo.ConnectGroup.None)
                                    {
                                        bool flag2 = instance.m_segments.m_buffer[(int)segment2].m_startNode == nodeID == ((instance.m_segments.m_buffer[(int)segment2].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                        if (flag == flag2)
                                        {
                                            goto IL_570;
                                        }
                                    }
                                    if (flag)
                                    {
                                        if ((node.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None)
                                        {
                                            goto IL_570;
                                        }
                                    }
                                    else
                                    {
                                        if ((node.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None)
                                        {
                                            goto IL_570;
                                        }
                                        dataVector2.x = -dataVector2.x;
                                        dataVector2.y = -dataVector2.y;
                                    }
                                }
                                if (cameraInfo.CheckRenderDistance(data.m_position, node.m_lodRenderDistance))
                                {
                                    instance.m_materialBlock.Clear();
                                    instance.m_materialBlock.SetMatrix(instance.ID_LeftMatrix, data.m_dataMatrix0);
                                    instance.m_materialBlock.SetMatrix(instance.ID_RightMatrix, data.m_extraData.m_dataMatrix2);
                                    instance.m_materialBlock.SetVector(instance.ID_MeshScale, dataVector2);
                                    instance.m_materialBlock.SetVector(instance.ID_ObjectIndex, dataVector);
                                    instance.m_materialBlock.SetColor(instance.ID_Color, data.m_dataColor0);
                                    if (node.m_requireSurfaceMaps && data.m_dataTexture1 != null)
                                    {
                                        instance.m_materialBlock.SetTexture(instance.ID_SurfaceTexA, data.m_dataTexture0);
                                        instance.m_materialBlock.SetTexture(instance.ID_SurfaceTexB, data.m_dataTexture1);
                                        instance.m_materialBlock.SetVector(instance.ID_SurfaceMapping, data.m_dataVector1);
                                    }
                                    NetManager netManager = instance;
                                    netManager.m_drawCallData.m_defaultCalls = netManager.m_drawCallData.m_defaultCalls + 1;
                                    Graphics.DrawMesh(node.m_nodeMesh, data.m_position, data.m_rotation, node.m_nodeMaterial, node.m_layer, null, 0, instance.m_materialBlock);
                                }
                                else
                                {
                                    NetInfo.LodValue combinedLod = node.m_combinedLod;
                                    if (combinedLod != null)
                                    {
                                        if (node.m_requireSurfaceMaps && data.m_dataTexture0 != combinedLod.m_surfaceTexA)
                                        {
                                            if (combinedLod.m_lodCount != 0)
                                            {
                                                NetSegment.RenderLod(cameraInfo, combinedLod);
                                            }
                                            combinedLod.m_surfaceTexA = data.m_dataTexture0;
                                            combinedLod.m_surfaceTexB = data.m_dataTexture1;
                                            combinedLod.m_surfaceMapping = data.m_dataVector1;
                                        }
                                        combinedLod.m_leftMatrices[combinedLod.m_lodCount] = data.m_dataMatrix0;
                                        combinedLod.m_rightMatrices[combinedLod.m_lodCount] = data.m_extraData.m_dataMatrix2;
                                        combinedLod.m_meshScales[combinedLod.m_lodCount] = dataVector2;
                                        combinedLod.m_objectIndices[combinedLod.m_lodCount] = dataVector;
                                        combinedLod.m_meshLocations[combinedLod.m_lodCount] = data.m_position;
                                        combinedLod.m_lodMin = Vector3.Min(combinedLod.m_lodMin, data.m_position);
                                        combinedLod.m_lodMax = Vector3.Max(combinedLod.m_lodMax, data.m_position);
                                        if (++combinedLod.m_lodCount == combinedLod.m_leftMatrices.Length)
                                        {
                                            NetSegment.RenderLod(cameraInfo, combinedLod);
                                        }
                                    }
                                }
                            }
                        IL_570:;
                        }
                    }
                }
                else
                {
                    ushort segment3 = this.GetSegment(data.m_dataInt0 & 7);
                    if (segment3 != 0)
                    {
                        NetManager instance2 = Singleton<NetManager>.instance;
                        info = instance2.m_segments.m_buffer[(int)segment3].Info;
                        for (int j = 0; j < info.m_nodes.Length; j++)
                        {
                            NetInfo.Node node2 = info.m_nodes[j];
                            if (node2.CheckFlags(flags) && !node2.m_directConnect)
                            {
                                Vector4 dataVector3 = data.m_extraData.m_dataVector4;
                                if (node2.m_requireWindSpeed)
                                {
                                    dataVector3.w = data.m_dataFloat0;
                                }
                                if (cameraInfo.CheckRenderDistance(data.m_position, node2.m_lodRenderDistance))
                                {
                                    instance2.m_materialBlock.Clear();
                                    instance2.m_materialBlock.SetMatrix(instance2.ID_LeftMatrix, data.m_dataMatrix0);
                                    instance2.m_materialBlock.SetMatrix(instance2.ID_RightMatrix, data.m_extraData.m_dataMatrix2);
                                    instance2.m_materialBlock.SetMatrix(instance2.ID_LeftMatrixB, data.m_extraData.m_dataMatrix3);
                                    instance2.m_materialBlock.SetMatrix(instance2.ID_RightMatrixB, data.m_dataMatrix1);
                                    instance2.m_materialBlock.SetVector(instance2.ID_MeshScale, data.m_dataVector0);
                                    instance2.m_materialBlock.SetVector(instance2.ID_CenterPos, data.m_dataVector1);
                                    instance2.m_materialBlock.SetVector(instance2.ID_SideScale, data.m_dataVector2);
                                    instance2.m_materialBlock.SetVector(instance2.ID_ObjectIndex, dataVector3);
                                    instance2.m_materialBlock.SetColor(instance2.ID_Color, data.m_dataColor0);
                                    if (node2.m_requireSurfaceMaps && data.m_dataTexture1 != null)
                                    {
                                        instance2.m_materialBlock.SetTexture(instance2.ID_SurfaceTexA, data.m_dataTexture0);
                                        instance2.m_materialBlock.SetTexture(instance2.ID_SurfaceTexB, data.m_dataTexture1);
                                        instance2.m_materialBlock.SetVector(instance2.ID_SurfaceMapping, data.m_dataVector3);
                                    }
                                    NetManager netManager2 = instance2;
                                    netManager2.m_drawCallData.m_defaultCalls = netManager2.m_drawCallData.m_defaultCalls + 1;
                                    Graphics.DrawMesh(node2.m_nodeMesh, data.m_position, data.m_rotation, node2.m_nodeMaterial, node2.m_layer, null, 0, instance2.m_materialBlock);
                                }
                                else
                                {
                                    NetInfo.LodValue combinedLod2 = node2.m_combinedLod;
                                    if (combinedLod2 != null)
                                    {
                                        if (node2.m_requireSurfaceMaps && data.m_dataTexture0 != combinedLod2.m_surfaceTexA)
                                        {
                                            if (combinedLod2.m_lodCount != 0)
                                            {
                                                NetNode.RenderLod(cameraInfo, combinedLod2);
                                            }
                                            combinedLod2.m_surfaceTexA = data.m_dataTexture0;
                                            combinedLod2.m_surfaceTexB = data.m_dataTexture1;
                                            combinedLod2.m_surfaceMapping = data.m_dataVector3;
                                        }
                                        combinedLod2.m_leftMatrices[combinedLod2.m_lodCount] = data.m_dataMatrix0;
                                        combinedLod2.m_leftMatricesB[combinedLod2.m_lodCount] = data.m_extraData.m_dataMatrix3;
                                        combinedLod2.m_rightMatrices[combinedLod2.m_lodCount] = data.m_extraData.m_dataMatrix2;
                                        combinedLod2.m_rightMatricesB[combinedLod2.m_lodCount] = data.m_dataMatrix1;
                                        combinedLod2.m_meshScales[combinedLod2.m_lodCount] = data.m_dataVector0;
                                        combinedLod2.m_centerPositions[combinedLod2.m_lodCount] = data.m_dataVector1;
                                        combinedLod2.m_sideScales[combinedLod2.m_lodCount] = data.m_dataVector2;
                                        combinedLod2.m_objectIndices[combinedLod2.m_lodCount] = dataVector3;
                                        combinedLod2.m_meshLocations[combinedLod2.m_lodCount] = data.m_position;
                                        combinedLod2.m_lodMin = Vector3.Min(combinedLod2.m_lodMin, data.m_position);
                                        combinedLod2.m_lodMax = Vector3.Max(combinedLod2.m_lodMax, data.m_position);
                                        if (++combinedLod2.m_lodCount == combinedLod2.m_leftMatrices.Length)
                                        {
                                            NetNode.RenderLod(cameraInfo, combinedLod2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if ((flags & NetNode.Flags.End) != NetNode.Flags.None)
            {
                NetManager instance3 = Singleton<NetManager>.instance;
                for (int k = 0; k < info.m_nodes.Length; k++)
                {
                    NetInfo.Node node3 = info.m_nodes[k];
                    if (node3.CheckFlags(flags) && !node3.m_directConnect)
                    {
                        Vector4 dataVector4 = data.m_extraData.m_dataVector4;
                        if (node3.m_requireWindSpeed)
                        {
                            dataVector4.w = data.m_dataFloat0;
                        }
                        if (cameraInfo.CheckRenderDistance(data.m_position, node3.m_lodRenderDistance))
                        {
                            instance3.m_materialBlock.Clear();
                            instance3.m_materialBlock.SetMatrix(instance3.ID_LeftMatrix, data.m_dataMatrix0);
                            instance3.m_materialBlock.SetMatrix(instance3.ID_RightMatrix, data.m_extraData.m_dataMatrix2);
                            instance3.m_materialBlock.SetMatrix(instance3.ID_LeftMatrixB, data.m_extraData.m_dataMatrix3);
                            instance3.m_materialBlock.SetMatrix(instance3.ID_RightMatrixB, data.m_dataMatrix1);
                            instance3.m_materialBlock.SetVector(instance3.ID_MeshScale, data.m_dataVector0);
                            instance3.m_materialBlock.SetVector(instance3.ID_CenterPos, data.m_dataVector1);
                            instance3.m_materialBlock.SetVector(instance3.ID_SideScale, data.m_dataVector2);
                            instance3.m_materialBlock.SetVector(instance3.ID_ObjectIndex, dataVector4);
                            instance3.m_materialBlock.SetColor(instance3.ID_Color, data.m_dataColor0);
                            if (node3.m_requireSurfaceMaps && data.m_dataTexture1 != null)
                            {
                                instance3.m_materialBlock.SetTexture(instance3.ID_SurfaceTexA, data.m_dataTexture0);
                                instance3.m_materialBlock.SetTexture(instance3.ID_SurfaceTexB, data.m_dataTexture1);
                                instance3.m_materialBlock.SetVector(instance3.ID_SurfaceMapping, data.m_dataVector3);
                            }
                            NetManager netManager3 = instance3;
                            netManager3.m_drawCallData.m_defaultCalls = netManager3.m_drawCallData.m_defaultCalls + 1;
                            Graphics.DrawMesh(node3.m_nodeMesh, data.m_position, data.m_rotation, node3.m_nodeMaterial, node3.m_layer, null, 0, instance3.m_materialBlock);
                        }
                        else
                        {
                            NetInfo.LodValue combinedLod3 = node3.m_combinedLod;
                            if (combinedLod3 != null)
                            {
                                if (node3.m_requireSurfaceMaps && data.m_dataTexture0 != combinedLod3.m_surfaceTexA)
                                {
                                    if (combinedLod3.m_lodCount != 0)
                                    {
                                        NetNode.RenderLod(cameraInfo, combinedLod3);
                                    }
                                    combinedLod3.m_surfaceTexA = data.m_dataTexture0;
                                    combinedLod3.m_surfaceTexB = data.m_dataTexture1;
                                    combinedLod3.m_surfaceMapping = data.m_dataVector3;
                                }
                                combinedLod3.m_leftMatrices[combinedLod3.m_lodCount] = data.m_dataMatrix0;
                                combinedLod3.m_leftMatricesB[combinedLod3.m_lodCount] = data.m_extraData.m_dataMatrix3;
                                combinedLod3.m_rightMatrices[combinedLod3.m_lodCount] = data.m_extraData.m_dataMatrix2;
                                combinedLod3.m_rightMatricesB[combinedLod3.m_lodCount] = data.m_dataMatrix1;
                                combinedLod3.m_meshScales[combinedLod3.m_lodCount] = data.m_dataVector0;
                                combinedLod3.m_centerPositions[combinedLod3.m_lodCount] = data.m_dataVector1;
                                combinedLod3.m_sideScales[combinedLod3.m_lodCount] = data.m_dataVector2;
                                combinedLod3.m_objectIndices[combinedLod3.m_lodCount] = dataVector4;
                                combinedLod3.m_meshLocations[combinedLod3.m_lodCount] = data.m_position;
                                combinedLod3.m_lodMin = Vector3.Min(combinedLod3.m_lodMin, data.m_position);
                                combinedLod3.m_lodMax = Vector3.Max(combinedLod3.m_lodMax, data.m_position);
                                if (++combinedLod3.m_lodCount == combinedLod3.m_leftMatrices.Length)
                                {
                                    NetNode.RenderLod(cameraInfo, combinedLod3);
                                }
                            }
                        }
                    }
                }
            }
            else if ((flags & NetNode.Flags.Bend) != NetNode.Flags.None)
            {
                NetManager instance4 = Singleton<NetManager>.instance;
                for (int l = 0; l < info.m_segments.Length; l++)
                {
                    NetInfo.Segment segment4 = info.m_segments[l];
                    bool flag3;
                    if (segment4.CheckFlags(info.m_netAI.GetBendFlags(nodeID, ref this), out flag3) && !segment4.m_disableBendNodes)
                    {
                        Vector4 dataVector5 = data.m_dataVector3;
                        Vector4 dataVector6 = data.m_dataVector0;
                        if (segment4.m_requireWindSpeed)
                        {
                            dataVector5.w = data.m_dataFloat0;
                        }
                        if (flag3)
                        {
                            dataVector6.x = -dataVector6.x;
                            dataVector6.y = -dataVector6.y;
                        }
                        if (cameraInfo.CheckRenderDistance(data.m_position, segment4.m_lodRenderDistance))
                        {
                            instance4.m_materialBlock.Clear();
                            instance4.m_materialBlock.SetMatrix(instance4.ID_LeftMatrix, data.m_dataMatrix0);
                            instance4.m_materialBlock.SetMatrix(instance4.ID_RightMatrix, data.m_extraData.m_dataMatrix2);
                            instance4.m_materialBlock.SetVector(instance4.ID_MeshScale, dataVector6);
                            instance4.m_materialBlock.SetVector(instance4.ID_ObjectIndex, dataVector5);
                            instance4.m_materialBlock.SetColor(instance4.ID_Color, data.m_dataColor0);
                            if (segment4.m_requireSurfaceMaps && data.m_dataTexture1 != null)
                            {
                                instance4.m_materialBlock.SetTexture(instance4.ID_SurfaceTexA, data.m_dataTexture0);
                                instance4.m_materialBlock.SetTexture(instance4.ID_SurfaceTexB, data.m_dataTexture1);
                                instance4.m_materialBlock.SetVector(instance4.ID_SurfaceMapping, data.m_dataVector1);
                            }
                            NetManager netManager4 = instance4;
                            netManager4.m_drawCallData.m_defaultCalls = netManager4.m_drawCallData.m_defaultCalls + 1;
                            Graphics.DrawMesh(segment4.m_segmentMesh, data.m_position, data.m_rotation, segment4.m_segmentMaterial, segment4.m_layer, null, 0, instance4.m_materialBlock);
                        }
                        else
                        {
                            NetInfo.LodValue combinedLod4 = segment4.m_combinedLod;
                            if (combinedLod4 != null)
                            {
                                if (segment4.m_requireSurfaceMaps && data.m_dataTexture0 != combinedLod4.m_surfaceTexA)
                                {
                                    if (combinedLod4.m_lodCount != 0)
                                    {
                                        NetSegment.RenderLod(cameraInfo, combinedLod4);
                                    }
                                    combinedLod4.m_surfaceTexA = data.m_dataTexture0;
                                    combinedLod4.m_surfaceTexB = data.m_dataTexture1;
                                    combinedLod4.m_surfaceMapping = data.m_dataVector1;
                                }
                                combinedLod4.m_leftMatrices[combinedLod4.m_lodCount] = data.m_dataMatrix0;
                                combinedLod4.m_rightMatrices[combinedLod4.m_lodCount] = data.m_extraData.m_dataMatrix2;
                                combinedLod4.m_meshScales[combinedLod4.m_lodCount] = dataVector6;
                                combinedLod4.m_objectIndices[combinedLod4.m_lodCount] = dataVector5;
                                combinedLod4.m_meshLocations[combinedLod4.m_lodCount] = data.m_position;
                                combinedLod4.m_lodMin = Vector3.Min(combinedLod4.m_lodMin, data.m_position);
                                combinedLod4.m_lodMax = Vector3.Max(combinedLod4.m_lodMax, data.m_position);
                                if (++combinedLod4.m_lodCount == combinedLod4.m_leftMatrices.Length)
                                {
                                    NetSegment.RenderLod(cameraInfo, combinedLod4);
                                }
                            }
                        }
                    }
                }
                for (int m = 0; m < info.m_nodes.Length; m++)
                {
                    ushort segment5 = this.GetSegment(data.m_dataInt0 & 7);
                    ushort segment6 = this.GetSegment(data.m_dataInt0 >> 4);
                    if (((instance4.m_segments.m_buffer[(int)segment5].m_flags | instance4.m_segments.m_buffer[(int)segment6].m_flags) & NetSegment.Flags.Collapsed) != NetSegment.Flags.None)
                    {
                        NetNode.Flags flags3 = flags | NetNode.Flags.Collapsed;
                    }
                    NetInfo.Node node4 = info.m_nodes[m];
                    if (node4.CheckFlags(flags) && node4.m_directConnect && (node4.m_connectGroup == NetInfo.ConnectGroup.None || (node4.m_connectGroup & info.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None))
                    {
                        Vector4 dataVector7 = data.m_dataVector3;
                        Vector4 dataVector8 = data.m_dataVector0;
                        if (node4.m_requireWindSpeed)
                        {
                            dataVector7.w = data.m_dataFloat0;
                        }
                        if ((node4.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None)
                        {
                            bool flag4 = instance4.m_segments.m_buffer[(int)segment5].m_startNode == nodeID == ((instance4.m_segments.m_buffer[(int)segment5].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                            bool flag5 = instance4.m_segments.m_buffer[(int)segment6].m_startNode == nodeID == ((instance4.m_segments.m_buffer[(int)segment6].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                            if (flag4 == flag5)
                            {
                                goto IL_1637;
                            }
                            if (flag4)
                            {
                                if ((node4.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None)
                                {
                                    goto IL_1637;
                                }
                            }
                            else
                            {
                                if ((node4.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None)
                                {
                                    goto IL_1637;
                                }
                                dataVector8.x = -dataVector8.x;
                                dataVector8.y = -dataVector8.y;
                            }
                        }
                        if (cameraInfo.CheckRenderDistance(data.m_position, node4.m_lodRenderDistance))
                        {
                            instance4.m_materialBlock.Clear();
                            instance4.m_materialBlock.SetMatrix(instance4.ID_LeftMatrix, data.m_dataMatrix0);
                            instance4.m_materialBlock.SetMatrix(instance4.ID_RightMatrix, data.m_extraData.m_dataMatrix2);
                            instance4.m_materialBlock.SetVector(instance4.ID_MeshScale, dataVector8);
                            instance4.m_materialBlock.SetVector(instance4.ID_ObjectIndex, dataVector7);
                            instance4.m_materialBlock.SetColor(instance4.ID_Color, data.m_dataColor0);
                            if (node4.m_requireSurfaceMaps && data.m_dataTexture1 != null)
                            {
                                instance4.m_materialBlock.SetTexture(instance4.ID_SurfaceTexA, data.m_dataTexture0);
                                instance4.m_materialBlock.SetTexture(instance4.ID_SurfaceTexB, data.m_dataTexture1);
                                instance4.m_materialBlock.SetVector(instance4.ID_SurfaceMapping, data.m_dataVector1);
                            }
                            NetManager netManager5 = instance4;
                            netManager5.m_drawCallData.m_defaultCalls = netManager5.m_drawCallData.m_defaultCalls + 1;
                            Graphics.DrawMesh(node4.m_nodeMesh, data.m_position, data.m_rotation, node4.m_nodeMaterial, node4.m_layer, null, 0, instance4.m_materialBlock);
                        }
                        else
                        {
                            NetInfo.LodValue combinedLod5 = node4.m_combinedLod;
                            if (combinedLod5 != null)
                            {
                                if (node4.m_requireSurfaceMaps && data.m_dataTexture0 != combinedLod5.m_surfaceTexA)
                                {
                                    if (combinedLod5.m_lodCount != 0)
                                    {
                                        NetSegment.RenderLod(cameraInfo, combinedLod5);
                                    }
                                    combinedLod5.m_surfaceTexA = data.m_dataTexture0;
                                    combinedLod5.m_surfaceTexB = data.m_dataTexture1;
                                    combinedLod5.m_surfaceMapping = data.m_dataVector1;
                                }
                                combinedLod5.m_leftMatrices[combinedLod5.m_lodCount] = data.m_dataMatrix0;
                                combinedLod5.m_rightMatrices[combinedLod5.m_lodCount] = data.m_extraData.m_dataMatrix2;
                                combinedLod5.m_meshScales[combinedLod5.m_lodCount] = dataVector8;
                                combinedLod5.m_objectIndices[combinedLod5.m_lodCount] = dataVector7;
                                combinedLod5.m_meshLocations[combinedLod5.m_lodCount] = data.m_position;
                                combinedLod5.m_lodMin = Vector3.Min(combinedLod5.m_lodMin, data.m_position);
                                combinedLod5.m_lodMax = Vector3.Max(combinedLod5.m_lodMax, data.m_position);
                                if (++combinedLod5.m_lodCount == combinedLod5.m_leftMatrices.Length)
                                {
                                    NetSegment.RenderLod(cameraInfo, combinedLod5);
                                }
                            }
                        }
                    }
                IL_1637:;
                }
            }
        }
        instanceIndex = (uint)data.m_nextInstance;
    }

    public static void CalculateNode(ref NetNode This, ushort nodeID)
    {
        if (This.m_flags == NetNode.Flags.None)
        {
            return;
        }
        NetManager netMan = Singleton<NetManager>.instance;
        Vector3 DirFirst = Vector3.zero;
        int iSegment = 0;
        int ConnectCount = 0;
        bool hasSegments = false;
        bool canBeMiddle = false;
        bool bCompatibleButNodeMiddle = false;
        bool isAsymForward = false;
        bool isAsymBackward = false;
        bool needsJunctionFlag = false;
        bool hasCurvedSegment = false;
        bool hasStraightSegment = false;
        bool bCompatibleAndStart2End = false;
        bool allConnectedSegmentsAreFlat = true;
        bool CanModify = true;
        bool bHasDetailMapping = Singleton<TerrainManager>.instance.HasDetailMapping(This.m_position);
        NetInfo prevInfo = null;
        int prev_backwardVehicleLaneCount = 0;
        int prev_m_forwardVehicleLaneCount = 0;
        NetInfo infoNode = null;
        float num5 = -1E+07f;
        for (int i = 0; i < 8; i++)
        {
            ushort segmentID = This.GetSegment(i);
            if (segmentID != 0)
            {
                NetInfo infoSegment = netMan.m_segments.m_buffer[segmentID].Info;
                float nodeInfoPriority = infoSegment.m_netAI.GetNodeInfoPriority(segmentID, ref netMan.m_segments.m_buffer[segmentID]);
                if (nodeInfoPriority > num5)
                {
                    infoSegment = infoSegment;
                    num5 = nodeInfoPriority;
                }
            }
        }
        if (infoNode == null)
        {
            infoNode = This.Info;
        }
        if (infoNode != This.Info)
        {
            This.Info = infoNode;
            Singleton<NetManager>.instance.UpdateNodeColors(nodeID);
            if (!infoNode.m_canDisable)
            {
                This.m_flags &= ~NetNode.Flags.Disabled;
            }
        }
        bool bStartNodeFirst = false;
        for (int j = 0; j < 8; j++)
        {
            ushort segmentID = This.GetSegment(j);
            if (segmentID != 0)
            {
                iSegment++;
                ushort startNodeID = netMan.m_segments.m_buffer[segmentID].m_startNode;
                ushort endNodeID = netMan.m_segments.m_buffer[segmentID].m_endNode;
                Vector3 startDirection = netMan.m_segments.m_buffer[segmentID].m_startDirection;
                Vector3 endDirection = netMan.m_segments.m_buffer[segmentID].m_endDirection;
                bool bStartNode = nodeID == startNodeID;
                Vector3 currentDir = (!bStartNode) ? endDirection : startDirection;
                NetInfo infoSegment = netMan.m_segments.m_buffer[segmentID].Info;
                ItemClass connectionClass = infoSegment.GetConnectionClass();
                if (!infoSegment.m_netAI.CanModify())
                {
                    CanModify = false;
                }
                int backwardVehicleLaneCount;
                int forwardVehicleLaneCount;
                if (bStartNode == ((netMan.m_segments.m_buffer[segmentID].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None))
                {
                    backwardVehicleLaneCount = infoSegment.m_backwardVehicleLaneCount;
                    forwardVehicleLaneCount = infoSegment.m_forwardVehicleLaneCount;
                }
                else
                {
                    backwardVehicleLaneCount = infoSegment.m_forwardVehicleLaneCount;
                    forwardVehicleLaneCount = infoSegment.m_backwardVehicleLaneCount;
                }
                for (int k = j + 1; k < 8; k++)
                {
                    ushort segmentID2 = This.GetSegment(k);
                    if (segmentID2 != 0)
                    {
                        NetInfo infoSegment2 = netMan.m_segments.m_buffer[segmentID2].Info;
                        ItemClass connectionClass2 = infoSegment2.GetConnectionClass();
                        if (connectionClass2.m_service == connectionClass.m_service || (infoSegment2.m_nodeConnectGroups & infoSegment.m_connectGroup) != NetInfo.ConnectGroup.None || (infoSegment.m_nodeConnectGroups & infoSegment2.m_connectGroup) != NetInfo.ConnectGroup.None)
                        {
                            bool bStartNode2 = nodeID == netMan.m_segments.m_buffer[segmentID2].m_startNode;
                            Vector3 dir2 = (!bStartNode2) ? netMan.m_segments.m_buffer[segmentID2].m_endDirection : netMan.m_segments.m_buffer[segmentID2].m_startDirection;
                            float dot2 = currentDir.x * dir2.x + currentDir.z * dir2.z;
                            float turnThreshold = 0.01f - Mathf.Min(infoSegment.m_maxTurnAngleCos, infoSegment2.m_maxTurnAngleCos);
                            if (dot2 < turnThreshold)
                            {
                                if ((infoSegment.m_requireDirectRenderers && (infoSegment.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (infoSegment.m_nodeConnectGroups & infoSegment2.m_connectGroup) != NetInfo.ConnectGroup.None)) || (infoSegment2.m_requireDirectRenderers && (infoSegment2.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (infoSegment2.m_nodeConnectGroups & infoSegment.m_connectGroup) != NetInfo.ConnectGroup.None)))
                                {
                                    ConnectCount++;
                                }
                            }
                            else
                            {
                                needsJunctionFlag = true;
                            }
                        }
                        else
                        {
                            needsJunctionFlag = true;
                        }
                    }
                }

                if (netMan.m_nodes.m_buffer[startNodeID].m_elevation != netMan.m_nodes.m_buffer[endNodeID].m_elevation)
                    allConnectedSegmentsAreFlat = false;

                Vector3 startPos = netMan.m_nodes.m_buffer[startNodeID].m_position;
                Vector3 endPos = netMan.m_nodes.m_buffer[endNodeID].m_position;
                if (bStartNode)
                    bHasDetailMapping = (bHasDetailMapping && Singleton<TerrainManager>.instance.HasDetailMapping(endPos));
                else
                    bHasDetailMapping = (bHasDetailMapping && Singleton<TerrainManager>.instance.HasDetailMapping(startPos));

                if (NetSegment.IsStraight(startPos, startDirection, endPos, endDirection))
                {
                    hasStraightSegment = true;
                }
                else
                {
                    hasCurvedSegment = true;
                }

                if (iSegment == 1)
                {
                    bStartNodeFirst = bStartNode;
                    DirFirst = currentDir;
                    hasSegments = true;
                }
                else if (iSegment == 2 && infoSegment.IsCombatible(prevInfo) && infoSegment.IsCombatible(infoNode) && (backwardVehicleLaneCount != 0) == (prev_m_forwardVehicleLaneCount != 0) && (forwardVehicleLaneCount != 0) == (prev_backwardVehicleLaneCount != 0))
                {
                    float dot = DirFirst.x * currentDir.x + DirFirst.z * currentDir.z;
                    if (backwardVehicleLaneCount != prev_m_forwardVehicleLaneCount || forwardVehicleLaneCount != prev_backwardVehicleLaneCount)
                    {
                        if (backwardVehicleLaneCount > forwardVehicleLaneCount)
                        {
                            isAsymForward = true;
                        }
                        else
                        {
                            isAsymBackward = true;
                        }
                        bCompatibleButNodeMiddle = true;
                    }
                    else if (dot < -0.999f) // straight.
                    {
                        canBeMiddle = true;
                    }
                    else
                    {
                        bCompatibleButNodeMiddle = true;
                    }
                    bCompatibleAndStart2End = (bStartNode != bStartNodeFirst);
                }
                else
                {
                    needsJunctionFlag = true;
                }
                prevInfo = infoSegment;
                prev_backwardVehicleLaneCount = backwardVehicleLaneCount;
                prev_m_forwardVehicleLaneCount = forwardVehicleLaneCount;
            }
        }
        if (!infoNode.m_enableMiddleNodes && canBeMiddle)
        {
            bCompatibleButNodeMiddle = true;
        }
        if (!infoNode.m_enableBendingNodes && bCompatibleButNodeMiddle)
        {
            needsJunctionFlag = true;
        }
        if (infoNode.m_requireContinuous && (This.m_flags & NetNode.Flags.Untouchable) != NetNode.Flags.None)
        {
            needsJunctionFlag = true;
        }
        if (infoNode.m_requireContinuous && !bCompatibleAndStart2End && (canBeMiddle || bCompatibleButNodeMiddle))
        {
            needsJunctionFlag = true;
        }
        NetNode.Flags flags = This.m_flags & ~(NetNode.Flags.End | NetNode.Flags.Middle | NetNode.Flags.Bend | NetNode.Flags.Junction | NetNode.Flags.Moveable | NetNode.Flags.AsymForward | NetNode.Flags.AsymBackward);
        if ((flags & NetNode.Flags.Outside) != NetNode.Flags.None)
        {
            This.m_flags = flags;
        }
        else if (needsJunctionFlag)
        {
            This.m_flags = (flags | NetNode.Flags.Junction);
        }
        else if (bCompatibleButNodeMiddle)
        {
            if (isAsymForward)
            {
                flags |= NetNode.Flags.AsymForward;
            }
            if (isAsymBackward)
            {
                flags |= NetNode.Flags.AsymBackward;
            }
            This.m_flags = (flags | NetNode.Flags.Bend);
        }
        else if (canBeMiddle)
        {
            if ((!hasCurvedSegment || !hasStraightSegment) && (This.m_flags & (NetNode.Flags.Untouchable | NetNode.Flags.Double)) == NetNode.Flags.None && allConnectedSegmentsAreFlat && CanModify)
            {
                flags |= NetNode.Flags.Moveable;
            }
            This.m_flags = (flags | NetNode.Flags.Middle);
        }
        else if (hasSegments)
        {
            if ((This.m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None && allConnectedSegmentsAreFlat && CanModify && infoNode.m_enableMiddleNodes)
            {
                flags |= NetNode.Flags.Moveable;
            }
            This.m_flags = (flags | NetNode.Flags.End);
        }
        else
        {
            This.m_flags = flags;
        }
        This.m_heightOffset = (byte)((!bHasDetailMapping && infoNode.m_requireSurfaceMaps) ? 64 : 0);
        This.m_connectCount = (byte)ConnectCount;
        BuildingInfo newBuilding;
        float heightOffset;
        infoNode.m_netAI.GetNodeBuilding(nodeID, ref This, out newBuilding, out heightOffset);
        This.UpdateBuilding(nodeID, newBuilding, heightOffset);
    }

    public static bool BlendJunction(ushort nodeID)
    {
        NetManager netManager = Singleton<NetManager>.instance;
        if ((netManager.m_nodes.m_buffer[(int)nodeID].m_flags & (NetNode.Flags.Middle | NetNode.Flags.Bend)) != NetNode.Flags.None)
        {
            return true;
        }
        if ((netManager.m_nodes.m_buffer[(int)nodeID].m_flags & NetNode.Flags.Junction) != NetNode.Flags.None)
        {
            bool bHasForward_Prev = false;
            bool bHasBackward_Prev = false;
            int segmentCount = 0;
            for (int i = 0; i < 8; i++)
            {
                ushort segmentID = nodeID.ToNode().GetSegment(i);
                if (segmentID != 0)
                {
                    if (++segmentCount >= 3)
                    {
                        return false;
                    }
                    NetInfo info_segment = segmentID.ToSegment().Info;
                    if (!info_segment.m_enableMiddleNodes || info_segment.m_requireContinuous)
                    {
                        return false;
                    }
                    bool bHasForward;
                    bool bHasBackward;
                    bool bStartNode = segmentID.ToSegment().m_startNode == nodeID;
                    bool bInvert = !segmentID.ToSegment().m_flags.IsFlagSet(NetSegment.Flags.Invert);
                    if (bStartNode == bInvert)
                    {
                        bHasForward = info_segment.m_hasForwardVehicleLanes;
                        bHasBackward = info_segment.m_hasBackwardVehicleLanes;
                    }
                    else
                    {
                        bHasForward = info_segment.m_hasBackwardVehicleLanes;
                        bHasBackward = info_segment.m_hasForwardVehicleLanes;
                    }
                    if (segmentCount == 2)
                    {
                        if (bHasForward != bHasBackward_Prev || bHasBackward != bHasForward_Prev)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        bHasForward_Prev = bHasForward;
                        bHasBackward_Prev = bHasBackward;
                    }
                }
            }
            return segmentCount == 2;
        }
        return false;
    }

    // NetNode
    // Token: 0x060034C6 RID: 13510 RVA: 0x0023D1EC File Offset: 0x0023B5EC

    /// <param name="centerPos">position between left corner and right corner of segmentID (or something like that).</param>
    private static void RefreshJunctionData(ref NetNode This, ushort nodeID, int segmentIndex, ushort SegmentID, Vector3 centerPos, ref uint instanceIndex, ref RenderManager.Instance data)
    {
        Vector3 cornerPos_right = Vector3.zero, cornerDir_right = Vector3.zero, cornerPos_left = Vector3.zero, cornerDir_left = Vector3.zero,
            cornerPosA_right = Vector3.zero, cornerDirA_right = Vector3.zero, cornerPosA_left = Vector3.zero, cornerDirA_left = Vector3.zero,
            cornerPosB_right = Vector3.zero, cornerDirB_right = Vector3.zero, cornerPosB_left = Vector3.zero, cornerDirB_left = Vector3.zero;

        NetManager instance = Singleton<NetManager>.instance;
        data.m_position = This.m_position;
        data.m_rotation = Quaternion.identity;
        data.m_initialized = true;
        NetSegment segment = SegmentID.ToSegment();
        NetInfo info = segment.Info;
        float vscale = info.m_netAI.GetVScale();
        ItemClass connectionClass = info.GetConnectionClass();
        bool bStartNode = nodeID == segment.m_startNode;
        Vector3 dir = !bStartNode ? segment.m_endDirection : segment.m_startDirection;
        float dot_A = -4f;
        float dot_B = -4f;
        ushort segmentID_A = 0;
        ushort segmentID_B = 0;
        for (int i = 0; i < 8; i++)
        {
            ushort segmentID2 = This.GetSegment(i);
            if (segmentID2 != 0 && segmentID2 != SegmentID)
            {
                NetInfo info2 = instance.m_segments.m_buffer[(int)segmentID2].Info;
                ItemClass connectionClass2 = info2.GetConnectionClass();
                if (connectionClass.m_service == connectionClass2.m_service)
                {
                    NetSegment segment2 = segmentID2.ToSegment();
                    bool bStartNode2 = nodeID != segment2.m_startNode;
                    Vector3 dir2 = !bStartNode2 ? segment2.m_endDirection : segment2.m_startDirection;
                    float dot = dir.x * dir2.x + dir.z * dir2.z;
                    float determinent = dir2.z * dir.x - dir2.x * dir.z;
                    bool bRight = determinent > 0;
                    bool bWide = dot < 0;
                    // 180 -> det=0 dot=-1
                    if (!bRight)
                    {
                        if (dot > dot_A) // most accute
                        {
                            dot_A = dot;
                            segmentID_A = segmentID2;
                        }
                        dot = -2f - dot;
                        if (dot > dot_B) // widest
                        {
                            dot_B = dot;
                            segmentID_B = segmentID2;
                        }
                    }
                    else
                    {
                        if (dot > dot_B) // most accute
                        {
                            dot_B = dot;
                            segmentID_B = segmentID2;
                        }
                        dot = -2f - dot;
                        if (dot > dot_A) // widest
                        {
                            dot_A = dot;
                            segmentID_A = segmentID2;
                        }
                    }
                }
            }
        }
        segment.CalculateCorner(SegmentID, true, bStartNode, false, out cornerPos_right, out cornerDir_right, out _);
        segment.CalculateCorner(SegmentID, true, bStartNode, true, out cornerPos_left, out cornerDir_left, out _);
        if (segmentID_A != 0 && segmentID_B != 0)
        {
            float pavementRatio_avgA = info.m_pavementWidth / info.m_halfWidth * 0.5f;
            float averageWidthA = 1f;
            if (segmentID_A != 0)
            {
                NetSegment segment_A = instance.m_segments.m_buffer[(int)segmentID_A];
                NetInfo infoA = segment_A.Info;
                bStartNode = (segment_A.m_startNode == nodeID);
                segment_A.CalculateCorner(segmentID_A, true, bStartNode, true, out cornerPosA_right, out cornerDirA_right, out _);
                segment_A.CalculateCorner(segmentID_A, true, bStartNode, false, out cornerPosA_left, out cornerDirA_left, out _);
                float pavementRatioA = infoA.m_pavementWidth / infoA.m_halfWidth * 0.5f;
                pavementRatio_avgA = (pavementRatio_avgA + pavementRatioA) * 0.5f;
                averageWidthA = 2f * info.m_halfWidth / (info.m_halfWidth + infoA.m_halfWidth);
            }
            float pavementRatio_avgB = info.m_pavementWidth / info.m_halfWidth * 0.5f;
            float averageWithB = 1f;
            if (segmentID_B != 0)
            {
                NetSegment segment_B = instance.m_segments.m_buffer[(int)segmentID_B];
                NetInfo infoB = segment_B.Info;
                bStartNode = (segment_B.m_startNode == nodeID);
                segment_B.CalculateCorner(segmentID_B, true, bStartNode, true, out cornerPosB_right, out cornerDirB_right, out _);
                segment_B.CalculateCorner(segmentID_B, true, bStartNode, false, out cornerPosB_left, out cornerDirB_left, out _);
                float pavementRatioB = infoB.m_pavementWidth / infoB.m_halfWidth * 0.5f;
                pavementRatio_avgB = (pavementRatio_avgB + pavementRatioB) * 0.5f;
                averageWithB = 2f * info.m_halfWidth / (info.m_halfWidth + infoB.m_halfWidth);
            }

            Bezier3 bezierA_right = new Bezier3
            {
                a = cornerPos_right,
                d = cornerPosA_right,
            };

            NetSegment.CalculateMiddlePoints(bezierA_right.a, -cornerDir_right, bezierA_right.d, -cornerDirA_right, true, true, out bezierA_right.b, out bezierA_right.c);
            NetSegment.CalculateMiddlePoints(cornerPos_left, -cornerDir_left, cornerPosA_left, -cornerDirA_left, true, true, out var cpoint2_Aleft, out var cpoint3_Aleft);
            NetSegment.CalculateMiddlePoints(cornerPos_right, -cornerDir_right, cornerPosB_right, -cornerDirB_right, true, true, out var cpoint2_Bright, out var cpoint3_Bright);
            NetSegment.CalculateMiddlePoints(cornerPos_left, -cornerDir_left, cornerPosB_left, -cornerDirB_left, true, true, out var cpoint2_Bleft, out var cpoint3_Bleft);

            data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(bezierA_right.a, bezierA_right.b, bezierA_right.c, bezierA_right.d, bezierA_right.a, bezierA_right.b, bezierA_right.c, bezierA_right.d, This.m_position, vscale);
            data.m_extraData.m_dataMatrix2 = NetSegment.CalculateControlMatrix(cornerPos_left, cpoint2_Aleft, cpoint3_Aleft, cornerPosA_left, cornerPos_left, cpoint2_Aleft, cpoint3_Aleft, cornerPosA_left, This.m_position, vscale);
            data.m_extraData.m_dataMatrix3 = NetSegment.CalculateControlMatrix(cornerPos_right, cpoint2_Bright, cpoint3_Bright, cornerPosB_right, cornerPos_right, cpoint2_Bright, cpoint3_Bright, cornerPosB_right, This.m_position, vscale);
            data.m_dataMatrix1 = NetSegment.CalculateControlMatrix(cornerPos_left, cpoint2_Bleft, cpoint3_Bleft, cornerPosB_left, cornerPos_left, cpoint2_Bleft, cpoint3_Bleft, cornerPosB_left, This.m_position, vscale);

            // Vector4(1/width | 1/length | 0.5 - pavement/width | pavement/width )
            data.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 0.5f - info.m_pavementWidth / info.m_halfWidth * 0.5f, info.m_pavementWidth / info.m_halfWidth * 0.5f);
            data.m_dataVector1 = centerPos - data.m_position;
            data.m_dataVector1.w = (data.m_dataMatrix0.m31 + data.m_dataMatrix0.m32 + data.m_extraData.m_dataMatrix2.m31 + data.m_extraData.m_dataMatrix2.m32 + data.m_extraData.m_dataMatrix3.m31 + data.m_extraData.m_dataMatrix3.m32 + data.m_dataMatrix1.m31 + data.m_dataMatrix1.m32) * 0.125f;
            data.m_dataVector2 = new Vector4(pavementRatio_avgA, averageWidthA, pavementRatio_avgB, averageWithB);
        }
        else
        {
            centerPos.x = (cornerPos_right.x + cornerPos_left.x) * 0.5f;
            centerPos.z = (cornerPos_right.z + cornerPos_left.z) * 0.5f;
            var cornerPos_left_prev = cornerPos_left;
            var cornerPos_right_prev = cornerPos_right;
            cornerDirB_right = cornerDir_left;
            cornerDirB_left = cornerDir_right;
            float d = info.m_netAI.GetEndRadius() * 1.33333337f;
            Vector3 vector13 = cornerPos_right - cornerDir_right * d;
            Vector3 vector14 = cornerPos_left_prev - cornerDirB_right * d;
            Vector3 vector15 = cornerPos_left - cornerDir_left * d;
            Vector3 vector16 = cornerPos_right_prev - cornerDirB_left * d;
            Vector3 vector17 = cornerPos_right + cornerDir_right * d;
            Vector3 vector18 = cornerPos_left_prev + cornerDirB_right * d;
            Vector3 vector19 = cornerPos_left + cornerDir_left * d;
            Vector3 vector20 = cornerPos_right_prev + cornerDirB_left * d;
            data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(cornerPos_right, vector13, vector14, cornerPos_left_prev, cornerPos_right, vector13, vector14, cornerPos_left_prev, This.m_position, vscale);
            data.m_extraData.m_dataMatrix2 = NetSegment.CalculateControlMatrix(cornerPos_left, vector19, vector20, cornerPos_right_prev, cornerPos_left, vector19, vector20, cornerPos_right_prev, This.m_position, vscale);
            data.m_extraData.m_dataMatrix3 = NetSegment.CalculateControlMatrix(cornerPos_right, vector17, vector18, cornerPos_left_prev, cornerPos_right, vector17, vector18, cornerPos_left_prev, This.m_position, vscale);
            data.m_dataMatrix1 = NetSegment.CalculateControlMatrix(cornerPos_left, vector15, vector16, cornerPos_right_prev, cornerPos_left, vector15, vector16, cornerPos_right_prev, This.m_position, vscale);
            data.m_dataMatrix0.SetRow(3, data.m_dataMatrix0.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
            data.m_extraData.m_dataMatrix2.SetRow(3, data.m_extraData.m_dataMatrix2.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
            data.m_extraData.m_dataMatrix3.SetRow(3, data.m_extraData.m_dataMatrix3.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
            data.m_dataMatrix1.SetRow(3, data.m_dataMatrix1.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
            data.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 0.5f - info.m_pavementWidth / info.m_halfWidth * 0.5f, info.m_pavementWidth / info.m_halfWidth * 0.5f);
            data.m_dataVector1 = centerPos - data.m_position;
            data.m_dataVector1.w = (data.m_dataMatrix0.m31 + data.m_dataMatrix0.m32 + data.m_extraData.m_dataMatrix2.m31 + data.m_extraData.m_dataMatrix2.m32 + data.m_extraData.m_dataMatrix3.m31 + data.m_extraData.m_dataMatrix3.m32 + data.m_dataMatrix1.m31 + data.m_dataMatrix1.m32) * 0.125f;
            data.m_dataVector2 = new Vector4(info.m_pavementWidth / info.m_halfWidth * 0.5f, 1f, info.m_pavementWidth / info.m_halfWidth * 0.5f, 1f);
        }
        Vector4 colorLocation;
        Vector4 vector21;
        if (NetNode.BlendJunction(nodeID))
        {
            colorLocation = RenderManager.GetColorLocation(86016u + (uint)nodeID);
            vector21 = colorLocation;
        }
        else
        {
            colorLocation = RenderManager.GetColorLocation((uint)(49152 + SegmentID));
            vector21 = RenderManager.GetColorLocation(86016u + (uint)nodeID);
        }
        data.m_extraData.m_dataVector4 = new Vector4(colorLocation.x, colorLocation.y, vector21.x, vector21.y);
        data.m_dataInt0 = segmentIndex;
        data.m_dataColor0 = info.m_color;
        data.m_dataColor0.a = 0f;
        data.m_dataFloat0 = Singleton<WeatherManager>.instance.GetWindSpeed(data.m_position);
        if (info.m_requireSurfaceMaps)
        {
            Singleton<TerrainManager>.instance.GetSurfaceMapping(data.m_position, out data.m_dataTexture0, out data.m_dataTexture1, out data.m_dataVector3);
        }
        instanceIndex = (uint)data.m_nextInstance;
    }

    Vector3 m_position;

    // NetNode
    // Token: 0x060034C5 RID: 13509 RVA: 0x0023CECC File Offset: 0x0023B2CC
    private void RefreshJunctionData(ushort nodeID, int segmentIndex, int segmentIndex2, NetInfo info, NetInfo info2, ushort segmentID, ushort segmentID2, ref uint instanceIndex, ref RenderManager.Instance data)
    {
        data.m_position = this.m_position;
        data.m_rotation = Quaternion.identity;
        data.m_initialized = true;
        float vscale = info.m_netAI.GetVScale();
        Vector3 CornerPos2L = Vector3.zero;
        Vector3 CornerPos2R = Vector3.zero;
        Vector3 CornerDir2L = Vector3.zero;
        Vector3 CornerDir2R = Vector3.zero;
        bool startNode = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID].m_startNode == nodeID;
        Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID].CalculateCorner(segmentID, true, startNode, false, out var CornerPosL, out var CornerDirL, out _);
        Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID].CalculateCorner(segmentID, true, startNode, true, out var CornerPosR, out var CornerDirR, out _);
        bool startNode2 = (Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID2].m_startNode == nodeID);
        Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID2].CalculateCorner(segmentID2, true, startNode2, true, out CornerPos2L, out CornerDir2L, out _);
        Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID2].CalculateCorner(segmentID2, true, startNode2, false, out CornerPos2R, out CornerDir2R, out _);
        Vector3 b = (CornerPos2R - CornerPos2L) * (info.m_halfWidth / info2.m_halfWidth * 0.5f - 0.5f);
        CornerPos2L -= b;
        CornerPos2R += b;
        NetSegment.CalculateMiddlePoints(CornerPosL, -CornerDirL, CornerPos2L, -CornerDir2L, true, true, out var bpointL, out var cpointL);
        NetSegment.CalculateMiddlePoints(CornerPosR, -CornerDirR, CornerPos2R, -CornerDir2R, true, true, out var bpointR, out var cpointR);
        data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(CornerPosL, bpointL, cpointL, CornerPos2L, CornerPosR, bpointR, cpointR, CornerPos2R, this.m_position, vscale);
        data.m_extraData.m_dataMatrix2 = NetSegment.CalculateControlMatrix(CornerPosR, bpointR, cpointR, CornerPos2R, CornerPosL, bpointL, cpointL, CornerPos2L, this.m_position, vscale);
        data.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);
        Vector4 colorLocation;
        Vector4 vector7;
        if (NetNode.BlendJunction(nodeID))
        {
            colorLocation = RenderManager.GetColorLocation(86016u + (uint)nodeID);
            vector7 = colorLocation;
        }
        else
        {
            colorLocation = RenderManager.GetColorLocation((uint)(49152 + segmentID));
            vector7 = RenderManager.GetColorLocation((uint)(49152 + segmentID2));
        }
        data.m_dataVector3 = new Vector4(colorLocation.x, colorLocation.y, vector7.x, vector7.y);
        data.m_dataInt0 = (8 | segmentIndex | segmentIndex2 << 4);
        data.m_dataColor0 = info.m_color;
        data.m_dataColor0.a = 0f;
        data.m_dataFloat0 = Singleton<WeatherManager>.instance.GetWindSpeed(data.m_position);
        if (info.m_requireSurfaceMaps)
        {
            Singleton<TerrainManager>.instance.GetSurfaceMapping(data.m_position, out data.m_dataTexture0, out data.m_dataTexture1, out data.m_dataVector1);
        }
        instanceIndex = (uint)data.m_nextInstance;
    }

}
