namespace AdaptiveRoads.CustomScript {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using KianCommons;
    using KianCommons.Serialization;
    using Mono.Cecil;

    [Serializable]
    public class ExpressionWrapper : ISerializable {
        #region life-cycle
        private ExpressionWrapper() { }

        public ExpressionWrapper(FileInfo file, string name) : this(file.OpenRead().ReadToEnd(), name) { }

        public ExpressionWrapper(byte[] data, string name) {
            Name = name;
            Init(data);
        }

        //serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            try {
                if (Log.VERBOSE) Log.Called();
                SerializationUtil.GetObjectFields(info, this);
                Init(AssemblyData);
            } catch (Exception ex) {
                ex.Log();
            }
        }

        // deserialization
        public ExpressionWrapper(SerializationInfo info, StreamingContext context) {
            try {
                if (Log.VERBOSE) Log.Called();
                SerializationUtil.SetObjectFields(info, this);
            } catch (Exception ex) {
                ex.Log();
            }
            Log.Succeeded();
        }

        private void Init(byte []data) {
            AssemblyData = data;
            Asm = ScriptCompiler.AddAssembly(AssemblyData);
            Instance = ScriptCompiler.GetPredicateInstance(Asm);
        }
        #endregion

        public string Name;

        public byte[] AssemblyData;

        [NonSerialized]
        public Assembly Asm;
        [NonSerialized]
        public PredicateBase Instance;


        public PredicateBase GetPredicateInstance(ushort segmentID, ushort nodeID) {
            Instance.NodeID = nodeID;
            Instance.SegmentID = segmentID;
            return Instance;
        }

        public bool Condition(ushort segmentID, ushort nodeID) {
            var instance = GetPredicateInstance(segmentID, nodeID);
            return instance.Condition();
        }
    }
}
