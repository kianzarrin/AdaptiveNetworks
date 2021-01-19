using System;
using System.Collections.Generic;
using System.Linq;

namespace Fbx {
    /// <summary>
    /// Represents a node in an FBX file
    /// </summary>
    public class FbxNode : FbxNodeList {
        /// <summary>
        /// The node name, which is often a class type
        /// </summary>
        /// <remarks>
        /// The name must be smaller than 256 characters to be written to a binary stream
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// The list of properties associated with the node
        /// </summary>
        /// <remarks>
        /// Supported types are primitives (apart from byte and char),arrays of primitives, and strings
        /// </remarks>
        public List<object> Properties { get; } = new List<object>();

        /// <summary>
        /// The first property element
        /// </summary>
        public object Value {
            get { return Properties.Count < 1 ? null : Properties[0]; }
            set {
                if (Properties.Count < 1)
                    Properties.Add(value);
                else
                    Properties[0] = value;
            }
        }

        /// <summary>
        /// Whether the node is empty of data
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(Name) && Properties.Count == 0 && Nodes.Count == 0;

        public override string ToString()
        {
#pragma warning disable
            return $"<{GetType().Name}> " + ShortName() +
            $"\n\tProperties(count:{Properties?.Count})=" + Properties.ToSTR() +
            $"\n\tNodes(count:{Nodes?.Count})=" + (Nodes?.Select(n => n?.ShortName())).ToSTR();
#pragma warning restore
        }

        public string ShortName(){
            string ret = Name.ToSTR() + ":";
            if (IsEmpty)
                return ret + "<Empty>";
            if(Value == null) {
                ret += "<null>";
            }else {
                ret += $"<{Value.GetType().Name}>{Value} ";
            }

            ret += Properties.Count + $" props and " + Nodes.Count + " nodes";
            
            return ret;
        }

        public override bool Diff(FbxNodeList rNode)
        {
            int? n = Properties?.Count;
            int? n2 = (rNode as FbxNode)?.Properties?.Count;
            bool ret = n != n2;
            if (ret) {
                string m = $"different Properties counts node1:{n} vs node2:{n2}";
                Console.WriteLine(m);
                return true;
            }
            for (int i = 0; i < n; ++i) {
                var prop1 = Properties[i];
                var prop2 = (rNode as FbxNode)?.Properties?[i];
                if (prop1!=prop2) {
                    ret = true;
                    string m = $"difference found in properties: {prop1} != {prop2}\n" +
                        $"node1:{this}\n" + $"node2:{rNode}";
                    Console.WriteLine(m);
                    break;
                }
            }
            ret |= base.Diff(rNode);
            if (ret) Console.WriteLine("difference found between\n -      " + this + "\n - and " + rNode);
            return ret;
        }
    }

    public static class StringExtensions {
        public static string ToSTR(this object o) => o?.ToString() ?? "<null>";
        public static string ToSTR(this string str) => str ?? "<null>";

        public static string ToSTR<T>(this IEnumerable<T> list)
        {
            if (list == null) return "<null>";
            string ret = "{ ";
            foreach (T item in list) {
                string s;
                ret += item.ToSTR() + ", ";
            }
            ret.Remove(ret.Length - 2, 2);
            ret += " }";
            return ret;
        }

    }
}
