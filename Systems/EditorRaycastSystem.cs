using Unity.Physics;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;

public class EditorRaycastSystem : ComponentSystem
{
    public bool castingAvailable = true;
    private bool alignment = true, detailSelected = false, mouseClickedDown = false;
    private ElementsChangeSystem changeSystem;
    public const float RAY_LENGTH = 7f, ALIGNMENT_LIMIT = 0.1f;
    private BlobAssetReference<Unity.Physics.Collider> selectedColliderCopy;

    protected override void OnStartRunning()
    {
        SpawnSystem.SpawnPart(PartType.Cube, float3.zero);
        SpawnSystem.SpawnPart(PartType.Cube, new float3(0.2f,-0.3f,1f));
        SpawnSystem.SpawnPart(PartType.Cube, new float3(0.4f, -0.6f, 2f));
        changeSystem = EntityManager.World.GetExistingSystem<ElementsChangeSystem>();
    }

    protected override void OnUpdate()
    {
        bool clickedAtThisFrame = false;
        if (Input.GetMouseButtonDown(0))
        {
            mouseClickedDown = true;
            clickedAtThisFrame = true;
        }
        else
        {
            if (Input.GetMouseButtonUp(0)) mouseClickedDown = false;
        }
        if (castingAvailable && mouseClickedDown)
        {
            Unity.Physics.RaycastHit rh;

            var manager = EntityManager;
            var physicsSystem = manager.World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
            var collisionWorld = physicsSystem.PhysicsWorld.CollisionWorld;
            if (clickedAtThisFrame)
            {
                if (collisionWorld.CastRay(GetInputRay(CollisionFilter.Default), out rh))
                {
                    var types = manager.GetComponentTypes(rh.Entity, Allocator.Temp);
                    if (types.Contains(typeof(PartInfoComponent))) SelectPart(rh.Entity);
                    else
                    {
                        if (types.Contains(typeof(ControllerElementComponent)))
                        {
                            //controls
                        }                            
                    }
                }
                else
                {
                    if (detailSelected) DeselectPart();
                }
            }
            else
            { // Moving
                if (detailSelected && (math.abs(Input.GetAxis("Mouse X")) > 0.001f || math.abs(Input.GetAxis("Mouse Y")) > 0.001f))
                {
                    float3 pos;
                    var inputRay = GetInputRay(MyCollisionLayerExtension.nonSelectedFilter);
                    if (collisionWorld.CastRay(inputRay, out rh))
                    {
                        if (alignment)
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
                    else pos = inputRay.End;                   

                    var array = manager.CreateEntityQuery(typeof(EditingObjectMarker)).ToEntityArray(Allocator.Temp);
                    foreach (var e in array)
                    {
                        manager.SetComponentData(e, new Translation() { Value = pos + manager.GetComponentData<PartInfoComponent>(e).Value.GetExtents() * rh.SurfaceNormal });
                   }
                   array.Dispose();
                }
            }
        }
    }

    public RaycastInput GetInputRay(CollisionFilter cf)
    {
        UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
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
        if (detailSelected)
        {
            var array = manager.CreateEntityQuery(typeof(EditingObjectMarker)).ToEntityArray(Allocator.Temp);
            if (array.Length == 1 && array[0] == e) return;
            else DeselectPart();
        }
        //
        detailSelected = true;
        manager.AddComponent<EditingObjectMarker>(e);
        selectedColliderCopy = manager.GetComponentData<PartInfoComponent>(e).Value.LoadMeshCollider();
            // BlobAssetReference<Unity.Physics.Collider>.Create(manager.GetComponentData<PhysicsCollider>(e).Value.Value);
        selectedColliderCopy.Value.Filter = MyCollisionLayerExtension.selectedFilter;
        manager.SetComponentData(e, new PhysicsCollider() { Value = selectedColliderCopy });
        ChangeMaterial(e, ResourcesMaster.selectedMaterial);
        changeSystem.Activate(manager.GetComponentData<Translation>(e).Value);    
    }
    private void DeselectPart()
    {
        var manager = EntityManager;
        var array = manager.CreateEntityQuery(typeof(EditingObjectMarker)).ToEntityArray(Allocator.Temp);
        foreach (var e in array)
        {
            ChangeMaterial(e, ResourcesMaster.defaultMaterial);
            manager.SetComponentData(e, new PhysicsCollider() { Value = ResourcesMaster.GetPartCollider(manager.GetComponentData<PartInfoComponent>(e).Value) });
            manager.RemoveComponent<EditingObjectMarker>(e);
        };
        array.Dispose();
        detailSelected = false;
        changeSystem.Disable();
    }

    private void ChangeMaterial(Entity e, UnityEngine.Material material)
    {
        var rm = EntityManager.GetSharedComponentData<RenderMesh>(e);
        rm.material = material;
        EntityManager.SetSharedComponentData(e, rm);
    }
}
