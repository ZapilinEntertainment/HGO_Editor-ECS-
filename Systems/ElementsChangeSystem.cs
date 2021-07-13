using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using UE = UnityEngine; // ehehe
using Unity.Collections;

public class ElementsChangeSystem : SystemBase
{
    public bool editingATM { get; private set; }
    private bool elementSelected =false;
    private ControllerElementComponent controllerElementData;
    private float3 prevdir;
    private UE.Material normalMaterial, selectedMaterial;

    protected override void OnStartRunning()
    {
        normalMaterial = UE.Resources.Load<UE.Material>("Materials/ArrowsMaterial");
        selectedMaterial = UE.Resources.Load<UE.Material>("Materials/ControlElementSelectedMaterial");

        var e = SpawnSystem.SpawnPart(PartType.Torus, false);
        var manager = EntityManager;
        manager.AddComponent<ControllerElementComponent>(e);
        var cec = new ControllerElementComponent() { type = ControllerElementType.RotatingWheel };
        cec.SYSTEM_AssignID();
        manager.SetComponentData(e, cec);
        var rm = manager.GetSharedComponentData<RenderMesh>(e);
        rm.layer = MyRenderSupportSystem.invisibleLayer;
        rm.material = normalMaterial;
        manager.SetSharedComponentData(e, rm);
    }


    protected override void OnUpdate()
    {
        if (elementSelected)
        {
            //следование управляющих элементов за целью
            var manager = EntityManager;
            float3 pos = float3.zero;
            quaternion rot = quaternion.identity;
            using (var array = manager.CreateEntityQuery(typeof(SelectedObjectMarker)).ToEntityArray(Allocator.Temp))
            {               
                if (array.Length == 0)
                {
                    PartDeselected();
                    return;
                }
                else
                {
                    var e = array[0];
                    pos = manager.GetComponentData<Translation>(e).Value;
                    rot = manager.GetComponentData<Rotation>(e).Value;                         
                }
            }
            using (var array = manager.CreateEntityQuery(typeof(ControllerElementComponent)).ToEntityArray(Allocator.Temp))
            {
                foreach (var e in array)
                {
                    manager.SetComponentData(e, new Translation() { Value = pos });
                    manager.SetComponentData(e, new Rotation() { Value = rot });
                }
            }
            // отслеживание действий над управляющим элементом
            switch (controllerElementData.type)
            {
                case ControllerElementType.RotatingWheel:
                    {

                        break;
                    }
            }
        }
    }

    public void PartWasSelected()
    {
        if (!elementSelected)
        {
            elementSelected = true;
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
        //двигать не нужно - этим занимается onUpdate
    }
    public void PartDeselected()
    {
        if (elementSelected)
        {
            if (elementSelected)
            {
                ControlElementStopTouch();
            }
            elementSelected = false;

            var manager = EntityManager;
            var array = manager.CreateEntityQuery(typeof(ControllerElementComponent)).ToEntityArray(Allocator.Temp);
            RenderMesh rm;
            int i_layer = MyRenderSupportSystem.invisibleLayer;
            foreach (var e in array)
            {
                rm = manager.GetSharedComponentData<RenderMesh>(e);
                rm.layer = i_layer;
                rm.material = normalMaterial;
                manager.SetSharedComponentData(e, rm);
            }
            array.Dispose();
            
        }
    }

    public void ControlElementStartTouch(RaycastHit rh)
    {
        var manager = EntityManager;
        var target = rh.Entity;
        controllerElementData = manager.GetComponentData<ControllerElementComponent>(target);
        prevdir = INLINE_CalculateRotationCircleDirection(target, rh.Position);
        //
        manager.AddComponent<SelectedObjectMarker>(rh.Entity);
        var rm = EntityManager.GetSharedComponentData<RenderMesh>(target);
        rm.material = selectedMaterial;
        EntityManager.SetSharedComponentData(target, rm);
        //
        editingATM = true;
    }
    private float3 INLINE_CalculateRotationCircleDirection(Entity target, float3 touchPosition)
    {
        var manager = EntityManager;
        return UE.Vector3.ProjectOnPlane(touchPosition - manager.GetComponentData<Translation>(target).Value, manager.GetComponentData<LocalToWorld>(target).Forward).normalized;
    }
    public void ControlMoveTouch(RaycastHit rh)
    {
        var manager = EntityManager;
        using (var list = manager.CreateEntityQuery(typeof(SelectedObjectMarker), typeof(LocalToWorld)).ToEntityArray(Allocator.Temp))
        {
            var e = list[0];
            var ndir = INLINE_CalculateRotationCircleDirection(e, rh.Position);
            var rv = UE.Quaternion.FromToRotation(prevdir, ndir);

            var r = manager.GetComponentData<Rotation>(e);
            manager.SetComponentData(e, new Rotation() { Value =rv });
        }
    }
    public void ControlMoveTouch(float2 cursorPosition)
    {
        if (controllerElementData.type == ControllerElementType.RotatingWheel)
        {
            var manager = EntityManager;

            UE.Ray ray = UE.Camera.main.ScreenPointToRay(new float3(cursorPosition.x, cursorPosition.y, 0f));
            using (var array = manager.CreateEntityQuery(typeof(SelectedObjectMarker), typeof(ControllerElementComponent)).ToEntityArray(Allocator.Temp))
            {
                var e = array[0];
                float3 p = manager.GetComponentData<Translation>(e).Value,
                    f = manager.GetComponentData<LocalToWorld>(e).Forward,
                    v = UE.Vector3.Normalize(ray.direction),
                    n = ray.origin;
                float d = -f.x * p.x - f.y * p.y - f.z * p.z;
                float low = (f.x * v.x + f.y * v.y + f.z * v.z);
                if (low == 0f) return;
                else
                {
                    float t = -1f * (f.x * n.x + f.y * n.y + f.z * n.z + d) / low;
                    float3 virtualTouchPos = new float3(v.x * t + n.x, v.y * t + n.y, v.z * t + n.z);
                    //var ndir = virtualTouchPos - p;
                    float3 ndir = UE.Vector3.ProjectOnPlane(ray.direction, f);
                    float m = UE.Vector3.Magnitude(ndir);                    
                    if (m < 15f && m != 0f)
                    {
                        ndir = UE.Vector3.Normalize(ndir);
                        var rv = UE.Quaternion.FromToRotation(prevdir, ndir);
                        using (var targetarray = manager.CreateEntityQuery(typeof(SelectedObjectMarker), typeof(PartInfoComponent)).ToEntityArray(Allocator.Temp))
                        {
                            var te = targetarray[0];
                            var r = manager.GetComponentData<Rotation>(te);
                            manager.SetComponentData(te, new Rotation() { Value = rv });
                        }
                        prevdir = ndir;
                    }
                }
            }
        }
    }
    public void ControlElementStopTouch()
    {
        if (elementSelected)
        {
            var manager = EntityManager;
            using (var array = manager.CreateEntityQuery(typeof(SelectedObjectMarker), typeof(ControllerElementComponent), typeof(RenderMesh)).ToEntityArray(Allocator.Temp))
            {
                foreach (var e in array)
                {
                    manager.RemoveComponent<SelectedObjectMarker>(e);
                    var rm = EntityManager.GetSharedComponentData<RenderMesh>(e);
                    rm.material = normalMaterial;
                    EntityManager.SetSharedComponentData(e, rm);
                }
            }
            editingATM = false;
        }
    }
}
