namespace AdaptiveRoads.Data.Flags {
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TagSource {
        public Dictionary<string, int> Tags2Index = new();
        private List<ulong> allFlags_ = new();

        public static DynamicFlags NONE => DynamicFlagsUtil.NONE;
        public DynamicFlags All => new DynamicFlags(allFlags_.ToArray());
        public string[] AllTags => Tags2Index.Keys.ToArray();

        public event Action EventChanged;

        public void RegisterTags(string[] tags) {
            if (tags == null) return;
            foreach (string tag in tags)
                RegisterTag(tag);
        }
        public void RegisterTag(string tag) {
            if (tag == null)
                return;
            if (!Tags2Index.ContainsKey(tag)) 
                Tags2Index.Add(tag, Tags2Index.Count);
            if (Tags2Index.Count > allFlags_.Count * 64)
                allFlags_.Add(ulong.MaxValue);
            EventChanged?.Invoke();
        }

        public DynamicFlags GetFlags(params string[] tags) {
            ulong[] flags = new ulong[allFlags_.Count];
            if (tags != null) {
                foreach (string key in tags) {
                    if (Tags2Index.TryGetValue(key, out var index)) {
                        flags[index / 64] |= (ulong)(1L << index % 64);
                    }
                }
            }
            return new DynamicFlags(flags);
        }

        /// <summary>WARNING: low performance!</summary>
        public string[] GetTags(DynamicFlags flags) {
            List<string> tags = new();
            foreach (string tag in Tags2Index.Keys) {
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
