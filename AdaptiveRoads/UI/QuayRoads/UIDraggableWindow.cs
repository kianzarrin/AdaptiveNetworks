using ColossalFramework;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static KianCommons.ReflectionHelpers;

// TODO: move to KianCommons; proper close button

namespace AdaptiveRoads.UI.QuayRoads {
    // based on FlagsPanel
    abstract class UIDraggableWindow<T> : UIPanel where T : UIDraggableWindow<T> {
        public string AtlasName => $"{GetType().FullName}_rev" + this.VersionOf();
        public abstract SavedFloat SavedX { get; }
        public abstract SavedFloat SavedY { get; }

        public string Caption {
            get => lblCaption_.text;
            set => lblCaption_.text = value;
        }
        private UILabel lblCaption_;
        private UIDragHandle dragHandle_;
        private UIButton closeButton_;
        public static T Instance = null;

        public bool IsOpen => Instance is not null;

        public static T Create() {
            var wrapper = UIView.GetAView().AddUIComponent<UIPanel>();
            return wrapper.AddUIComponent<T>() as T;
        }

        public static T GetOrOpen() {
            if (Instance is null) {
                Instance = Create();
            }
            return Instance;
        }

        public virtual void Close() {
            if (Instance == this) {
                Instance = null;
            }
            DestroyImmediate(gameObject);
            if (parent is not null) {
                DestroyImmediate(parent.gameObject);
            }
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public override void Awake() {
            try {
                base.Awake();
                LogCalled();
                name = "UIDraggableWindow";

                backgroundSprite = "MenuPanel2";
                atlas = TextureUtil.Ingame;

                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Vertical;
                autoFitChildrenHorizontally = true;
                autoFitChildrenVertically = true;
                autoLayoutPadding = new RectOffset(3, 3, 3, 3);
                padding = new RectOffset(3, 3, 3, 3);
            }
            catch (Exception ex) {
                ex.Log();
            }

        }

        public override void Start() {
            try {
                base.Start();
                LogCalled();

                absolutePosition = new Vector3(SavedX, SavedY);

                {
                    dragHandle_ = AddUIComponent<UIDragHandle>();
                    dragHandle_.height = 20;
                    dragHandle_.relativePosition = Vector3.zero;
                    dragHandle_.target = parent;

                    lblCaption_ = dragHandle_.AddUIComponent<UILabel>();
                    lblCaption_.text = "UIDraggableWindow";
                    lblCaption_.name = "UIDraggableWindow_caption";

                    closeButton_ = AddUIComponent<UIButtonExt>();
                    closeButton_.eventClicked += (_,_) => { Close(); };
                    closeButton_.text = "close";
                }

                isVisible = true;
                Refresh();
            }
            catch (Exception ex) { ex.Log(); }
        }
        void Refresh() {
            dragHandle_.FitChildren();
            dragHandle_.width = Mathf.Max(width, dragHandle_.width);
            dragHandle_.height = 32;
            lblCaption_.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            parent.FitChildren();
            Invalidate();
        }
    }
}
