using System;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpelunkerAttachmentPoint))]
public class Spelunker : MonoBehaviour
{
    public delegate void SpelunkerTookDamageHandler(int damage, int currentHealth);
    public static event SpelunkerTookDamageHandler SpelunkerTookDamageEvent;

    public delegate void SpelunkerDeathHandler();
    public static event SpelunkerDeathHandler SpelunkerDeathEvent;

    [Header("Scene References")]
    public Rope Rope;
    [Header("Settings")]
    public float SwingForce;
    public float DescendSpeed;
    public float AscendSpeed;
    public float MaxAnchorPlacementSpeed;
    public float MinAnchorPlacementDistance;
    public LayerMask GroundLayerMask;
    public float MinDistanceFromGroundToTouch;
    public int MaxHealth;
    public float ImpactDamageMultiplier;
    public float MinSpeedForImpactDamage;

    private Rigidbody2D _rigidbody2D;
    private CircleCollider2D _circleCollider2D;
    private SpelunkerAttachmentPoint _spelunkerAttachmentPoint;
    private Vector2 _swingForceInput;
    private float _targetDistance;
    private int _health;

    void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _spelunkerAttachmentPoint = GetComponent<SpelunkerAttachmentPoint>();
    }

    private void OnEnable()
    {
        Rope.RopeSegmentAddedEvent += RopeSegmentAddedHandler;
        Rope.LastRopeSegmentRemovedEvent += LastRopeSegmentRemovedHandler;
    }

    private void OnDisable()
    {
        Rope.RopeSegmentAddedEvent -= RopeSegmentAddedHandler;
        Rope.LastRopeSegmentRemovedEvent -= LastRopeSegmentRemovedHandler;
    }

    void Start()
    {
        _health = MaxHealth;

        _targetDistance = _spelunkerAttachmentPoint.DistanceJoint2D.distance;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) &&
            (_rigidbody2D.velocity.magnitude < MaxAnchorPlacementSpeed) &&
            Vector2.Distance(_spelunkerAttachmentPoint.transform.position, Rope.ActiveRopeSegment.StartAttachmentPoint.transform.position) > MinAnchorPlacementDistance)
        {
            Rope.AddNewAnchorAttachmentPoint(this.transform.position + new Vector3(0f, 1f, 0f));
        }

        _swingForceInput = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            _swingForceInput += Vector2.left;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _swingForceInput += Vector2.right;
        }

        var currentDistance = _spelunkerAttachmentPoint.DistanceJoint2D.distance;
        _targetDistance = currentDistance;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            _targetDistance = currentDistance - 1;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            _targetDistance = currentDistance + 1;
        }
    }

    void FixedUpdate()
    {
        _rigidbody2D.AddForce(_swingForceInput * SwingForce * Time.fixedDeltaTime, ForceMode2D.Force);

        var currentDistance = _spelunkerAttachmentPoint.DistanceJoint2D.distance;
        var activeRopeDirectionNormalized = Rope.GetActiveSegmentDirectionNormalized();
        if (_targetDistance > currentDistance)
        {
            var groundCastTowardsDistanceChange = Physics2D.CircleCast(_circleCollider2D.bounds.center, _circleCollider2D.radius, activeRopeDirectionNormalized, 0.1f, GroundLayerMask);
            if (groundCastTowardsDistanceChange.collider == null)
            {
                _spelunkerAttachmentPoint.DistanceJoint2D.distance = Mathf.Lerp(currentDistance, _targetDistance, Time.fixedDeltaTime * DescendSpeed);
            }
            _targetDistance = currentDistance;
        }
        else if (_targetDistance < currentDistance)
        {
            var groundCastTowardsDistanceChange = Physics2D.CircleCast(_circleCollider2D.bounds.center, _circleCollider2D.radius, activeRopeDirectionNormalized * -1f, 0.1f, GroundLayerMask);
            if (groundCastTowardsDistanceChange.collider == null)
            {
                _spelunkerAttachmentPoint.DistanceJoint2D.distance = Mathf.Lerp(currentDistance, _targetDistance, Time.fixedDeltaTime * AscendSpeed);
            }
            _targetDistance = currentDistance;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            var impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed >= MinSpeedForImpactDamage)
            {
                var impactDamage = Mathf.RoundToInt(impactSpeed * ImpactDamageMultiplier);
                _health -= impactDamage;
                _health = Math.Clamp(_health, 0, MaxHealth);

                if (SpelunkerTookDamageEvent != null)
                {
                    SpelunkerTookDamageEvent(impactDamage, _health);
                }

                if (_health <= 0)
                {
                    if (SpelunkerDeathEvent != null)
                    {
                        SpelunkerDeathEvent();
                    }
                }
            }
        }
    }

    private void RopeSegmentAddedHandler(RopeSegment ropeSegment)
    {
        var currentDistance = _spelunkerAttachmentPoint.DistanceJoint2D.distance;
        _targetDistance = currentDistance;
    }

    private void LastRopeSegmentRemovedHandler()
    {
        var currentDistance = _spelunkerAttachmentPoint.DistanceJoint2D.distance;
        _targetDistance = currentDistance;
    }
}
