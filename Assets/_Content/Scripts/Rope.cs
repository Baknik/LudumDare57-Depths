using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public delegate void RopeSegmentAddedHandler(RopeSegment segment);
    public static event RopeSegmentAddedHandler RopeSegmentAddedEvent;

    public delegate void LastRopeSegmentRemovedHandler();
    public static event LastRopeSegmentRemovedHandler LastRopeSegmentRemovedEvent;

    [Header("Prefabs")]
    public AnchorAttachmentPoint AnchorAttachmentPointPrefab;
    public RopeBendAttachmentPoint RopeBendAttachmentPointPrefab;
    public RopeSegment RopeSegmentPrefab;

    [Header("Scene References")]
    public AnchorAttachmentPoint TreeAttachmentPoint;
    public SpelunkerAttachmentPoint SpelunkerAttachmentPoint;
    public Rigidbody2D SpelunkerRigidbody2D;
    public Transform AttachmentPointsParent;
    public Transform RopeSegmentsParent;

    [Header("Settings")]
    public LayerMask RopeBendCornerLayerMask;
    public float StraightnessRequiredToDetachFromCorner;

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
        ActiveRopeSegment.UpdateEndpoints();
    }

    void Update()
    {
        ActiveRopeSegment.UpdateEndpoints();
    }

    void FixedUpdate()
    {
        // Rope intersects corner, while moving towards corner
        var activeSegmentDirectionNormalized = GetActiveSegmentDirectionNormalized();
        var midRopeRaycastHit = Physics2D.Raycast((Vector2)ActiveRopeSegment.StartAttachmentPoint.transform.position + activeSegmentDirectionNormalized, activeSegmentDirectionNormalized, SpelunkerAttachmentPoint.DistanceJoint2D.distance - 1, RopeBendCornerLayerMask);
        if (midRopeRaycastHit.collider != null)
        {
            var ropeBendCorner = midRopeRaycastHit.collider.gameObject.GetComponent<RopeBendCorner>();
            if (ropeBendCorner != null)
            {
                var isRopeMovingTowardsRopeBendCorner = (SpelunkerRigidbody2D.velocity.x * ropeBendCorner.OutDirection.x) < 0f;
                if (isRopeMovingTowardsRopeBendCorner)
                {
                    AddNewRopeBendAttachmentPoint(midRopeRaycastHit.point, ropeBendCorner);
                }
            }
        }

        // Active segment lines up with previous segment across a rope bend attachment
        if (_ropeSegments.Count > 1 &&
            ActiveRopeSegment.StartAttachmentPoint.GetType() == typeof(RopeBendAttachmentPoint))
        {
            var ropeBendAttachmentPoint = (RopeBendAttachmentPoint)ActiveRopeSegment.StartAttachmentPoint;
            var isRopeMovingAwayFromRopeBendCorner = (SpelunkerRigidbody2D.velocity.x * ropeBendAttachmentPoint.RopeBendCorner.OutDirection.x) >= 0f;
            if (isRopeMovingAwayFromRopeBendCorner)
            {
                var lastThreeAttachmentPointsCollinear =
                ArePointsCollinear(
                    ActiveRopeSegment.EndAttachmentPoint.transform.position,
                    ActiveRopeSegment.StartAttachmentPoint.transform.position,
                    _ropeSegments[_ropeSegments.Count - 2].StartAttachmentPoint.transform.position);

                if (lastThreeAttachmentPointsCollinear)
                {
                    CombineLastTwoRopeSegments();
                }
            }
        }
    }

    private void CombineLastTwoRopeSegments()
    {
        var secondToLastSegment = _ropeSegments[_ropeSegments.Count - 2];
        _attachmentPoints.RemoveAt(_attachmentPoints.Count - 2);
        Destroy(secondToLastSegment.EndAttachmentPoint.gameObject);
        Destroy(_ropeSegments[_ropeSegments.Count - 1].gameObject);
        _ropeSegments.RemoveAt(_ropeSegments.Count - 1);
        secondToLastSegment.EndAttachmentPoint = SpelunkerAttachmentPoint;
        ActiveRopeSegment = secondToLastSegment;
        ActiveRopeSegment.UpdateEndpoints();

        SpelunkerAttachmentPoint.DistanceJoint2D.connectedBody = ActiveRopeSegment.StartAttachmentPoint.Rigidbody2D;
        var distanceToNewAnchor = Vector2.Distance(SpelunkerAttachmentPoint.transform.position, ActiveRopeSegment.StartAttachmentPoint.transform.position);
        SpelunkerAttachmentPoint.DistanceJoint2D.distance = distanceToNewAnchor;

        if (LastRopeSegmentRemovedEvent != null)
        {
            LastRopeSegmentRemovedEvent();
        }
    }

    public void AddNewRopeBendAttachmentPoint(Vector2 worldPosition, RopeBendCorner ropeBendCorner)
    {
        var ropeBendAttachmentPoint = Instantiate(RopeBendAttachmentPointPrefab, worldPosition, Quaternion.identity, AttachmentPointsParent);
        ropeBendAttachmentPoint.RopeBendCorner = ropeBendCorner;
        _attachmentPoints.RemoveAt(_attachmentPoints.Count - 1);
        _attachmentPoints.Add(ropeBendAttachmentPoint);
        _attachmentPoints.Add(SpelunkerAttachmentPoint);
        ActiveRopeSegment.EndAttachmentPoint = ropeBendAttachmentPoint;
        ActiveRopeSegment.UpdateEndpoints();

        SpelunkerAttachmentPoint.DistanceJoint2D.connectedBody = ropeBendAttachmentPoint.Rigidbody2D;
        var distanceToNewAnchor = Vector2.Distance(SpelunkerAttachmentPoint.transform.position, ropeBendAttachmentPoint.transform.position);
        SpelunkerAttachmentPoint.DistanceJoint2D.distance = distanceToNewAnchor;

        var ropeSegment = AddNewRopeSegment(ropeBendAttachmentPoint, SpelunkerAttachmentPoint);
        ActiveRopeSegment = ropeSegment;
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

    public Vector2 GetActiveSegmentDirectionNormalized()
    {
        return (ActiveRopeSegment.EndAttachmentPoint.transform.position - ActiveRopeSegment.StartAttachmentPoint.transform.position).normalized;
    }

    private RopeSegment AddNewRopeSegment(AttachmentPoint startAttachmentPoint, AttachmentPoint endAttachmentPoint)
    {
        var ropeSegment = Instantiate(RopeSegmentPrefab, Vector3.zero, Quaternion.identity, RopeSegmentsParent);
        ropeSegment.StartAttachmentPoint = startAttachmentPoint;
        ropeSegment.EndAttachmentPoint = endAttachmentPoint;
        _ropeSegments.Add(ropeSegment);
        ropeSegment.UpdateEndpoints();

        if (RopeSegmentAddedEvent != null)
        {
            RopeSegmentAddedEvent(ropeSegment);
        }

        return ropeSegment;
    }

    private bool ArePointsCollinear(Vector2 a, Vector2 b, Vector2 c)
    {
        // Calculate the vectors AB and AC
        Vector2 ab = b - a;
        Vector2 ac = c - a;

        // Calculate the cross product (z-component)
        float crossProduct = ab.x * ac.y - ab.y * ac.x;

        // Check if the cross product is close to zero (due to floating-point precision)
        return Mathf.Abs(crossProduct) < StraightnessRequiredToDetachFromCorner;
    }
}
