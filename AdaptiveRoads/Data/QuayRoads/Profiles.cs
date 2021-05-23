using System;
using static TerrainModify;

namespace AdaptiveRoads.Data
{
    struct ProfileSection
    {
        public ProfileSection(float[] posRel, float[] posAbs, float[] heightOffset, float[] heightTerrain, TerrainModify.Surface? surface, TerrainModify.Heights? heights, TerrainModify.Edges? edges)
        {

            PosRel = expand2(posRel);
            PosAbs = expand2(posAbs);
            HeightOffset = expand4(heightOffset);
            HeightTerrain = expand4(heightTerrain);
            Surface = surface;
            Heights = heights;
            EdgeFlags = edges;
        }
        /// <summary>
        /// relative position of section boundaries. 0 means left edge of segment, 1 means right edge of segment.
        /// 2 elements: left and right boundary
        /// </summary>
        public float[] PosRel; //{ get; }
        /// <summary>
        /// absolute offset of section boundaries. positive means right.
        /// 2: elements: left and right boundary
        /// </summary>
        public float[] PosAbs; //{ get; }
        /// <summary>
        /// absolute offset to node position for terrain height deformation<br/>
        /// 4 elements: startLeft, endLeft, startRight, endRight<br/>
        /// can be set by shorthand with 2 elements: left, right
        /// </summary>
        public float[] HeightOffset; //{ get; }
        /// <summary>
        /// original terrain height influence for terrain height deformation. 0 means no influence, 1 means no deformation
        /// </summary>
        public float[] HeightTerrain; //{ get; }
        /// <summary>
        /// surface "paint" to apply within this profile section. null means use default
        /// </summary>
        public Surface? Surface; //{ get; }
        /// <summary>
        /// height deformation mode to apply within this profile section. null means use default
        /// </summary>
        public Heights? Heights; //{ get; }
        /// <summary>
        /// edge mode to apply within this profile section. Effect unknown. null means use default
        /// </summary>
        public Edges? EdgeFlags; //{ get; }

        public ProfileSection Inverse()
        {
            Edges? invertedEdgeFlags = null;
            if (EdgeFlags.HasValue)
            {
                invertedEdgeFlags =
                        (EdgeFlags.Value & ~Edges.AB & ~Edges.CD)
                        | (((EdgeFlags.Value & Edges.AB) != 0) ? Edges.CD : 0)
                        | (((EdgeFlags.Value & Edges.CD) != 0) ? Edges.AB : 0);
            }
            return new ProfileSection(
                posRel: new float[] { 1f - PosRel[1], 1f - PosRel[0] },
                posAbs: new float[] { 0f - PosAbs[1], 0f - PosAbs[0] },
                heightOffset: new float[] { HeightOffset[2], HeightOffset[3], HeightOffset[0], HeightOffset[1] },
                heightTerrain: new float[] { HeightTerrain[2], HeightTerrain[3], HeightTerrain[0], HeightTerrain[1] },
                surface: Surface,
                heights: Heights,
                //swap AB and CD (why? I don't know, but this emulates QuaiAI.SegmentModifyMask behaviour)
                edges: invertedEdgeFlags
            );
        }

        private static T[] expand2<T>(T[] array)
        {
            switch (array.Length)
            {
                case 1:
                    return new T[] { array[0], array[0] };
                case 2:
                    return array;
                default:
                    throw new ArgumentException("Expected 1 or 2 elements, found: " + array.Length);
            }
        }
        private static T[] expand4<T>(T[] array)
        {
            switch (array.Length)
            {
                case 1:
                    return new T[] { array[0], array[0], array[0], array[0] };
                case 2:
                    return new T[] { array[0], array[0], array[1], array[1] };
                case 4:
                    return array;
                default:
                    throw new ArgumentException("Expected 1, 2 or 4 elements, found: " + array.Length);
            }
        }
    }

    class Profiles
    {
        public static ProfileSection[] OneSidedRoadProfile = {
            new ProfileSection(new float[]{0f, .5f}, new float[]{0f}, new float[]{0f,-.3f}, new float[]{0f }, null, null, null),
            new ProfileSection(new float[]{.5f, 1f}, new float[]{0f}, new float[]{-.3f}, new float[]{0f}, null, Heights.PrimaryMax, null)
        };
        public static ProfileSection[] QuayProfile = {
            new ProfileSection(new float[]{.5f, .5f}, new float[]{ -5f, 5f}, new float[]{ 0f}, new float[]{0f}, null, Heights.None, null) ,
            new ProfileSection(new float[]{.5f, .5f}, new float[]{ -1f, 5f}, new float[]{ 0f}, new float[]{0f}, Surface.None, Heights.PrimaryLevel | Heights.BlockHeight | Heights.RawHeight, Edges.BC | Edges.CD | Edges.DA),
            new ProfileSection(new float[]{.5f, .5f}, new float[]{-15f,-1f}, new float[]{-1f}, new float[]{0f}, Surface.None, Heights.PrimaryMax, Edges.AB | Edges.BC | Edges.DA),
            new ProfileSection(new float[]{.5f, 1f }, new float[]{  8f, 0f}, new float[]{ 0f}, new float[]{0f, .6f}, Surface.None, Heights.SecondaryLevel | Heights.BlockHeight | Heights.RawHeight, Edges.BC|Edges.CD|Edges.DA)
        };
        public static ProfileSection[] HalfpipeProfile =
        {
            new ProfileSection(new float[]{.4f,.6f}, new float[]{0f}, new float[]{0f}, new float[]{0f}, null, Heights.PrimaryLevel | Heights.BlockHeight | Heights.RawHeight, Edges.All),
            new ProfileSection(new float[]{.6f,.8f}, new float[]{0f}, new float[]{0f,4f}, new float[]{0f}, Surface.Gravel, Heights.PrimaryLevel | Heights.BlockHeight | Heights.RawHeight, Edges.All),
            new ProfileSection(new float[]{.8f,1f}, new float[]{0f}, new float[]{4f,10f}, new float[]{0f}, Surface.Field, Heights.PrimaryLevel | Heights.BlockHeight | Heights.RawHeight, Edges.All),
            new ProfileSection(new float[]{.2f,.4f}, new float[]{0f}, new float[]{4f,0f}, new float[]{0f}, Surface.None, Heights.PrimaryLevel | Heights.BlockHeight | Heights.RawHeight, Edges.All),
            new ProfileSection(new float[]{0f,.2f}, new float[]{0f}, new float[]{10f,4f}, new float[]{0f}, Surface.PavementA, Heights.PrimaryLevel | Heights.BlockHeight | Heights.RawHeight, Edges.All)
        };
        public static ProfileSection[] PainterProfile =
        {
            new ProfileSection(new float[]{0/7f,1/7f}, new float[]{0f}, new float[]{0f}, new float[]{0f}, Surface.Clip, Heights.None, null),
            new ProfileSection(new float[]{1/7f,2/7f}, new float[]{0f}, new float[]{0f}, new float[]{0f}, Surface.Field, Heights.None, null),
            new ProfileSection(new float[]{2/7f,3/7f}, new float[]{0f}, new float[]{0f}, new float[]{0f}, Surface.Gravel, Heights.None, null),
            new ProfileSection(new float[]{3/7f,4/7f}, new float[]{0f}, new float[]{0f}, new float[]{0f}, Surface.PavementA, Heights.None, null),
            new ProfileSection(new float[]{4/7f,5/7f}, new float[]{0f}, new float[]{0f}, new float[]{0f}, Surface.PavementB, Heights.None, null),
            new ProfileSection(new float[]{5/7f,6/7f}, new float[]{0f}, new float[]{0f}, new float[]{0f}, Surface.Ruined, Heights.None, null),
            new ProfileSection(new float[]{6/7f,7/7f}, new float[]{0f}, new float[]{0f}, new float[]{0f}, Surface.RuinedWeak, Heights.None, null)
        };
    }

}