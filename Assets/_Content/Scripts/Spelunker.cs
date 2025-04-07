using System;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using DG.Tweening;

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
    public SpriteRenderer AnchorPlacementHint;
    public SpriteRenderer SpelunkerSprite;
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
    public float SwingCooldown;
    public Color CannotPlaceAnchorHighlight;
    public Color CanPlaceAnchorHighlight;
    public Vector2 AnchorPlacementOffset;

    [HideInInspector]
    public float TimeOfLastSwing;

    private Rigidbody2D _rigidbody2D;
    private CircleCollider2D _circleCollider2D;
    private SpelunkerAttachmentPoint _spelunkerAttachmentPoint;
    private Vector2 _swingForceInput;
    private float _targetDistance;
    private int _health;
    private bool _inputEnabled;

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
        GoalCavern.GoalReachedEvent += GoalReachedEvent;
    }

    private void OnDisable()
    {
        Rope.RopeSegmentAddedEvent -= RopeSegmentAddedHandler;
        Rope.LastRopeSegmentRemovedEvent -= LastRopeSegmentRemovedHandler;
        GoalCavern.GoalReachedEvent -= GoalReachedEvent;
    }

    void Start()
    {
        _health = MaxHealth;
        _inputEnabled = true;
        TimeOfLastSwing = Time.time - SwingCooldown;
        _targetDistance = _spelunkerAttachmentPoint.DistanceJoint2D.distance;
        _rigidbody2D.WakeUp();
    }

    void Update()
    {
        // Sprite facing
        SpelunkerSprite.flipX = _rigidbody2D.velocity.x > 0f;

        bool canPlaceAnchor = CanPlaceAnchor();

        // Update anchor placement hint
        AnchorPlacementHint.color = canPlaceAnchor ? CanPlaceAnchorHighlight : CannotPlaceAnchorHighlight;

        // Reset constant press (hold) input vars
        var currentDistance = _spelunkerAttachmentPoint.DistanceJoint2D.distance;
        _targetDistance = currentDistance;

        if (_inputEnabled)
        {
            // Check for placing an anchor input
            if (Input.GetKeyDown(KeyCode.Space) && CanPlaceAnchor())
            {
                Rope.AddNewAnchorAttachmentPoint((Vector2)this.transform.position + AnchorPlacementOffset);
            }

            if ((Time.time - TimeOfLastSwing) >= SwingCooldown &&
                _swingForceInput.magnitude <= Mathf.Epsilon)
            {
                // Check for swinging inputs
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    _swingForceInput += Vector2.left;
                }
                else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    _swingForceInput += Vector2.right;
                }
            }

            // Check for acsent/descent inputs
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                _targetDistance = currentDistance - 1;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                _targetDistance = currentDistance + 1;
            }
        }
    }

    void FixedUpdate()
    {
        // Swing force
        if (_swingForceInput.magnitude > 0f)
        {
            _rigidbody2D.AddForce(_swingForceInput * SwingForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
            _swingForceInput = Vector2.zero;
            TimeOfLastSwing = Time.time;
            SpelunkerSprite.transform.DOShakeRotation(0.1f, 45f);
        }

        // Ascend and descend rope
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
        // Wall impact damage
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
                    _inputEnabled = false;
                    if (SpelunkerDeathEvent != null)
                    {
                        SpelunkerDeathEvent();
                    }
                }
            }
        }
    }

    public bool CanPlaceAnchor()
    {
        return (_rigidbody2D.velocity.magnitude < MaxAnchorPlacementSpeed) &&
                Vector2.Distance(_spelunkerAttachmentPoint.transform.position, Rope.ActiveRopeSegment.StartAttachmentPoint.transform.position) > MinAnchorPlacementDistance;
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

    private void GoalReachedEvent()
    {
        _inputEnabled = false;
    }
}
