using AdaptiveRoads.Manager;
using KianCommons.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static TerrainModify;

namespace AdaptiveRoads.Data.QuayRoads {
    [Serializable]
    public struct ProfileSection : ISerializable {

        [CustomizableProperty("left", "position")]
        [Hint("relative position of the left section boundary. 0 means left edge of segment, 1 means right edge of segment.")]
        public float LeftX;
        [CustomizableProperty("right", "position")]
        [Hint("relative position of the right section boundary. 0 means left edge of segment, 1 means right edge of segment.")]
        public float RightX;

        [CustomizableProperty("left start", "height offset")]
        [Hint("Height offset at left start corner for terrain deformation. Value is in meters, invert flag is respected.")]
        public float LeftStartY;
        [CustomizableProperty("left end", "height offset")]
        [Hint("Height offset at left end corner for terrain deformation. Value is in meters, invert flag is respected.")]
        public float LeftEndY;
        [CustomizableProperty("right start", "height offset")]
        [Hint("Height offset at right start corner for terrain deformation. Value is in meters, invert flag is respected.")]
        public float RightStartY;
        [CustomizableProperty("right end", "height offset")]
        [Hint("Height offset at right end corner for terrain deformation. Value is in meters, invert flag is respected.")]
        public float RightEndY;

        [CustomizableProperty("surface flags", "surface flags")]
        [Hint("surface \"paint\" to apply within this profile section.")]
        public Surface Surface;
        [CustomizableProperty("height flags", "height flags")]
        [Hint("height deformation mode to apply within this profile section.")]
        public Heights Heights;
        [CustomizableProperty("edge flags", "edge flags")]
        [Hint("edge mode to apply within this profile section. Effect unknown. null means use default")]
        public Edges Edges;

        public ProfileSection(float leftX = 0, float rightX = 1, float leftStartY = 0, float leftEndY = 0, float rightStartY = 0, float rightEndY = 0, Surface surface = Surface.None, Heights heights = Heights.None, Edges edges = Edges.All) {
            LeftX = leftX;
            RightX = rightX;
            LeftStartY = leftStartY;
            LeftEndY = leftEndY;
            RightStartY = rightStartY;
            RightEndY = rightEndY;
            Surface = surface;
            Heights = heights;
            Edges = edges;
        }

        public static ProfileSection Default() {
            return new ProfileSection(
                leftX: 0,
                rightX: 1,
                leftStartY: 0,
                leftEndY: 0,
                rightStartY: 0,
                rightEndY: 0,
                surface: Surface.None,
                edges: Edges.All,
                heights: Heights.None
            );
        }

        public ProfileSection Inverse() {
            //swap AB and CD (why? I don't know, but this emulates QuaiAI.SegmentModifyMask behaviour)
            Edges invertedEdgeFlags =
                        (Edges & ~Edges.AB & ~Edges.CD)
                        | ((Edges & Edges.AB) != 0 ? Edges.CD : 0)
                        | ((Edges & Edges.CD) != 0 ? Edges.AB : 0);

            return new ProfileSection(
                leftX: 1 - RightX,
                rightX: 1 - LeftX,
                leftStartY: RightStartY,
                leftEndY: RightEndY,
                rightStartY: LeftStartY,
                rightEndY: LeftEndY,
                surface: Surface,
                heights: Heights,
                edges: invertedEdgeFlags
            ); ;
        }

        #region serialization
        //serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context) =>
            SerializationUtil.GetObjectFields(info, this);

        // deserialization
        public ProfileSection(SerializationInfo info, StreamingContext context) {
            this = Default();
            SerializationUtil.SetObjectFields(info, ref this);
        }
        #endregion
    }
    static class Profiles {
        public static ProfileSection[] Inverse(this ProfileSection[] original) {
            return original
                .Select(section => section.Inverse())
                .ToArray();
        }
        public static Dictionary<string, ProfileSection[]> presets = new() {
            {
                "OneSidedRoad",
                new ProfileSection[] {
                    new ProfileSection(
                        leftX: .5f,
                        rightX: 1f,
                        leftStartY: -.3f,
                        leftEndY: -.3f,
                        rightStartY: 0f,
                        rightEndY: 0f,
                        surface: Surface.PavementA | Surface.Clip,
                        heights: Heights.PrimaryLevel,
                        edges: Edges.All
                    ),
                    new ProfileSection(
                        leftX: 0f,
                        rightX: .5f,
                        leftStartY: -.3f,
                        leftEndY: -.3f,
                        rightStartY: -.3f,
                        rightEndY: -.3f,
                        surface: Surface.None,
                        heights: Heights.PrimaryMax,
                        edges: Edges.BC | Edges.CD | Edges.DA
                    )
                }
            },
            {
                "Painter",
                new ProfileSection[]
                {
                    new ProfileSection(
                        leftX: 0 / 7f,
                        rightX: 1 / 7f,
                        surface: Surface.Clip
                        ),
                    new ProfileSection(
                        leftX: 1 / 7f,
                        rightX: 2 / 7f,
                        surface: Surface.Field
                        ),
                    new ProfileSection(
                        leftX: 2 / 7f,
                        rightX: 3 / 7f,
                        surface: Surface.Gravel
                        ),
                    new ProfileSection(
                        leftX: 3 / 7f,
                        rightX: 4 / 7f,
                        surface: Surface.PavementA
                        ),
                    new ProfileSection(
                        leftX: 4 / 7f,
                        rightX: 5 / 7f,
                        surface: Surface.PavementB
                        ),
                    new ProfileSection(
                        leftX: 5 / 7f,
                        rightX: 6 / 7f,
                        surface: Surface.Ruined
                        ),
                    new ProfileSection(
                        leftX: 6 / 7f,
                        rightX: 7 / 7f,
                        surface: Surface.RuinedWeak
                        )
                }
            }
        };
    }

}