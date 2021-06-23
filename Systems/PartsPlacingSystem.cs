using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
public class PartsPlacingSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        
    }

    public static void PlacePart(ref Entity e, Unity.Physics.RaycastHit hitInfo)
    {        
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        float boundDistance = manager.GetComponentData<PartInfoComponent>(e).Value.GetExtents();
        var x = hitInfo.SurfaceNormal * boundDistance;
        manager.SetComponentData(e, 
            new Translation() { Value = hitInfo.Position + x}
            );
        manager.SetComponentData(e, new Rotation() { Value = manager.GetComponentData<Rotation>(hitInfo.Entity).Value });
    }
}
