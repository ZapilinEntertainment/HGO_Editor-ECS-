using Unity.Entities;
using System;
using UnityEngine;
using Unity.Mathematics;

public class TouchControlSystem : SystemBase
{
    public Action<float2> touchEndedEvent, touchStartedEvent;
    public Action<float2, float2> touchMovedEvent;
    private bool touchStarted = false;
    private Vector2 prevpos;
    private const float TOUCH_DELTA_LIMIT = 0.1f;

    protected override void OnUpdate()
    {
        Vector2 pos = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            if (!touchStarted)
            {
                touchStarted = true;
                touchStartedEvent?.Invoke(pos);
                prevpos = pos;
            }
            else
            {
                var d = prevpos - pos;
                if (d.magnitude > TOUCH_DELTA_LIMIT)
                {
                    touchMovedEvent?.Invoke(pos, d);
                    prevpos = pos;
                }
            }
        }
        else
        {
            if (touchStarted && Input.GetMouseButtonUp(0))
            {
                touchEndedEvent?.Invoke(pos);
                touchStarted = false;
            }
        }
    }
}
