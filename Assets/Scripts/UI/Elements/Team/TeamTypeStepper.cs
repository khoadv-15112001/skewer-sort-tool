using System;
using System.Collections.Generic;
using UnityEngine;

public class TeamTypeStepper : EnumStepper<TeamType>
{
    [SerializeField] private List<TeamTypeLabel> labels;

    protected override string Format(TeamType v)
    {
        if (labels != null)
        {
            foreach (var m in labels)
                if (EqualityComparer<TeamType>.Default.Equals(m.value, v))
                    return string.IsNullOrEmpty(m.label) ? base.Format(v) : m.label;
        }
        return base.Format(v);
    }
}

[Serializable]
public struct TeamTypeLabel
{
    public TeamType value;
    public string label;
}
