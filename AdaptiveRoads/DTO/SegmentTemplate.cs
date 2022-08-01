using AdaptiveRoads.Util;
using KianCommons;
using System;
using System.IO;
using System.Linq;

namespace AdaptiveRoads.DTO {
    public class SegmentTemplate : TemplateBase<SegmentTemplate> {
        public NetInfoDTO.Segment[] Segments { get; private set; }
        public NetInfo.Segment[] GetSegments() =>
            Segments.Select(_item => (NetInfo.Segment)_item).ToArray();

        public override string Summary {
            get {
                string ret = Name + $"({Date})";
                if (!string.IsNullOrEmpty(Description))
                    ret += "\n" + Description;
                var summaries = GetSegments().Select(_item => _item.Summary());
                ret += "\n" + summaries.JoinLines();
                return ret;
            }
        }

        public static SegmentTemplate Create(
            string name,
            NetInfo.Segment[] segments,
            string description) {
            return new SegmentTemplate {
                Name = name,
                Segments = segments.Select(_item => (NetInfoDTO.Segment)_item).ToArray(),
                Description = description,
            };
        }
    }
}
