using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [Header("Prefabs")]
    public AnchorAttachmentPoint AnchorAttachmentPointPrefab;
    public RopeBendAttachmentPoint RopeBendAttachmentPointPrefab;
    public RopeSegment RopeSegmentPrefab;

    [Header("Scene References")]
    public AnchorAttachmentPoint TreeAttachmentPoint;
    public SpelunkerAttachmentPoint SpelunkerAttachmentPoint;
    public Transform AttachmentPointsParent;
    public Transform RopeSegmentsParent;

    [HideInInspector]
    public RopeSegment ActiveRopeSegment;

    private List<AttachmentPoint> _attachmentPoints;
    private List<RopeSegment> _ropeSegments;

    void Start()
    {
        _attachmentPoints = new List<AttachmentPoint>();
        _attachmentPoints.Add(TreeAttachmentPoint);
        _attachmentPoints.Add(SpelunkerAttachmentPoint);

        _ropeSegments = new List<RopeSegment>();
        var ropeSegment = AddNewRopeSegment(TreeAttachmentPoint, SpelunkerAttachmentPoint);
        ActiveRopeSegment = ropeSegment;
    }

    void Update()
    {
        ActiveRopeSegment.UpdateEndpoints();
    }

    public void AddNewAnchorAttachmentPoint(Vector2 worldPosition)
    {
        var anchorAttachmentPoint = Instantiate(AnchorAttachmentPointPrefab, worldPosition, Quaternion.identity, AttachmentPointsParent);
        _attachmentPoints.RemoveAt(_attachmentPoints.Count - 1);
        _attachmentPoints.Add(anchorAttachmentPoint);
        _attachmentPoints.Add(SpelunkerAttachmentPoint);
        ActiveRopeSegment.EndAttachmentPoint = anchorAttachmentPoint;
        ActiveRopeSegment.UpdateEndpoints();

        SpelunkerAttachmentPoint.DistanceJoint2D.connectedBody = anchorAttachmentPoint.Rigidbody2D;
        var distanceToNewAnchor = Vector2.Distance(SpelunkerAttachmentPoint.transform.position, anchorAttachmentPoint.transform.position);
        var jointDistance = Mathf.Max(1f, distanceToNewAnchor);
        SpelunkerAttachmentPoint.DistanceJoint2D.distance = jointDistance;

        var ropeSegment = AddNewRopeSegment(anchorAttachmentPoint, SpelunkerAttachmentPoint);
        ActiveRopeSegment = ropeSegment;
    }

    private RopeSegment AddNewRopeSegment(AttachmentPoint startAttachmentPoint, AttachmentPoint endAttachmentPoint)
    {
        var ropeSegment = Instantiate(RopeSegmentPrefab, Vector3.zero, Quaternion.identity, RopeSegmentsParent);
        ropeSegment.StartAttachmentPoint = startAttachmentPoint;
        ropeSegment.EndAttachmentPoint = endAttachmentPoint;
        _ropeSegments.Add(ropeSegment);

        return ropeSegment;
    }
}
