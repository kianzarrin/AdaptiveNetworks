using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using UnityEngine;

namespace AdaptiveRoads.UI.ControlPanel {
    public class InterAvtiveButton : UIButton {
        public override void Awake() {
            base.Awake();

            //text = "some text\nnewline";
            //tooltip = "some tooltip";
            // Set the button dimensions.
            width = 500;
            height = 30;

            //autoSize = true;
            textPadding = new RectOffset(10, 10, 5, 5);
            textHorizontalAlignment = UIHorizontalAlignment.Left;
        }
        public override void Start() {
            base.Start();
            Log.Debug("InterAvtiveButton.Start");

            // Style the button to look like a menu
            atlas = TextureUtil.GetAtlas("Ingame");
            normalBgSprite = disabledBgSprite = focusedBgSprite = "ButtonSmall";
            hoveredBgSprite = "ButtonSmallHovered";
            pressedBgSprite = "ButtonSmallPressed";
            //textColor = Color.white;
            //disabledTextColor = new Color32(7, 7, 7, 255);
            //hoveredTextColor = new Color32(7, 132, 255, 255);
            //focusedTextColor = new Color32(255, 255, 255, 255);
            //pressedTextColor = new Color32(30, 30, 44, 255);

            // Enable button sounds.
            playAudioEvents = true;
        }

        public bool IsHovered => this.m_IsMouseHovering;

        private InstanceID _instanceID;

        public InstanceID InstanceID {
            get => _instanceID;
            set {
                _instanceID = value;
                if (value.Type == InstanceType.NetLane)
                    LaneData = NetUtil.GetLaneData(value.NetLane);
                else
                    LaneData = default;
            }
        }

        public LaneData LaneData { get; private set; } //optional. only valid for lanes.

        public virtual void RenderOverlay(RenderManager.CameraInfo cameraInfo, bool alphaBlend = false) {
            if (InstanceID.IsEmpty)
                return;
            switch (InstanceID.Type) {
                case InstanceType.NetLane:
                    RenderUtil.RenderLaneOverlay(cameraInfo, LaneData, Color.yellow, alphaBlend);
                    break;
                case InstanceType.NetSegment:
                    RenderUtil.RenderSegmnetOverlay(cameraInfo, InstanceID.NetSegment, Color.cyan, alphaBlend);
                    break;
                case InstanceType.NetNode:
                    RenderUtil.DrawNodeCircle(cameraInfo, Color.blue, InstanceID.NetNode, alphaBlend);
                    break;
                default:
                    Log.Error("Unexpected InstanceID.Type: "+ InstanceID.Type);
                    return;
            }
        }

        public string GetDetails() {
            if (InstanceID.IsEmpty)
                return "Please Hover/Select a network";
#pragma warning disable
            return InstanceID.Type switch
            {
                InstanceType.NetNode => "node flags: " + InstanceID.NetNode.ToNode().m_flags,
                InstanceType.NetSegment => "segment flags: " + InstanceID.NetSegment.ToSegment().m_flags,
                InstanceType.NetLane =>
                    "lane flags: " + LaneData.Flags + "\n" +
                    "lane types: " + LaneData.LaneInfo.m_laneType + "\n" +
                    "vehicle types: " + LaneData.LaneInfo.m_vehicleType + "\n" +
                    "direction: " + LaneData.LaneInfo.m_direction + "\n" +
                    "final direction: " + LaneData.LaneInfo.m_finalDirection + "\n" +
                    "start node:" + LaneData.StartNode + "\n",
                _ => "Unexpected InstanceID.Type: " + InstanceID.Type,
            };
#pragma warning enable

        }

        protected override void OnClick(UIMouseEventParameter p) {
            base.OnClick(p);
            Log.Debug("InterAvtiveButton.OnClick");
            if (InstanceID.Type != InstanceType.NetLane)
                MainPanel.Instance.Display(this.InstanceID);
        }

        protected override void OnMouseEnter(UIMouseEventParameter p) {
            base.OnMouseEnter(p);
            Log.Debug("InterAvtiveButton.OnMouseEnter");
            MainPanel.Instance.UpdateDetails(this);
        }

        protected override void OnMouseLeave(UIMouseEventParameter p) {
            base.OnMouseLeave(p);
            Log.Debug("InterAvtiveButton.OnMouseLeave");
            MainPanel.Instance.UpdateDetails(MainPanel.Instance.Title); // default
        }

    }
}
