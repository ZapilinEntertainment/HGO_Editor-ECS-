using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class MoveSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        
    }

    public void MoveEntityToPoint(Entity e, float3 position)
    {
        EntityManager.SetComponentData(e, new Translation() { Value = position });
    }
}
