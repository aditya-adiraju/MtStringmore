using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object that moves at a starting velocity and accelerates to a max velocity to a point.
///
/// Can be stopped at any arbitrary point.
/// </summary>
/// <remarks>
/// Operates by moving a rigidbody with velocity.
///
/// Not sure if it's possible to just attach a FixedJoint2D due to the amount of custom stuff in PlayerController
/// especially since we need to add a velocity boost on exit.
///
/// Also not sure if we even need the rigidbody, but there was interp/exterp problems without it.
///
/// Also, this may cause issues if at any point the player collides with a wall while the object still keeps moving.
///
/// After all, the moving object doesn't collide with anything (trigger + Kinematic).
///
/// Also, there's shared code among the #cloth branch as well - upon merger of one of the branches, this should be
/// refactored as well.
/// </remarks>
[RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))]
public class AttachableMovingObject : AbstractPlayerInteractable
{
    [SerializeField, Tooltip("Acceleration curve over time, in [0, 1]")]
    private AnimationCurve accelerationCurve;

    [SerializeField, Min(0), Tooltip("Minimum speed")]
    private float minSpeed = 1;

    [SerializeField, Min(0), Tooltip("Maximum speed")]
    private float maxSpeed = 1;

    [SerializeField, Min(0), Tooltip("Acceleration time (seconds)")]
    private float accelerationTime = 1;

    [SerializeField, Tooltip("Additional velocity boost on exit")]
    private Vector2 exitVelBoost = new(10, 10);

    [SerializeField, Min(0), Tooltip("Tolerance after zero velocity exit to keep max velocity")]
    private float exitDelayTime = 0.5f;

    [SerializeField, Tooltip("Player offset on attach")]
    private Vector2 playerOffset = new(0, -1);

    /// <remarks>
    /// Has to be public to allow the editor to modify this without reflection.
    /// </remarks>
    [Tooltip("First position (world space)")]
    public Vector2 firstPosition;

    /// <remarks>
    /// Has to be public to allow the editor to modify this without reflection.
    /// </remarks>
    [Tooltip("Second position (world space)")]
    public Vector2 secondPosition;

    [Tooltip("Path renderer")] public MovingObjectPathRenderer pathRenderer;

    /// <remarks>
    /// I know the new reset logic hasn't been merged in yet,
    /// but we need to save a copy of the enumerator to reset the object later.
    /// </remarks>
    private Coroutine _activeMotion;

    private Rigidbody2D _rigidbody;

    /// <summary>
    /// Saved copy of the previous moving rigidbody velocity since we want a short <see cref="exitDelayTime"/>
    /// where the player would *somehow* still have velocity.
    ///
    /// Physics.
    /// </summary>
    private Vector2 _prevVelocity;

    private PlayerController _player;
    private AudioSource _audioSource;

    /// <inheritdoc />
    public override bool IgnoreGravity => true;

    /// <summary>
    /// Returns the time of the last keyframe.
    /// </summary>
    /// <remarks>
    /// Should be 1, but just in case there's user error, we scale anyways.
    /// </remarks>
    private float LastKeyframeTime
    {
        get
        {
            Keyframe[] keys = accelerationCurve.keys;
            if (keys == null || keys.Length == 0) return 1;
            return keys[^1].time;
        }
    }

    /// <summary>
    /// Returns the distance along the path.
    /// </summary>
    /// <remarks>
    /// Since people may or may not adjust the position in the editor while moving,
    /// this computes the vector projection along the actual path in case someone changes the direction while running.
    /// </remarks>
    private float DistanceAlongPath
    {
        get
        {
            Vector2 direction = secondPosition - firstPosition;
            Vector2 travelled = _rigidbody.position - firstPosition;
            return Vector2.Dot(direction, travelled) / direction.magnitude;
        }
    }

    /// <summary>
    /// Evaluates the velocity at a specific time since motion start.
    /// </summary>
    /// <param name="time">Time since motion start</param>
    /// <returns>Velocity at time</returns>
    private float EvaluateAt(float time)
    {
        return Mathf.Lerp(minSpeed, maxSpeed, accelerationCurve.Evaluate(LastKeyframeTime * time / accelerationTime));
    }


    /// <summary>
    /// Coroutine to move the object according to the motion curve.
    /// </summary>
    /// <returns></returns>
    private IEnumerator MotionCoroutine()
    {
        // i can't guarantee that the end user *won't* change the positions during runtime
        // so I have to check *literally every frame*.
        float time = 0;
        for (Vector2 diff = secondPosition - firstPosition;
             DistanceAlongPath <= diff.magnitude;
             diff = secondPosition - firstPosition)
        {
            yield return new WaitForFixedUpdate();
            // yes, I could use possibly use FixedJoint2D, but I suspect that PlayerController may cause problems
            _rigidbody.velocity = EvaluateAt(time) * diff.normalized;
            _prevVelocity = _rigidbody.velocity;
            time += Time.fixedDeltaTime;
        }

        _rigidbody.position = secondPosition;
        _rigidbody.velocity = Vector2.zero;
        yield return new WaitForSeconds(exitDelayTime);
        _prevVelocity = _rigidbody.velocity;
        _player.StopInteraction(this);
        EndInteract(_player);
        // This does two things:
        //  - Disallows interaction
        //  - Stops the race condition check from happening
        if (_player.CurrentInteractableArea == this) _player.CurrentInteractableArea = null;
    }

    /// <summary>
    /// Starts moving.
    /// </summary>
    private void StartMotion()
    {
        _activeMotion ??= StartCoroutine(MotionCoroutine());
    }

    /// <summary>
    /// Stops moving.
    /// </summary>
    private void StopMotion()
    {
        if (_activeMotion != null) StopCoroutine(_activeMotion);
        _activeMotion = null;
        // defensive check to make sure velocity gets set to 0
        // we only want to set velocity to zero after applying previous velocity.
        if (!ReferenceEquals(_player.ActiveVelocityEffector, this))
        {
            _rigidbody.velocity = Vector2.zero;
        }
    }

    /// <inheritdoc />
    public override void OnPlayerEnter(PlayerController player)
    {
        if (_rigidbody.position == secondPosition)
        {
            // disallow re-attaching if reached
            player.CurrentInteractableArea = null;
        }
    }

    /// <inheritdoc />
    public override void OnPlayerExit(PlayerController player)
    {
    }

    /// <inheritdoc />
    public override Vector2 ApplyVelocity(Vector2 velocity)
    {
        Vector2 vel = _rigidbody.velocity;
        if (_activeMotion == null)
        {
            if (_player.ActiveVelocityEffector is AttachableMovingObject)
            {
                if (!ReferenceEquals(_player.ActiveVelocityEffector, this))
                {
                    Debug.LogWarning("Removing other attachable moving object - this is likely a bug!");
                }

                _player.ActiveVelocityEffector = null;
            }

            vel = _prevVelocity + new Vector2(_player.Direction * exitVelBoost.x, exitVelBoost.y);
            _rigidbody.velocity = Vector2.zero;
        }

        return vel;
    }

    /// <inheritdoc />
    public override void StartInteract(PlayerController player)
    {
        _player = player;
        _audioSource.Play();
        if (_rigidbody.position == secondPosition)
        {
            Debug.LogWarning("Attempted interact when motion was finished.");
            if (player.ActiveVelocityEffector != null)
            {
                Debug.LogWarning("Player has active velocity effector when motion is finished!");
            }

            if (player.CurrentInteractableArea == this)
            {
                player.CurrentInteractableArea = null;
            }
            else
            {
                Debug.LogWarning("Player current interactable area mismatch on interact final position check");
            }

            return;
        }

        player.ActiveVelocityEffector = this;
        _player.transform.position = transform.position + transform.TransformDirection(playerOffset);
        StartMotion();
    }

    /// <inheritdoc />
    public override void EndInteract(PlayerController player)
    {
        _audioSource.Stop();
        StopMotion();
    }
    
    /// <summary>
    /// Called on reset: resets back to the first position.
    /// </summary>
    private void OnReset()
    {
        transform.position = firstPosition;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
        GameManager.Instance.Reset += OnReset;
    }

    private void OnDisable()
    {
        GameManager.Instance.Reset -= OnReset;
    }

    private void OnValidate()
    {
        foreach (Keyframe key in (accelerationCurve?.keys ?? Array.Empty<Keyframe>()))
        {
            if (key.value is < 0 or > 1)
            {
                Debug.LogWarning($"Acceleration curve keyframe is out of range: {key.time}, {key.value}");
            }
        }

        if (!Mathf.Approximately(LastKeyframeTime, 1))
        {
            Debug.LogWarning(
                $"Acceleration curve last keyframe time is not 1; time will be scaled: {LastKeyframeTime}");
        }

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body?.interpolation != RigidbodyInterpolation2D.Interpolate)
        {
            Debug.LogWarning("Rigidbody isn't interpolated: positions may appear invalid while moving!");
        }

        if (!body?.isKinematic ?? false)
        {
            Debug.LogWarning("Rigidbody isn't kinematic: may cause problems!");
        }

        if (body)
        {
            List<RaycastHit2D> hits = new();
            body.position = firstPosition;
            body.Cast((secondPosition - firstPosition).normalized, hits, (secondPosition - firstPosition).magnitude);
            foreach (RaycastHit2D hit in hits)
            {
                // some wack things may happen if the player collides with something while moving
                Debug.LogWarning("Object may be in motion path: " + hit.transform.gameObject.name);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(firstPosition, 1);
        Gizmos.DrawLine(firstPosition, secondPosition);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(secondPosition, 1);
    }
}
