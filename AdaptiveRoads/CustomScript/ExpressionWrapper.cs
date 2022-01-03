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

        public ExpressionWrapper(FileInfo dllFile, string name) : this(dllFile.OpenRead().ReadToEnd(), name) { }

        public ExpressionWrapper(byte[] data, string name) {
            Name = name;
            Init(data);
        }

        //serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            try {
                if (Log.VERBOSE) Log.Called();
                SerializationUtil.GetObjectFields(info, this);
            } catch (Exception ex) {
                ex.Log();
            }
        }

        // deserialization
        public ExpressionWrapper(SerializationInfo info, StreamingContext context) {
            try {
                if (Log.VERBOSE) Log.Called();
                SerializationUtil.SetObjectFields(info, this);
                Init(AssemblyData);
            } catch (Exception ex) {
                ex.Log();
            }
            Log.Succeeded(this.ToSTR());
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


        public PredicateBase GetPredicateInstance(ushort segmentID, ushort nodeID, int laneIndex = -1) {
            Assertion.NotNull(Instance, "Instance");
            Instance.NodeID = nodeID;
            Instance.SegmentID = segmentID;
            Instance.LaneIndex = laneIndex;
            return Instance;
        }

        public bool Condition(ushort segmentID, ushort nodeID, int laneIndex = -1) {
            try {
                var instance = GetPredicateInstance(segmentID, nodeID, laneIndex);
                if (Log.VERBOSE) Log.Debug("Getting condition for " + this, false);
                return instance.Condition();
            }catch(Exception ex) {
                ex.Log($"failed to get condition from {Instance}", showInPannel: false);
                return false;
            }
        }

        public override string ToString() => $"ExpressionWrapper(Name={Name}, Instance={Instance}, Assembly='{Asm.GetName().Name} V{Asm.VersionOf()}')";
    }
}
