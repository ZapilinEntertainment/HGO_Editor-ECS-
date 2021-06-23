using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public enum PartType : ushort
{
    Cube, Cylinder, Sphere, TotalCount
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
        string name = "Cube";
        if (ptype != PartType.Cube)
        {
            switch (ptype)
            {
                case PartType.Cylinder: name = "Cylinder"; break;
                case PartType.Sphere: name = "Sphere"; break;
            }
        }
        return Resources.Load<GameObject>(name).GetComponent<MeshFilter>().sharedMesh;
    }
}
