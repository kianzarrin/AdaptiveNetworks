namespace AdaptiveRoads.Data.Flags {
    using KianCommons.Util;
    using System.Collections.Generic;

    public class TagSource {
        public Dictionary<string, int> AllTags = new();
        private List<ulong> allFlags_ = new();

        public static DynamicFlags EmptyFlags => new DynamicFlags(DynamicFlagsUtil.EMPTY_FLAGS);
        public DynamicFlags All => new DynamicFlags(allFlags_.ToArray());

        public void RegisterTags(string[] tags) {
            if (tags == null) return;
            foreach (string tag in tags)
                RegisterTag(tag);
        }
        public void RegisterTag(string tag) {
            if (tag == null)
                return;
            if (!AllTags.ContainsKey(tag))
                AllTags.Add(tag, AllTags.Count);
            if (AllTags.Count > allFlags_.Count * 64)
                allFlags_.Add(ulong.MaxValue);
        }
        public DynamicFlags GetFlags(params string[] tags) {
            ulong[] flags = new ulong[allFlags_.Count];
            if (tags != null) {
                foreach (string key in tags) {
                    if (AllTags.TryGetValue(key, out var index)) {
                        flags[index / 64] |= (ulong)(1L << index % 64);
                    }
                }
            }
            return new DynamicFlags(flags);
        }

        /// <summary>WARNING: low performance!</summary>
        public string[] GetTags(DynamicFlags flags) {
            List<string> tags = new();
            foreach (string tag in AllTags.Keys) {
                var flag = GetFlags(new[] { tag });
                bool hasFlag = !(flag & flags).IsEmpty;
                if (hasFlag) {
                    tags.Add(tag);
                }
            }
            return tags.ToArray();
        }
    }
}
