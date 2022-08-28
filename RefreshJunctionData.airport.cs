private void RefreshJunctionData(ushort nodeID, int segmentIndex, ushort nodeSegment, Vector3 centerPos, ref uint instanceIndex, ref RenderManager.Instance data) {
    NetManager instance = Singleton<NetManager>.instance;
    data.m_position = m_position;
    data.m_rotation = Quaternion.identity;
    data.m_initialized = true;
    Vector3 cornerPos_right = Vector3.zero;
    Vector3 cornerPos_left = Vector3.zero;
    Vector3 cornerDir_right = Vector3.zero;
    Vector3 cornerDir_left = Vector3.zero;
    Vector3 cornerPosA_right = Vector3.zero;
    Vector3 cornerPosA_left = Vector3.zero;
    Vector3 cornerDirA_right = Vector3.zero;
    Vector3 cornerDirA_left = Vector3.zero;
    Vector3 cornerPos5 = Vector3.zero;
    Vector3 cornerPos6 = Vector3.zero;
    Vector3 cornerDirection5 = Vector3.zero;
    Vector3 cornerDirection6 = Vector3.zero;
    NetSegment segment = instance.m_segments.m_buffer[nodeSegment];
    NetInfo info = segment.Info;
    float vScale = info.m_netAI.GetVScale();
    ItemClass connectionClass = info.GetConnectionClass();
    Vector3 dir = ((nodeID != segment.m_startNode) ? segment.m_endDirection : segment.m_startDirection);
    float dot_A = -4f;
    float dot_B = -4f;
    ushort segmentID_A = 0;
    ushort segmentID_B = 0;
    for (int i = 0; i < 8; i++) {
        ushort segmentID2 = GetSegment(i);
        if (segmentID2 == 0 || segmentID2 == nodeSegment) {
            continue;
        }
        NetInfo info2 = instance.m_segments.m_buffer[segmentID2].Info;
        ItemClass connectionClass2 = info2.GetConnectionClass();
        if (connectionClass.m_service != connectionClass2.m_service) {
            continue;
        }
        NetSegment segment2 = instance.m_segments.m_buffer[segmentID2];
        Vector3 dir2 = ((nodeID != segment2.m_startNode) ? segment2.m_endDirection : segment2.m_startDirection);
        float dot = dir.x * dir2.x + dir.z * dir2.z;
        if (dir2.z * dir.x - dir2.x * dir.z < 0f) {
            if (dot > dot_A) {
                dot_A = dot;
                segmentID_A = segmentID2;
            }
            dot = -2f - dot;
            if (dot > dot_B) {
                dot_B = dot;
                segmentID_B = segmentID2;
            }
        } else {
            if (dot > dot_B) {
                dot_B = dot;
                segmentID_B = segmentID2;
            }
            dot = -2f - dot;
            if (dot > dot_A) {
                dot_A = dot;
                segmentID_A = segmentID2;
            }
        }
    }
    bool bStartNode = segment.m_startNode == nodeID;
    segment.CalculateCorner(nodeSegment, heightOffset: true, bStartNode, leftSide: false, out cornerPos_right, out cornerDir_right, out var smooth);
    segment.CalculateCorner(nodeSegment, heightOffset: true, bStartNode, leftSide: true, out cornerPos_left, out cornerDir_left, out smooth);
    if (segmentID_A != 0 && segmentID_B != 0) {
        float pavementRatio_avgA = info.m_pavementWidth / info.m_halfWidth * 0.5f;
        float widthRatioA = 1f;
        if (segmentID_A != 0) {
            NetSegment segment_A = instance.m_segments.m_buffer[segmentID_A];
            NetInfo infoA = segment_A.Info;
            bStartNode = segment_A.m_startNode == nodeID;
            segment_A.CalculateCorner(segmentID_A, heightOffset: true, bStartNode, leftSide: true, out cornerPosA_right, out cornerDirA_right, out smooth);
            segment_A.CalculateCorner(segmentID_A, heightOffset: true, bStartNode, leftSide: false, out cornerPosA_left, out cornerDirA_left, out smooth);
            float pavementRatioA = infoA.m_pavementWidth / infoA.m_halfWidth * 0.5f;
            pavementRatio_avgA = (pavementRatio_avgA + pavementRatioA) * 0.5f;
            widthRatioA = 2f * info.m_halfWidth / (info.m_halfWidth + infoA.m_halfWidth);
        }
        float pavementRatio_avgB = info.m_pavementWidth / info.m_halfWidth * 0.5f;
        float widthRatioB = 1f;
        if (segmentID_B != 0) {
            NetSegment segment_B = instance.m_segments.m_buffer[segmentID_B];
            NetInfo infoB = segment_B.Info;
            bStartNode = segment_B.m_startNode == nodeID;
            segment_B.CalculateCorner(segmentID_B, heightOffset: true, bStartNode, leftSide: true, out cornerPos5, out cornerDirection5, out smooth);
            segment_B.CalculateCorner(segmentID_B, heightOffset: true, bStartNode, leftSide: false, out cornerPos6, out cornerDirection6, out smooth);
            float pavementRatioB = infoB.m_pavementWidth / infoB.m_halfWidth * 0.5f;
            pavementRatio_avgB = (pavementRatio_avgB + pavementRatioB) * 0.5f;
            widthRatioB = 2f * info.m_halfWidth / (info.m_halfWidth + infoB.m_halfWidth);
        }
        NetSegment.CalculateMiddlePoints(cornerPos_right, -cornerDir_right, cornerPosA_right, -cornerDirA_right, smoothStart: true, smoothEnd: true, out var bpointA_right, out var cpointA_right);
        NetSegment.CalculateMiddlePoints(cornerPos_left, -cornerDir_left, cornerPosA_left, -cornerDirA_left, smoothStart: true, smoothEnd: true, out var bpoint_Aleft, out var cpoint_Aleft);
        NetSegment.CalculateMiddlePoints(cornerPos_right, -cornerDir_right, cornerPos5, -cornerDirection5, smoothStart: true, smoothEnd: true, out var bpoint_Bright, out var cpoint_Bright);
        NetSegment.CalculateMiddlePoints(cornerPos_left, -cornerDir_left, cornerPos6, -cornerDirection6, smoothStart: true, smoothEnd: true, out var bpoint_Bleft, out var cpoint_Bleft);
        data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(cornerPos_right, bpointA_right, cpointA_right, cornerPosA_right, cornerPos_right, bpointA_right, cpointA_right, cornerPosA_right, m_position, vScale);
        data.m_extraData.m_dataMatrix2 = NetSegment.CalculateControlMatrix(cornerPos_left, bpoint_Aleft, cpoint_Aleft, cornerPosA_left, cornerPos_left, bpoint_Aleft, cpoint_Aleft, cornerPosA_left, m_position, vScale);
        data.m_extraData.m_dataMatrix3 = NetSegment.CalculateControlMatrix(cornerPos_right, bpoint_Bright, cpoint_Bright, cornerPos5, cornerPos_right, bpoint_Bright, cpoint_Bright, cornerPos5, m_position, vScale);
        data.m_dataMatrix1 = NetSegment.CalculateControlMatrix(cornerPos_left, bpoint_Bleft, cpoint_Bleft, cornerPos6, cornerPos_left, bpoint_Bleft, cpoint_Bleft, cornerPos6, m_position, vScale);
        data.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 0.5f - info.m_pavementWidth / info.m_halfWidth * 0.5f, info.m_pavementWidth / info.m_halfWidth * 0.5f);
        data.m_dataVector1 = centerPos - data.m_position;
        data.m_dataVector1.w = (data.m_dataMatrix0.m31 + data.m_dataMatrix0.m32 + data.m_extraData.m_dataMatrix2.m31 + data.m_extraData.m_dataMatrix2.m32 + data.m_extraData.m_dataMatrix3.m31 + data.m_extraData.m_dataMatrix3.m32 + data.m_dataMatrix1.m31 + data.m_dataMatrix1.m32) * 0.125f;
        data.m_dataVector2 = new Vector4(pavementRatio_avgA, widthRatioA, pavementRatio_avgB, widthRatioB);
    } else {
        centerPos.x = (cornerPos_right.x + cornerPos_left.x) * 0.5f;
        centerPos.z = (cornerPos_right.z + cornerPos_left.z) * 0.5f;
        cornerPosA_right = cornerPos_left;
        cornerPosA_left = cornerPos_right;
        cornerDirA_right = cornerDir_left;
        cornerDirA_left = cornerDir_right;
        float endRaduis = info.m_netAI.GetEndRadius() * 1.33333337f;
        Vector3 vector3 = cornerPos_right - cornerDir_right * endRaduis;
        Vector3 vector4 = cornerPosA_right - cornerDirA_right * endRaduis;
        Vector3 vector5 = cornerPos_left - cornerDir_left * endRaduis;
        Vector3 vector6 = cornerPosA_left - cornerDirA_left * endRaduis;
        Vector3 vector7 = cornerPos_right + cornerDir_right * endRaduis;
        Vector3 vector8 = cornerPosA_right + cornerDirA_right * endRaduis;
        Vector3 vector9 = cornerPos_left + cornerDir_left * endRaduis;
        Vector3 vector10 = cornerPosA_left + cornerDirA_left * endRaduis;
        data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(cornerPos_right, vector3, vector4, cornerPosA_right, cornerPos_right, vector3, vector4, cornerPosA_right, m_position, vScale);
        data.m_extraData.m_dataMatrix2 = NetSegment.CalculateControlMatrix(cornerPos_left, vector9, vector10, cornerPosA_left, cornerPos_left, vector9, vector10, cornerPosA_left, m_position, vScale);
        data.m_extraData.m_dataMatrix3 = NetSegment.CalculateControlMatrix(cornerPos_right, vector7, vector8, cornerPosA_right, cornerPos_right, vector7, vector8, cornerPosA_right, m_position, vScale);
        data.m_dataMatrix1 = NetSegment.CalculateControlMatrix(cornerPos_left, vector5, vector6, cornerPosA_left, cornerPos_left, vector5, vector6, cornerPosA_left, m_position, vScale);
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
    Vector4 vector11;
    if (BlendJunction(nodeID)) {
        colorLocation = RenderManager.GetColorLocation((uint)(86016 + nodeID));
        vector11 = colorLocation;
    } else {
        colorLocation = RenderManager.GetColorLocation((uint)(49152 + nodeSegment));
        vector11 = RenderManager.GetColorLocation((uint)(86016 + nodeID));
    }
    data.m_extraData.m_dataVector4 = new Vector4(colorLocation.x, colorLocation.y, vector11.x, vector11.y);
    data.m_dataInt0 = segmentIndex;
    data.m_dataColor0 = info.m_color;
    data.m_dataColor0.a = 0f;
    data.m_dataFloat0 = Singleton<WeatherManager>.instance.GetWindSpeed(data.m_position);
    if (info.m_requireSurfaceMaps) {
        Singleton<TerrainManager>.instance.GetSurfaceMapping(data.m_position, out data.m_dataTexture0, out data.m_dataTexture1, out data.m_dataVector3);
    }
    instanceIndex = data.m_nextInstance;
}