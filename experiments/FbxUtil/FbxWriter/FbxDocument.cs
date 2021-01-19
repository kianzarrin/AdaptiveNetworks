using System.Linq;

namespace Fbx
{
	/// <summary>
	/// A top-level FBX node
	/// </summary>
	public class FbxDocument : FbxNodeList
	{
		/// <summary>
		/// Describes the format and data of the document
		/// </summary>
		/// <remarks>
		/// It isn't recommended that you change this value directly, because
		/// it won't change any of the document's data which can be version-specific.
		/// Most FBX importers can cope with any version.
		/// </remarks>
		public FbxVersion Version { get; set; } = FbxVersion.v7_4;

		public override string ToString() => $"<{GetType().Name}> Version={(int)Version} "
            + $"Nodes(count:{Nodes?.Count})=" + (Nodes?.Select(n => n?.ShortName())).ToSTR();

		public override bool Diff(FbxNodeList rNode)
        {
			bool ret = Version != (rNode as FbxDocument)?.Version;
            ret |=  base.Diff(rNode);
			if (ret) System.Console.WriteLine("difference found between:\n -     " + this + "\n - and " + rNode);
			return ret;
		}
	}
}
