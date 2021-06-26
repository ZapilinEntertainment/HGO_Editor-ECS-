using UnityEngine;
using System.Collections.Generic;
using Unity.Entities;

public static class ResourcesMaster
{
    public readonly static Material defaultMaterial,  transparentMaterial, selectedMaterial;
    private static Dictionary<PartType, Mesh> meshesData;
    private static Dictionary<PartType, BlobAssetReference<Unity.Physics.Collider>> collidersData;

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
    public static BlobAssetReference<Unity.Physics.Collider> GetPartCollider(PartType ptype)
    {
        if (collidersData == null)
        {
            collidersData = new Dictionary<PartType, BlobAssetReference<Unity.Physics.Collider>>();
        }        
        if (!collidersData.ContainsKey(ptype))
        {
            var cd = ptype.LoadMeshCollider();
            collidersData.Add(ptype, cd);
            return cd;
        }        
        else return collidersData[ptype];
    }
}
