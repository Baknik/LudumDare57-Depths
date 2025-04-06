using UnityEngine;

[RequireComponent(typeof(DistanceJoint2D))]
public class SpelunkerAttachmentPoint : AttachmentPoint
{
    [HideInInspector]
    public DistanceJoint2D DistanceJoint2D;

    private void Awake()
    {
        DistanceJoint2D = GetComponent<DistanceJoint2D>();
    }
}
