using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeSegment : MonoBehaviour
{
    [HideInInspector]
    public AttachmentPoint StartAttachmentPoint;
    [HideInInspector]
    public AttachmentPoint EndAttachmentPoint;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public void UpdateEndpoints()
    {
        _lineRenderer.SetPosition(0, StartAttachmentPoint.transform.position);
        _lineRenderer.SetPosition(1, EndAttachmentPoint.transform.position);
    }
}
