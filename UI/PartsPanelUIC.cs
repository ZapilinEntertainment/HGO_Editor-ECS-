using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Unity.Entities;
using Unity.Mathematics;

public class PartsPanelUIC : MonoBehaviour, UIControllerInterface, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Transform buttonsHost;
    private int rows = 10, columns = 5;
    private int selectedIndex = -1;
    private bool initialized = false, dragging = false, detailSpawned = false;
    private PartButton[] partsArray;
    private Rect partsWindowRect;
    private EditorRaycastSystem raycastSystem;

    private class PartButton // container
    {
        public PartType partType;
        public GameObject button;

        public void SetHighlight(bool x)
        {
            if (x) button.GetComponent<Image>().color = Color.yellow;
            else button.GetComponent<Image>().color = Color.white;
        }
    }

    private void Start()
    {
        raycastSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<EditorRaycastSystem>();
        Open();
    }

    public void Open()
    {
        if (!initialized)
        {
            partsWindowRect = GetComponent<RectTransform>().rect;

            int count = (int)PartType.TotalCount;
            partsArray = new PartButton[count];
            var exampleButton = buttonsHost.GetChild(0).gameObject;
            for (int i = 0; i < count; i++)
            {
                var b = new PartButton();
                var i_button = Instantiate(exampleButton, buttonsHost);
                var rt = i_button.GetComponent<RectTransform>();
                int x = i % columns;
                float xp = 1f / columns;
                float y = i / rows, yp = 1f / rows;
                rt.anchorMin = new Vector2(x * xp, 1f - (y + 1) * yp);                
                rt.anchorMax = new Vector2((x+1) * xp, 1f - y * yp);
                rt.offsetMax = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                b.button = i_button;
                PrepareButton(b, (PartType)i, i);
                partsArray[i] = b;
            }
            initialized = true;
        }
    }
    private void PrepareButton(PartButton obj, PartType ptype, int index )
    {
        obj.partType = ptype;
        var ob = obj.button;
        var ri = ob.GetComponent<RawImage>();
        ptype.SetIcon(ref ri);
        //
        ob.gameObject.SetActive(true);
    }

    public void Refresh()
    {

    }
    public void Close()
    {
        
    }

    //
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        var ppos = eventData.pressPosition;
        var scale = transform.parent.GetComponent<Canvas>().scaleFactor;
        ppos = new Vector2(ppos.x / (partsWindowRect.width * scale), 1f - ppos.y / (partsWindowRect.height * scale));
        detailSpawned = false;
        if (ppos.x > 1f | ppos.y < 0f)
        {
            dragging = false;
            return;
        }
        else
        {            
            float fy = 1f / rows, fx = 1f / columns;
            selectedIndex = (((int)(ppos.y / fy)) * columns) + (int)(ppos.x / fx);
            if (selectedIndex > -1 && selectedIndex < partsArray.Length) dragging = true;
        }        
    }

    public void OnPointerUp(PointerEventData eventData)
    {            
        if (detailSpawned)
        {            
            detailSpawned = false;
        }
        dragging = false;
    }

    private void Update()
    {
        bool cursorInEditorZone = Input.mousePosition.x >= partsWindowRect.width;        

        if (dragging)
        {
            if (!detailSpawned)
            {
                if (cursorInEditorZone)
                {
                    Unity.Physics.RaycastHit rh;
                    var inputRay = raycastSystem.GetInputRay(MyCollisionLayerExtension.allFilter);
                    var e = SpawnSystem.SpawnPart(partsArray[selectedIndex].partType, inputRay.End );
                    detailSpawned = true;
                    raycastSystem.SelectPart(e);
                }
            }
        }
        raycastSystem.castingAvailable = cursorInEditorZone;
    }
}
