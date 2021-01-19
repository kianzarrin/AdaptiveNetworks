using System;
using System.Collections.Generic;
namespace Fbx
{
	/// <summary>
	/// Base class for nodes and documents
	/// </summary>
	public abstract class FbxNodeList
	{
		/// <summary>
		/// The list of child/nested nodes
		/// </summary>
		/// <remarks>
		/// A list with one or more null elements is treated differently than an empty list,
		/// and represented differently in all FBX output files.
		/// </remarks>
		public List<FbxNode> Nodes { get; } = new List<FbxNode>();

		/// <summary>
		/// Gets a named child node
		/// </summary>
		/// <param name="name"></param>
		/// <returns>The child node, or null</returns>
		public FbxNode this[string name] { get { return Nodes.Find(n => n != null && n.Name == name); } }

		/// <summary>
		/// Gets a child node, using a '/' separated path
		/// </summary>
		/// <param name="path"></param>
		/// <returns>The child node, or null</returns>
		public FbxNode GetRelative(string path)
		{
			var tokens = path.Split('/');
			FbxNodeList n = this;
			foreach (var t in tokens)
			{
				if (t == "")
					continue;
				n = n[t];
				if (n == null)
					break;
			}
			return n as FbxNode;
		}

		public virtual bool Diff(FbxNodeList rNode)
        {
			Type t1 = this.GetType();
			Type t2 = rNode.GetType();
			bool ret = t1!=t2;
            if (ret) {
				string m = $"node type mismatch node1:{t1} node2:{t2}";
				Console.WriteLine(m);
				return true;
			}
			int n = Nodes.Count;
			int n2 = rNode.Nodes.Count;
			if (n != n2) {
				string m = $"different node counts node1:{n} vs node2:{n2}";
				Console.WriteLine(m);
				return true;
			}
			for(int i = 0; i < n; ++i) {
				FbxNode node1 = Nodes[i];
				FbxNode node2 = rNode.Nodes[i];
                if (node1.Diff(node2)) {
					ret = true;
					string m = $"difference found:\n\t" + node1 + "\t\n" + node2;
					Console.WriteLine(m);
					return true; // comment out to see all differences.
				}
            }
			return ret;
        }
    }
}
