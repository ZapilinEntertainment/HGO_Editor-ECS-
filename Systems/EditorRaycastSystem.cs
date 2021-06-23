using Unity.Physics;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Collections;
using System.Collections.Generic;

public class EditorRaycastSystem : ComponentSystem
{
    public bool castingAvailable = true;
    private bool alignment = true, detailSelected = false, mouseClickedDown = false;
    private float3 cursorPosition;
    public const float RAY_LENGTH = 7f, ALIGNMENT_LIMIT = 0.1f;

    protected override void OnStartRunning()
    {
        SpawnSystem.SpawnPart(PartType.Cube, float3.zero);
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
                if (collisionWorld.CastRay(GetInputRay(MyCollisionLayerExtension.defaultFilter), out rh))
                {
                    SelectPart(rh.Entity);
                }
                else
                {
                    if (detailSelected) DeselectPart();
                }
            }
            else
            { // Moving
                if (detailSelected && (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0))
                {
                    float3 pos;
                    var inputRay = GetInputRay(MyCollisionLayerExtension.editorSpecialFilter_Move);
                    if (collisionWorld.CastRay(inputRay, out rh)) pos = rh.Position;
                    else pos = inputRay.End;
                    var array = manager.CreateEntityQuery(typeof(EditingObjectMarker)).ToEntityArray(Allocator.Temp);
                    foreach (var e in array)
                    {
                        manager.SetComponentData(e, new Translation() { Value = pos });
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
        var pc = manager.GetComponentData<PhysicsCollider>(e);
        pc.Value.Value.Filter = MyCollisionLayerExtension.editorSpecialFilter_Select;
        manager.SetComponentData(e, pc);
        ChangeMaterial(e, ResourcesMaster.selectedMaterial);
    }
    private void DeselectPart()
    {
        var manager = EntityManager;
        var array = manager.CreateEntityQuery(typeof(EditingObjectMarker)).ToEntityArray(Allocator.Temp);
        foreach (var e in array)
        {
            ChangeMaterial(e, ResourcesMaster.defaultMaterial);
            var pc = manager.GetComponentData<PhysicsCollider>(e);
            pc.Value.Value.Filter = MyCollisionLayerExtension.defaultFilter;
            manager.SetComponentData(e, pc);
            manager.RemoveComponent<EditingObjectMarker>(e);
        };
        array.Dispose();
        detailSelected = false;
    }

    private void ChangeMaterial(Entity e, UnityEngine.Material material)
    {
        var rm = EntityManager.GetSharedComponentData<RenderMesh>(e);
        rm.material = material;
        EntityManager.SetSharedComponentData(e, rm);
    }
}
