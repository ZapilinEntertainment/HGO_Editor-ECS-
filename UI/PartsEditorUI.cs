using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using Unity.Mathematics;

public class PartsEditorUI : MonoBehaviour
{
    EditorRaycastSystem raycastSystem;
    [SerializeField] private Transform plane, cube;

    private void Start()
    {
        raycastSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<EditorRaycastSystem>();
    }

    private void Update()
    {
        if (Input.GetKeyDown("k"))
        {
            var rh = raycastSystem.GetInputRay(new float2(Input.mousePosition.x, Input.mousePosition.y), MyCollisionLayerExtension.selectedDetailsFilter);

            
        }
    }
}
