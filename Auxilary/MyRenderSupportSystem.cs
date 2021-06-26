using Unity.Entities;
using UnityEngine;

public static class MyRenderSupportSystem
{
    public static int invisibleLayer = LayerMask.NameToLayer("Invisible"), 
        defaultLayer = LayerMask.NameToLayer("Default");
}
