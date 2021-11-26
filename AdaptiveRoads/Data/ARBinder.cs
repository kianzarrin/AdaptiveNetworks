namespace AdaptiveRoads.Data {
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using KianCommons;
    using KianCommons.Serialization;

    public class ARBinder : SerializationBinder {
        public Assembly ThisAssembly;
        public AssemblyName ThisAssemblyName;
        public ARBinder() {
            ThisAssembly = typeof(ARBinder).Assembly;
            ThisAssemblyName = ThisAssembly.GetName();
        }

        public override Type BindToType(string assemblyName0, string fulltypeName0) {
            if(Log.VERBOSE) Log.Called(assemblyName0, fulltypeName0);
            AssemblyName assemblyName = new AssemblyName(assemblyName0);
            if(assemblyName.Name == ThisAssemblyName.Name) {
                var ret = ThisAssembly.GetType(fulltypeName0);
                if(ret != null) return null;

                var typeName = new AssemblyBinder.TypeName(fulltypeName0);
                if(Log.VERBOSE) Log.Debug($"ARBinder: searching for type:'{typeName.NestedName}' in another name space");
                foreach(var type2 in ThisAssembly.GetTypes()) {
                    if(typeName.Name == type2.Name && type2.Namespace.StartsWith("AdaptiveRoads")) {
                        var typeName2 = new AssemblyBinder.TypeName(type2.FullName);
                        if(typeName2.NestedName == typeName.NestedName)
                            return type2;
                    }
                    if(type2.FullName.Contains("NetSegmentExt")) {
                        var typeName2 = new AssemblyBinder.TypeName(type2.FullName);
                        Log.Debug($"{typeName2.NestedName} did not match {typeName.NestedName}.\n" +
                            $"name={type2.Name} did not match {typeName.Name}\n" +
                            $"{type2.FullName} did not match {fulltypeName0}");
                    }
                }
            }
            if(Log.VERBOSE) Log.Debug("ARBinder: return null");
            return null;
        }
    }
}
