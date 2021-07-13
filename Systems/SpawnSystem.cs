using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Physics;
using Unity.Mathematics;

public class SpawnSystem : SystemBase
{
    private static EntityArchetype partArchetype;

    protected override void OnStartRunning()
    {        
        partArchetype = EntityManager.CreateArchetype(typeof(LocalToWorld), typeof(Translation), typeof(Rotation),
            typeof(RenderMesh), typeof(RenderBounds), typeof(PhysicsCollider), typeof(PartInfoComponent));
    }

    protected override void OnUpdate()
    {
        
    }


    public static Entity SpawnPart(PartType ptype, float3 position)
    {
        return SpawnPart(ptype, position, ResourcesMaster.defaultMaterial, false);
    }
    public static Entity SpawnPartForEditor(PartType ptype, float3 position)
    {
        return SpawnPart(ptype, position, ResourcesMaster.defaultMaterial, true);
    }
    public static Entity SpawnPart(PartType ptype, bool editable)
    {
        return SpawnPart(ptype, float3.zero, ResourcesMaster.defaultMaterial, editable);
    }
    public static Entity SpawnPart(PartType ptype, UnityEngine.Material m)
    {
        return SpawnPart(ptype, float3.zero, m, false);
    }

    public static Entity SpawnPart(PartType ptype, float3 position, UnityEngine.Material m, bool editable)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;         

        var entity = manager.CreateEntity(partArchetype);
        manager.SetComponentData(entity, new Translation() { Value = position });
        quaternion orientation = quaternion.identity;
        manager.SetComponentData(entity, new Rotation() { Value = orientation });

        var renderData = new RenderMesh() { mesh = ResourcesMaster.GetPartMesh(ptype), material = m , receiveShadows = true,
            castShadows = UnityEngine.Rendering.ShadowCastingMode.On, needMotionVectorPass = false};
        manager.SetSharedComponentData(entity, renderData);
        manager.SetComponentData(entity, new PhysicsCollider() { Value = ResourcesMaster.GetPartCollider(ptype) });
        //
        manager.SetComponentData(entity, new PartInfoComponent() { Value = ptype });
        if (editable) manager.AddComponent<UsingByEditorMarker>(entity);
        return entity;
    }
    

    //creating bodies from scratch: https://docs.unity3d.com/Packages/com.unity.physics@0.6/manual/interacting_with_bodies.html
}
