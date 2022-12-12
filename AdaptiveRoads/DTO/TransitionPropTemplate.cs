namespace AdaptiveRoads.DTO;
using AdaptiveRoads.Util;
using KianCommons;
using System.Linq;
using static AdaptiveRoads.Manager.NetInfoExtionsion;

public class TransitionPropTemplate : TemplateBase<TransitionPropTemplate> {
    public TransitionProp[] Props { get; private set; }
    public TransitionProp[] GetProps() => Props.ToArray();

    public override string Summary {
        get {
            string ret = Name + $"({Date})";
            if (!string.IsNullOrEmpty(Description))
                ret += "\n" + Description;
            var summaries = Props.Select(_prop => _prop.Summary());
            ret += "\n" + summaries.JoinLines();
            return ret;
        }
    }

    public static TransitionPropTemplate Create(
        string name,
        TransitionProp[] props,
        string description) {
        return new TransitionPropTemplate {
            Name = name,
            Props = props.ToArray(),
            Description = description,
        };
    }
}
