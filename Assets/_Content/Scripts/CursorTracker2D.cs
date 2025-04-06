using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorPosition : MonoBehaviour
{
    public float zOffset = -1f;

    void Update()
    {
        var worldPosition2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPosition2D.z = zOffset;
        transform.position = worldPosition2D;
    }
}
