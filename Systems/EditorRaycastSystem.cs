using Unity.Physics;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class EditorRaycastSystem : ComponentSystem
{
    public bool castingAvailable = true;
    private bool alignment = true, detailSelected = false, mouseClickedDown = false, controlElementTouched = false;
    private int controlElementID = -1;
    private ElementsChangeSystem changeSystem;
    public const float RAY_LENGTH = 20f, ALIGNMENT_LIMIT = 0.1f;
    private BlobAssetReference<Unity.Physics.Collider> selectedColliderCopy;
   

    protected override void OnStartRunning()
    {
        var manager = EntityManager;
        var e = SpawnSystem.SpawnPartForEditor(PartType.Cube, float3.zero);;
        e = SpawnSystem.SpawnPartForEditor(PartType.Cube, new float3(0.2f,-0.3f,1f));
        e = SpawnSystem.SpawnPartForEditor(PartType.Cube, new float3(0.4f, -0.6f, 2f));
        // ---
        var world = manager.World;
        changeSystem = world.GetExistingSystem<ElementsChangeSystem>();
        //
      
        var tsystem = world.GetExistingSystem<TouchControlSystem>();
        tsystem.touchStartedEvent += this.TouchStart;
        tsystem.touchEndedEvent += this.TouchEnded;
        tsystem.touchMovedEvent += this.TouchMoved;
    }

    private void TouchStart(float2 mpos)
    {
        controlElementTouched = false;
        if (castingAvailable)
        {                       
            Unity.Physics.RaycastHit rh;
            var physicsSystem = EntityManager.World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
            var collisionWorld = physicsSystem.PhysicsWorld.CollisionWorld;
            if (collisionWorld.CastRay(GetInputRay(mpos, MyCollisionLayerExtension.allDetailsFilter), out rh))
            {
                var types = EntityManager.GetComponentTypes(rh.Entity, Allocator.Temp);
                if (!types.Contains(typeof(SelectedObjectMarker)))
                {
                    if (types.Contains(typeof(UsingByEditorMarker)) && types.Contains(typeof(PartInfoComponent))) SelectPart(rh.Entity);
                    else
                    {
                        if (types.Contains(typeof(ControllerElementComponent)))
                        {
                            changeSystem.ControlElementStartTouch(rh);
                            controlElementTouched = true;
                            controlElementID = EntityManager.GetComponentData<ControllerElementComponent>(rh.Entity).ID;
                        }
                        else if (detailSelected) DeselectPart();
                    }
                }                
                types.Dispose();
            }
            else
            {
                if (detailSelected) DeselectPart();
            }

        }
        else if (detailSelected) DeselectPart();
    }
    private void TouchMoved(float2 screenpos, float2 dir)
    {
        if (detailSelected)
        {
            var manager = EntityManager;
            var inputRay = GetInputRay(screenpos, MyCollisionLayerExtension.nonSelectedDetailsFilter);
            Unity.Physics.RaycastHit rh;
            float3 pos = float3.zero;
            var physicsSystem = EntityManager.World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
            var collisionWorld = physicsSystem.PhysicsWorld.CollisionWorld;
            if (collisionWorld.CastRay(inputRay, out rh))
            {
                using (var types = manager.GetComponentTypes(rh.Entity, Allocator.Temp)) {
                    if (controlElementTouched && types.Contains(typeof(ControllerElementComponent)))
                    {
                        if (manager.GetComponentData<ControllerElementComponent>(rh.Entity).ID == controlElementID)
                        {
                            changeSystem.ControlMoveTouch(rh);
                            return;
                        }
                    }
                    else
                    {
                        if (changeSystem.editingATM) return;
                        if ( alignment && types.Contains(typeof(LocalToWorld)))
                        {
                            var ltw = EntityManager.GetComponentData<LocalToWorld>(rh.Entity);
                            var worldToLocal = math.inverse(ltw.Value);
                            float4 hitPosition = new float4(rh.Position, 1f); // матрица * вектор, не наоборот
                            var localPosition = math.mul(worldToLocal, hitPosition).xyz;
                            if (math.abs(localPosition.x) < ALIGNMENT_LIMIT) localPosition.x = 0f;
                            if (math.abs(localPosition.y) < ALIGNMENT_LIMIT) localPosition.y = 0f;
                            if (math.abs(localPosition.z) < ALIGNMENT_LIMIT) localPosition.z = 0f;
                            pos = math.mul(ltw.Value, new float4(localPosition, 1f)).xyz;
                            rh.Position = pos;
                        }
                        else pos = rh.Position;
                    }
                }
            }
            else
            {
                if (controlElementTouched)
                {
                    changeSystem.ControlMoveTouch(screenpos);
                    return;
                }
                else pos = inputRay.End;
            }
            var array = manager.CreateEntityQuery(typeof(SelectedObjectMarker), typeof(UsingByEditorMarker), typeof(PartInfoComponent)).ToEntityArray(Allocator.Temp);
            foreach (var e in array)
            {
                manager.SetComponentData(e, new Translation() { Value = pos + manager.GetComponentData<PartInfoComponent>(e).Value.GetExtents() * rh.SurfaceNormal });
            }
            array.Dispose();
        }
    }
    private void TouchEnded(float2 endpos)
    {
        if (controlElementTouched)
        {
            controlElementTouched = false;
            changeSystem.ControlElementStopTouch();
        }
    }

    protected override void OnUpdate()
    {
    }

    public RaycastInput GetInputRay(float2 screenPixelPosition, CollisionFilter cf)
    {
        UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(new Vector3(screenPixelPosition.x, screenPixelPosition.y, 0f));
        return new RaycastInput()
        {
            Start = cameraRay.origin,
            End = cameraRay.origin + cameraRay.direction * RAY_LENGTH,
            Filter = cf
        };
    }
    public bool Raycast(out Unity.Physics.RaycastHit hit, CollisionFilter filter)
    {
        UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        var inputRay = new RaycastInput()
        {
            Start = cameraRay.origin,
            End = cameraRay.origin + cameraRay.direction * RAY_LENGTH,
            Filter = filter
        };

        var physicsSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsSystem.PhysicsWorld.CollisionWorld;
        return collisionWorld.CastRay(inputRay, out hit);
    }

    public void SelectPart(Entity e)
    {
        var manager = EntityManager;
        //previous deselection
        if (detailSelected)
        {
            var array = manager.CreateEntityQuery(typeof(SelectedObjectMarker), typeof(UsingByEditorMarker), typeof(PartInfoComponent)).ToEntityArray(Allocator.Temp);
            if (array.Length == 1 && array[0] == e) return;
            else DeselectPart();
        }
        //
        detailSelected = true;
        manager.AddComponent<SelectedObjectMarker>(e);
        selectedColliderCopy = manager.GetComponentData<PartInfoComponent>(e).Value.LoadMeshCollider();
            // BlobAssetReference<Unity.Physics.Collider>.Create(manager.GetComponentData<PhysicsCollider>(e).Value.Value);
        selectedColliderCopy.Value.Filter = MyCollisionLayerExtension.selectedDetailsFilter;
        manager.SetComponentData(e, new PhysicsCollider() { Value = selectedColliderCopy });
        ChangeMaterial(e, ResourcesMaster.selectedMaterial);
        changeSystem.PartWasSelected();    
    }
    public void DeselectPart()
    {
        var manager = EntityManager;
        var array = manager.CreateEntityQuery(typeof(UsingByEditorMarker), typeof(PartInfoComponent)).ToEntityArray(Allocator.Temp);
        foreach (var e in array)
        {
            ChangeMaterial(e, ResourcesMaster.defaultMaterial);
            manager.SetComponentData(e, new PhysicsCollider() { Value = ResourcesMaster.GetPartCollider(manager.GetComponentData<PartInfoComponent>(e).Value) });
            manager.RemoveComponent<SelectedObjectMarker>(e);
        };
        array.Dispose();
        detailSelected = false;
        changeSystem.PartDeselected();
    }

    private void ChangeMaterial(Entity e, UnityEngine.Material material)
    {
        var rm = EntityManager.GetSharedComponentData<RenderMesh>(e);
        rm.material = material;
        EntityManager.SetSharedComponentData(e, rm);
    }
}
