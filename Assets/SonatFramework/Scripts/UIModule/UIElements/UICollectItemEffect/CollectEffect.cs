using System;
using Sonat.Enums;
using UnityEngine;

public abstract class SonatCollectEffect
{
    public abstract void Collect(GameResource resource, int quantity, Vector3 startPos, Vector3 endPos, Action callback);
}
