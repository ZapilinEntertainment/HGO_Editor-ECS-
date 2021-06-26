using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using UE = UnityEngine; // ehehe
using Unity.Collections;

public class ElementsChangeSystem : SystemBase
{
    private bool isActive = false;
    private UE.Material normalMaterial, selectedMaterial;

    protected override void OnStartRunning()
    {
        
    }

    protected override void OnUpdate()
    {

    }


    public void Activate(float3 pos)
    {
        if (!isActive)
        {
            isActive = true;
            var manager = EntityManager;
            var array = manager.CreateEntityQuery(typeof(ControllerElementComponent)).ToEntityArray(Allocator.Temp);
            RenderMesh rm;
            int i_layer = MyRenderSupportSystem.defaultLayer;
            foreach (var e in array)
            {
                rm = manager.GetSharedComponentData<RenderMesh>(e);
                rm.layer = i_layer;
                manager.SetSharedComponentData(e, rm);
            }
            array.Dispose();
        }
    }
    public void Disable()
    {
        if (isActive)
        {
            isActive = false;
            var manager = EntityManager;
            var array = manager.CreateEntityQuery(typeof(ControllerElementComponent)).ToEntityArray(Allocator.Temp);
            RenderMesh rm;
            int i_layer = MyRenderSupportSystem.invisibleLayer;
            foreach (var e in array)
            {
                rm = manager.GetSharedComponentData<RenderMesh>(e);
                rm.layer = i_layer;
                manager.SetSharedComponentData(e, rm);
            }
            array.Dispose();
        }
    }
}
