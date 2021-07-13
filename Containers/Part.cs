using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using Unity.Mathematics;

public enum PartType : ushort
{
    Cube, Cylinder, Sphere,Torus, TotalCount
}

public static class PartTypeExtension {
    public static void SetIcon(this PartType ptype, ref RawImage rimage)
    {
        Object asset;
        switch (ptype)
        {
            case PartType.Cube: asset = Resources.Load("Cube") ; break;
            case PartType.Cylinder: asset = Resources.Load("Cylinder"); break;
            case PartType.Sphere: asset = Resources.Load("Sphere"); break;
            default: return;
        }
        if (asset == null) return;
        var tx = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(asset));
        if (tx == null) return;
        rimage.texture = tx;
        rimage.uvRect = new Rect(0f, 0f, tx.width, tx.height);
    }
    public static float GetExtents(this PartType ptype)
    {
        switch (ptype)
        {
            default: return 0.5f;
        }
    }
    public static Mesh LoadMesh(this PartType ptype)
    {
        string name;
        switch (ptype)
        {
            case PartType.Cylinder: name = "Cylinder"; break;
            case PartType.Sphere: name = "Sphere"; break;
            case PartType.Torus: return Resources.Load<MeshFilter>("rotatingTorus").sharedMesh;
            default: name = "Cube"; break;
        }
        return Resources.Load<GameObject>(name).GetComponent<MeshFilter>().sharedMesh;
    }
    public static BlobAssetReference<Unity.Physics.Collider> LoadMeshCollider(this PartType ptype)
    {
        var i_orientation = quaternion.identity;
        switch (ptype)
        {
            case PartType.Torus:
                {
                    Mesh i_mesh = ResourcesMaster.GetPartMesh(ptype);
                    int count = i_mesh.vertices.Length, i = 0;
                    var verts = new NativeArray<float3>(count, Allocator.Temp);
                    for (; i < count; i++)
                    {
                        verts[i] = i_mesh.vertices[i];
                    }
                    count = i_mesh.triangles.Length / 3;
                    var tris = new NativeArray<int3>(count, Allocator.Temp);
                    for (i = 0; i < count; i++)
                    {
                        tris[i] = new int3(i_mesh.triangles[i * 3], i_mesh.triangles[i * 3 + 1], i_mesh.triangles[i * 3 + 2]);
                    }
                    var x = Unity.Physics.MeshCollider.Create(verts, tris);
                    verts.Dispose();
                    tris.Dispose();      
                    
                    return x;
                }
            default:  return Unity.Physics.BoxCollider.Create(new BoxGeometry() { Size = 1f, Orientation = i_orientation }, MyCollisionLayerExtension.allDetailsFilter);
        }
    }
}
