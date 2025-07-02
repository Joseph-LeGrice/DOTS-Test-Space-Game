using System;
using UnityEngine;

public class BehaviourNodeReferenceAttribute : PropertyAttribute
{
    public readonly bool ConnectsIn = false;

    public BehaviourNodeReferenceAttribute(bool connectsIn = false)
    {
        ConnectsIn = connectsIn;
    }
}
