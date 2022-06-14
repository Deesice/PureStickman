using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanInput : ICharacterInput
{
    const int inputDelay = 2;
    readonly CustomJoystick joystick;
    readonly Camera cam;
    readonly Queue<Vector2> deltaPositionQueue = new Queue<Vector2>();
    Vector3 previousMousePos;
    public HumanInput()
    {
        joystick = GameObject.FindObjectOfType<CustomJoystick>(true);
        cam = Camera.main;
    }
    public bool GetAimingPoint(out Vector3 point)
    {
        bool output = true;
        Vector2 translateInfo = Vector2.zero;
        if (Input.mousePresent)
        {
            if (Input.GetMouseButton(0))
            {
                deltaPositionQueue.Enqueue(Input.mousePosition - previousMousePos);
                if (deltaPositionQueue.Count > inputDelay)
                    translateInfo = deltaPositionQueue.Dequeue();
                previousMousePos = Input.mousePosition;
            }
            else
            {
                previousMousePos = Input.mousePosition;
                output = false;
            }
        }
        else
        {
            int i = 0;
            for (; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.rawPosition.x > ScreenHelper.x / 2)
                {
                    if (touch.phase != TouchPhase.Canceled && touch.phase != TouchPhase.Ended)
                    {
                        deltaPositionQueue.Enqueue(touch.deltaPosition);
                        if (deltaPositionQueue.Count > inputDelay)
                            translateInfo = deltaPositionQueue.Dequeue();
                        break;
                    }
                }
            }
            if (i == Input.touchCount)
            {
                deltaPositionQueue.Clear();
                output = false;
            }
        }

        if (output)
        {
            Crosshair.instance.SwitchAim(true);
            Crosshair.instance.Translate(translateInfo);
        }

        var ray = cam.ScreenPointToRay(Crosshair.instance.position);
        point = ExtensionMethods.IntersectionPoint(ray.origin, ray.origin + ray.direction, Vector3.right, new Vector3(0, 100, 0));
        return output;
    }
    public float GetMovementInput()
    {
        if (Input.mousePresent)
            return Input.GetAxis("Horizontal");
        else
            return joystick.Axis.x;
    }

    public void PreUpdate()
    {
    }
}
