using UnityEngine;
using System.Collections.Generic;

public static class ResourcesMaster
{
    public readonly static Material defaultMaterial,  transparentMaterial, selectedMaterial;
    private static Dictionary<PartType, Mesh> meshesData;

    static ResourcesMaster()
    {
        defaultMaterial = Resources.Load<UnityEngine.Material>("Materials/defaultMaterial");
        transparentMaterial = Resources.Load<UnityEngine.Material>("Materials/transparentMaterial");
        selectedMaterial = Resources.Load<UnityEngine.Material>("Materials/selectedMaterial");
    }

    public static Mesh GetPartMesh(PartType ptype)
    {
        if (meshesData == null)
        {
            meshesData = new Dictionary<PartType, Mesh>();
        }
        if (!meshesData.ContainsKey(ptype)) 
        {
            meshesData.Add(ptype, ptype.LoadMesh());
        }
        return meshesData[ptype];
    }
}
