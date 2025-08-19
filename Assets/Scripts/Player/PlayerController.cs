using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interactables;
using Managers;
using StringmoreCamera;
using UnityEngine;
using Util;

namespace Player
{
    /// <summary>
    /// Controls player movement and invokes events for different player states
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(AudioSource), typeof(CapsuleCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        #region Serialized Private Fields
    
        // @formatter:off
        [Header("Input")]
        [SerializeField] private bool doubleJumpEnabled;
        [SerializeField] private bool dashEnabled;
        [SerializeField] private float buttonBufferTime;
        [Header("Collisions")] 
        [SerializeField] private LayerMask collisionLayer;
        [SerializeField] private float collisionDistance;
        [SerializeField] private float wallCloseDistance;
        [Header("Ground")]
        [SerializeField] private float maxGroundSpeed;
        [SerializeField] private float groundAcceleration;
        [SerializeField] private float startDirection;
        [SerializeField] private float groundingForce;
        [Header("Jump")]
        [SerializeField] private float jumpPower;
        [SerializeField] private float doubleJumpPower;
        [SerializeField] private float coyoteTime;
        [Header("Air")]
        [SerializeField] private float maxFallSpeed;
        [SerializeField] private float fallAccelerationUp;
        [SerializeField] private float fallAccelerationDown;
        [SerializeField] private float earlyReleaseFallAcceleration;
        [SerializeField] private float maxAirSpeed;
        [SerializeField] private float airAcceleration;
        [Header("Dash")] 
        [SerializeField] private float dashTime;
        [SerializeField] private float dashSpeed;
        [SerializeField] private float endDashSpeed;
        [SerializeField, Min(0), Tooltip("allow noclip (sec) even after dash ends")] private float dashEndTolerance = 1;
        [Header("Wall")] 
        [SerializeField] private float wallJumpAngle;
        [SerializeField] private float wallJumpPower;
        [SerializeField] private float wallSlideSpeed;
        [SerializeField] private float wallSlideAcceleration;
        [Header("Swinging")] 
        [SerializeField] private float swingBoostMultiplier;
        [SerializeField] private float maxSwingSpeed;
        [SerializeField] private float swingAcceleration;
        [SerializeField] private float minSwingReleaseX;
        [SerializeField] private bool enableSwingHacks = true;
        [SerializeField] private float maxHackAcceleration = 200;
        [SerializeField, Min(0)] private float hackVelocityRatio = 0.8f;
        [SerializeField, Range(0, Mathf.PI), Tooltip("Maximum angle from bottom to apply hacky additional acceleration")]
        private float hackRange = Mathf.PI/6;
        [Header("Visual")]
        [SerializeField, Min(0)] private float runParticleVelocityThreshold = 0.1f;
        [SerializeField] private LineRenderer ropeRenderer;
        [SerializeField] private float deathTime;
        // this is just here for battle of the concepts
        [Header("Temporary")]
        [SerializeField] private GameObject poofSmoke;
        [Header("Audio")]
        [SerializeField] private AudioClip swingAttach;
        [SerializeField] private AudioClip swingDetach;
        [SerializeField] private AudioClip[] swingChangeDirection;
        [Header("Dashing Camera Shake")]
        [SerializeField] private float shakeDuration;
        [SerializeField] private float shakeIntensity;
        [SerializeField, Tooltip("Set to true for shake along x-axis")] private bool xShake;
        [SerializeField, Tooltip("Set to true for shake along y-axis")] private bool yShake;
        [SerializeField, Tooltip("Set to true if object is destructible")] private bool destructibleObject;
        // @formatter:on

        #endregion

        #region Public Properties and Actions

        public enum PlayerStateEnum
        {
            Run,
            Air,
            LeftWallSlide,
            RightWallSlide,
            Dead,
            Swing,
            Dash,
            OnObject
        }

        // TODO: PlayerState set should be a function that fires an action
        public PlayerStateEnum PlayerState { get; private set; }

        /// <summary>
        /// Current velocity of the player.
        /// </summary>
        public Vector2 Velocity => _velocity;

        /// <summary>
        /// Facing direction of the player. -1.0 for left, 1.0 for right.
        /// </summary>
        public float Direction { get; private set; }

        /// <summary>
        /// Time when the most recent dash would have ended (may be a time in the future).
        /// </summary>
        public float TimeDashEnded { get; private set; } = float.MinValue;

        /// <summary>
        /// Public getter for dash end tolerance.
        /// </summary>
        public float DashEndTolerance => dashEndTolerance;

        /// <summary>
        /// Fires when the player becomes grounded or leaves the ground.
        /// Parameters:
        ///     bool: false if leaving the ground, true if becoming grounded
        ///     float: player's Y velocity
        /// </summary>
        public event Action<bool, float> GroundedChanged;

        /// <summary>
        /// Fires when the player hits the wall or leaves the wall.
        /// Parameters:
        ///     bool: True if hitting the wall, false if leaving the wall
        /// </summary>
        public event Action<bool> WallChanged;

        public event Action Jumped;
        public event Action Dashed;

        /// <summary>
        /// Fires when the player hangs onto an interactable object.
        /// Parameters:
        ///     bool (hanging): True when player attaches, false when player lets go
        ///     bool (facingLeft): True when facing left, false otherwise
        /// </summary>
        public event Action<bool, bool> HangChanged;

        public event Action DoubleJumped;
        public event Action Death;
        public event Action<Vector2> OnSwingStart;
        public event Action<bool> SwingDifferentDirection;

        /// <summary>
        /// If true, skips death logic.
        /// </summary>
        public bool DebugIgnoreDeath { get; set; }

        /// <summary>
        /// Current interactable in range.
        /// </summary>
        public AbstractPlayerInteractable CurrentInteractableArea { get; set; }

        /// <summary>
        /// Whether the player can use their dash in the air.
        /// </summary>
        public bool CanDash
        {
            get => _canDash;
            set => _canDash = value;
        }

        /// <summary>
        /// Current ground Y level; null if none.
        /// </summary>
        public float? CurrentGroundY { get; private set; }

        #endregion

        #region Private Properties

        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private AudioSource _audioSource;
        private IPlayerVelocityEffector _activeEffector;
        private ParticleSystem _runningDust;
        private ParticleSystem _landingDust;
        private ParticleSystem _leftWallSlideDust;
        private ParticleSystem _rightWallSlideDust;

        private float _time;
        private float _timeButtonPressed;
        private float _timeLeftGround;
        private float _timeDashed;

        private bool _buttonNotPressedPreviousFrame;
        private bool _isButtonHeld;
        private bool _canReleaseEarly;
        private bool _releasedEarly;
        private bool _canDoubleJump;
        private bool _canDash;

        private Vector2 _velocity;
        private bool _closeToWall;
        private float? _groundY;
        private Vector2 _groundNormal;

        private readonly List<IPlayerVelocityEffector> _playerVelocityEffectors = new();
        private readonly List<IPlayerVelocityEffector> _impulseVelocityEffectors = new();
        private Collider2D _swingArea;
        private float _swingRadius;
        private bool _canSwing;
        private bool _swingStarted;
        private bool _wasSwingClockwise;
        private ShakeCamera _shake;
        private Coroutine _dashNoclipHax;
        #endregion

        #region Unity Event Handlers

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _audioSource = GetComponent<AudioSource>();
            _shake = FindObjectOfType<ShakeCamera>();
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                switch (ps.gameObject.name)
                {
                    case "RunningDust":
                        _runningDust = ps;
                        break;
                    case "LandingDust":
                        _landingDust = ps;
                        break;
                    case "LeftWallSlidingDust":
                        _leftWallSlideDust = ps;
                        break;
                    case "RightWallSlidingDust":
                        _rightWallSlideDust = ps;
                        break;
                }
            }

            _buttonNotPressedPreviousFrame = true;
            Direction = startDirection;
            GameManager.Instance.RespawnFacingLeft = startDirection < 0.0f;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("LetterBlock"), false);
            GameManager.Instance.Reset += OnReset;
        }

        private void Update()
        {
            _time += Time.deltaTime;

            GetInput();
            RedrawRope(); // TODO this should be moved outside player controller when knitby is real
        }

        private void OnDestroy()
        {
            GameManager.Instance.Reset -= OnReset;
        }

        private void GetInput()
        {
            if (InputUtil.StartJumpOrTouch())
            {
                _timeButtonPressed = _time;
                _buttonNotPressedPreviousFrame = false;
            }

            _isButtonHeld = InputUtil.HoldJumpOrTouch();

            if (Input.GetButtonDown("Debug Reset") && !ResultsManager.isResultsPageOpen)
            {
                GameManager.Instance.Respawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("SwingArea"))
            {
                _swingArea = other;
                _swingRadius = _swingArea.GetComponent<SwingArea>().swingRadius;
                _swingRadius *= _swingArea.transform.lossyScale.x; // assume global scale is same for every dimension
                _canSwing = true;
            }
            else if (other.gameObject.CompareTag("Death"))
            {
                TryKill();
            }
        }

        private void OnParticleCollision(GameObject other)
        {
            if (other.CompareTag("Death"))
            {
                TryKill();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("SwingArea"))
            {
                // can't swing if outside swing area
                // assumes swing areas are not overlapping
                _canSwing = false;
            }
        }

        private void FixedUpdate()
        {
            if (PlayerState == PlayerStateEnum.Dead)
                return;

            CheckCollisions();
            HandleSwing();
            HandleInteractables();
            HandleWallJump();
            HandleJump();
            if (doubleJumpEnabled) HandleDoubleJump();
            HandleEarlyRelease();
            HandleWalk();
            HandleGravity();
            _velocity = _playerVelocityEffectors
                .Aggregate(_velocity, (initial, effector) => effector.ApplyVelocity(initial));
            _velocity = _impulseVelocityEffectors
                .Aggregate(_velocity, (initial, effector) => effector.ApplyVelocity(initial));
            bool hadVelocityEffectors = _playerVelocityEffectors.Count > 0 || _impulseVelocityEffectors.Count > 0;
            _impulseVelocityEffectors.Clear();
            if (!hadVelocityEffectors && dashEnabled) HandleDash();
            ApplyMovement();
        }

        #endregion

        public bool HasPlayerVelocityEffector(IPlayerVelocityEffector playerVelocityEffector)
        {
            return _playerVelocityEffectors.Contains(playerVelocityEffector) ||
                   _impulseVelocityEffectors.Contains(playerVelocityEffector);
        }

        public void AddPlayerVelocityEffector(IPlayerVelocityEffector effector, bool impulse = false)
        {
            IPlayerVelocityEffector ignores =
                _impulseVelocityEffectors.FirstOrDefault(effect => effect.IgnoreOtherEffectors);
            ignores ??= _playerVelocityEffectors.FirstOrDefault(effect => effect.IgnoreOtherEffectors);
            if (ignores != null)
            {
                Debug.LogWarning(
                    $"Ignoring effector {effector} as existing effector {ignores} ignores other effectors");
                return;
            }

            (impulse ? _impulseVelocityEffectors : _playerVelocityEffectors).Add(effector);
        }

        public void RemovePlayerVelocityEffector(IPlayerVelocityEffector effector)
        {
            _playerVelocityEffectors.Remove(effector);
            _impulseVelocityEffectors.Remove(effector);
        }

        /// <summary>
        /// Stops interacting with the interactable.
        /// </summary>
        /// <param name="interactable">Requesting interactable, only used to check activity.</param>
        /// <remarks>
        /// We could possibly remove the parameter since it's only used for a precondition check,
        /// but I've left it here in case I screwed up somewhere.
        /// </remarks>
        public void StopInteraction(AbstractPlayerInteractable interactable)
        {
            Debug.Assert(CurrentInteractableArea == interactable,
                $"Requested to stop interaction on inactive interactable: {interactable} vs Current {CurrentInteractableArea}");
            _buttonNotPressedPreviousFrame = true;
            CurrentInteractableArea.EndInteract(this);
            PlayerState = PlayerStateEnum.Air;
            HangChanged?.Invoke(false, _velocity.x < 0);
        }

        /// <summary>
        /// Stops dashing if the player is currently dashing.
        /// </summary>
        public void ForceCancelDash()
        {
            if (PlayerState != PlayerStateEnum.Dash)
            {
                Debug.LogWarning("ForceCancelDash called but PlayerState is not Dash");
                return;
            }

            PlayerState = PlayerStateEnum.Air;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("LetterBlock"), false);
            TimeDashEnded = Time.time;
        }

        /// <summary>
        /// Disallows early release for this jump.
        /// </summary>
        public void ForceCancelEarlyRelease()
        {
            _canReleaseEarly = false;
            _releasedEarly = false;
        }

        /// <summary>
        /// Tries to kill the player.
        /// </summary>
        /// <returns>True if successfully killed; false if already being killed</returns>
        public void TryKill()
        {
            if (PlayerState != PlayerStateEnum.Dead) HandleDeath();
        }

        /// <summary>
        /// Force kills the player.
        /// </summary>
        public void ForceKill()
        {
            HandleDeath();
        }

        #region Private Methods

        private RaycastHit2D CapsuleCastCollision(Vector2 direction, float distance)
        {
            return Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0,
                direction, distance, collisionLayer);
        }

        private void CheckCollisions()
        {
            RaycastHit2D groundCast = CapsuleCastCollision(Vector2.down, collisionDistance);
            bool groundHit = groundCast;
            bool ceilingHit = CapsuleCastCollision(Vector2.up, collisionDistance);
            bool leftWallHit = CapsuleCastCollision(Vector2.left, collisionDistance);
            bool rightWallHit = CapsuleCastCollision(Vector2.right, collisionDistance);
            RaycastHit2D wallCloseHit = CapsuleCastCollision(_velocity, wallCloseDistance);
            if (wallCloseHit && wallCloseHit.transform.CompareTag("LetterBlock"))
                _closeToWall = false;
            else
                _closeToWall = wallCloseHit;

            if (groundCast) _groundNormal = groundCast.normal;
            CurrentGroundY = groundCast ? groundCast.point.y : null;

            if (ceilingHit) _velocity.y = Mathf.Min(0, _velocity.y);

            // TODO is this being fired constantly while on a wall?
            if (leftWallHit || rightWallHit)
                WallChanged?.Invoke(true);
            else
                WallChanged?.Invoke(false);

            if (PlayerState == PlayerStateEnum.Run && !groundHit)
            {
                PlayerState = PlayerStateEnum.Air;
                _timeLeftGround = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            if (PlayerState == PlayerStateEnum.Air)
            {
                if (leftWallHit) PlayerState = PlayerStateEnum.LeftWallSlide;
                if (rightWallHit) PlayerState = PlayerStateEnum.RightWallSlide;
            }

            if (PlayerState != PlayerStateEnum.Run && groundHit)
            {
                if (PlayerState == PlayerStateEnum.Dash)
                {
                    if (_dashNoclipHax != null) StopCoroutine(_dashNoclipHax);
                    _dashNoclipHax = StartCoroutine(DashNoClipHaxRoutine());
                    TimeDashEnded = Time.time;
                }
                PlayerState = PlayerStateEnum.Run;
                _canDoubleJump = true;
                _canDash = true;
                GroundedChanged?.Invoke(true, Mathf.Abs(_velocity.y));
                _landingDust.Play();
            }

            if (PlayerState == PlayerStateEnum.RightWallSlide && !rightWallHit ||
                PlayerState == PlayerStateEnum.LeftWallSlide && !leftWallHit)
                PlayerState = PlayerStateEnum.Air;

            UpdateParticleSystemState(_runningDust, PlayerStateEnum.Run,
                () => Mathf.Abs(_rb.velocity.x) > runParticleVelocityThreshold);
            UpdateParticleSystemState(_leftWallSlideDust, PlayerStateEnum.LeftWallSlide);
            UpdateParticleSystemState(_rightWallSlideDust, PlayerStateEnum.RightWallSlide);
        }

        private void UpdateParticleSystemState(ParticleSystem system, PlayerStateEnum targetState,
            Func<bool> optionalPredicate = null)
        {
            if (PlayerState == targetState && (optionalPredicate == null || optionalPredicate()))
            {
                if (!system.isPlaying) system.Play();
            }
            else if (system.isPlaying)
            {
                system.Stop();
            }
        }

        private void HandleDeath()
        {
            if (DebugIgnoreDeath) return;
            _velocity = Vector2.zero;
            _rb.velocity = _velocity;
            PlayerState = PlayerStateEnum.Dead;
            Death?.Invoke();
            Invoke(nameof(Respawn), deathTime);
        }

        private void Respawn()
        {
            GameManager.Instance.Respawn();
        }

        private void HandleWallJump()
        {
            if (IsButtonUsed() && PlayerState is PlayerStateEnum.LeftWallSlide or PlayerStateEnum.RightWallSlide)
            {
                _velocity = new Vector2(Mathf.Cos(wallJumpAngle), Mathf.Sin(wallJumpAngle)) * wallJumpPower;
                if (PlayerState == PlayerStateEnum.RightWallSlide) _velocity.x = -_velocity.x;

                _buttonNotPressedPreviousFrame = true;
                PlayerState = PlayerStateEnum.Air;
                _canReleaseEarly = false;
                _releasedEarly = false;
                _canDoubleJump = true;
                Jumped?.Invoke();
                GroundedChanged?.Invoke(false, Mathf.Abs(_velocity.y)); // TODO need a new action for wj
            }
        }

        private void HandleJump()
        {
            bool canUseCoyote = PlayerState == PlayerStateEnum.Air && _time - _timeLeftGround <= coyoteTime;
            if ((PlayerState == PlayerStateEnum.Run || canUseCoyote) && IsButtonUsed())
            {
                _velocity.y = jumpPower;
                _buttonNotPressedPreviousFrame = true;
                PlayerState = PlayerStateEnum.Air;
                _canReleaseEarly = true;
                Jumped?.Invoke();
                GroundedChanged?.Invoke(false, Mathf.Abs(_velocity.y));
            }
        }

        private void HandleDoubleJump()
        {
            if (IsButtonUsed() && _canDoubleJump && !_closeToWall)
            {
                _velocity.y = doubleJumpPower;
                _buttonNotPressedPreviousFrame = true;
                _canDoubleJump = false;
                _canReleaseEarly = true;
                DoubleJumped?.Invoke();
            }
        }

        /// <summary>
        /// Coroutine to enable physics collision after a delay with letter blocks.
        /// </summary>
        /// <returns>Coroutine</returns>
        private IEnumerator DashNoClipHaxRoutine()
        {
            yield return new WaitForSeconds(dashEndTolerance);
            if (_dashNoclipHax == null) yield break;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("LetterBlock"), false);
            _dashNoclipHax = null;
        }

        private void HandleDash()
        {
            if (PlayerState is PlayerStateEnum.Air && IsButtonUsed() && _canDash && !_closeToWall)
            {
                // start a dash
                _canDash = false;
                _buttonNotPressedPreviousFrame = true;
                PlayerState = PlayerStateEnum.Dash;
                _shake?.Shake(shakeDuration, shakeIntensity, xShake, yShake,destructibleObject);
                Dashed?.Invoke();
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("LetterBlock"), true);
                if (_dashNoclipHax != null)
                {
                    StopCoroutine(_dashNoclipHax);
                    _dashNoclipHax = null;
                }
                _timeDashed = _time;
                TimeDashEnded = Time.time + dashTime;
            }
            else if (PlayerState is PlayerStateEnum.Dash)
            {
                // move player forward at dash speed
                _velocity.y = 0;
                _velocity.x = dashSpeed * Direction;
                // check if dash is over
                if (_time - _timeDashed >= dashTime)
                {
                    PlayerState = PlayerStateEnum.Air;
                    if (_dashNoclipHax != null) StopCoroutine(_dashNoclipHax);
                    _dashNoclipHax = StartCoroutine(DashNoClipHaxRoutine());
                    _velocity.x = endDashSpeed * Direction;
                    TimeDashEnded = Time.time;
                }
            }
            else if (PlayerState is PlayerStateEnum.Run or PlayerStateEnum.Swing)
            {
                // can dash after landing on the ground or swinging
                _canDash = true;
            }
        }

        private void HandleEarlyRelease()
        {
            if (!_canReleaseEarly) return;

            if (!_isButtonHeld) _releasedEarly = true;
            if (_velocity.y < 0f)
            {
                _canReleaseEarly = false;
                _releasedEarly = false;
            }
        }

        private void HandleWalk()
        {
            switch (PlayerState)
            {
                case PlayerStateEnum.Run:
                    // rotate ground normal vector 90 degrees towards facing direction
                    Vector2 walkTarget = new Vector2(_groundNormal.y * Direction, _groundNormal.x * -Direction) *
                                         maxGroundSpeed;
                    float newX = Mathf.MoveTowards(_velocity.x, walkTarget.x, groundAcceleration * Time.fixedDeltaTime);
                    float newY = walkTarget.y;
                    _velocity = new Vector2(newX, newY);
                    break;
                case PlayerStateEnum.Air:
                    if (Mathf.Abs(_velocity.x) < maxAirSpeed)
                        _velocity.x = Mathf.MoveTowards(_velocity.x, maxAirSpeed * Direction,
                            airAcceleration * Time.fixedDeltaTime);
                    break;
            }
        }

        private void HandleGravity()
        {
            if (_playerVelocityEffectors.Any(effector => effector.IgnoreGravity)) return;
            switch (PlayerState)
            {
                case PlayerStateEnum.Run:
                    if (_velocity.y <= 0f)
                        _velocity.y = -groundingForce;
                    break;
                case PlayerStateEnum.LeftWallSlide:
                case PlayerStateEnum.RightWallSlide:
                    _velocity.x = 0f;
                    if (_velocity.y <= 0f)
                        _velocity.y = Mathf.MoveTowards(_velocity.y, -wallSlideSpeed,
                            wallSlideAcceleration * Time.fixedDeltaTime);
                    else goto case PlayerStateEnum.Air;
                    break;
                case PlayerStateEnum.Air:
                case PlayerStateEnum.Swing:
                    float accel;
                    if (_releasedEarly) accel = earlyReleaseFallAcceleration;
                    else if (_velocity.y >= 0f) accel = fallAccelerationUp;
                    else accel = fallAccelerationDown;

                    _velocity.y = Mathf.MoveTowards(_velocity.y, -maxFallSpeed, accel * Time.fixedDeltaTime);
                    break;
            }
        }

        /// <summary>
        /// Handle logic of interactable objects.
        /// </summary>
        private void HandleInteractables()
        {
            if (!CurrentInteractableArea || !CurrentInteractableArea.CanInteract) return;
            bool previouslyGrounded = PlayerState == PlayerStateEnum.Run;
            if (IsButtonUsed())
            {
                // IsButtonUsed can be true multiple frames so don't re-call StartInteract jic
                if (PlayerState == PlayerStateEnum.OnObject) return;
                _buttonNotPressedPreviousFrame = true;
                PlayerState = PlayerStateEnum.OnObject;
                HangChanged?.Invoke(true, _velocity.x < 0);
                CurrentInteractableArea.StartInteract(this);
                if (previouslyGrounded)
                {
                    _timeLeftGround = _time;
                    GroundedChanged?.Invoke(false, 0);
                }
            }

            // fix for occasional race condition where state changes after moving sometimes
            // (such as due to weird quirks in how unity persists collision data for 1 additional tick)
            // thus setting the state to Run/Air without re-setting the player object state
            // I suspect the swing may have this problem but who puts the swing in contact with the ground?
            // should we just have the grounded check first check if you're on an object?
            if (_playerVelocityEffectors.Contains(CurrentInteractableArea) && PlayerState != PlayerStateEnum.OnObject)
            {
                Debug.LogWarning(
                    $"_playerVelocityEffectors.Contains(CurrentInteractableArea) while State != PlayerStateEnum.OnObject, actual state: {PlayerState}");
                PlayerState = PlayerStateEnum.OnObject;

                // not sure if it's guaranteed for the player to not be on the ground when on the object
                // (e.g. if we build objects that push the player along the ground or something like that)
                // but this is required to stop the footsteps from playing.
                if (previouslyGrounded)
                {
                    _timeLeftGround = _time;
                    GroundedChanged?.Invoke(false, 0);
                }
            }

            if (!IsButtonUsed() && PlayerState == PlayerStateEnum.OnObject && !_isButtonHeld)
            {
                StopInteraction(CurrentInteractableArea);
            }
        }

        private void HandleSwing()
        {
            if (_canSwing && IsButtonUsed() && GameManager.Instance.AreInteractablesEnabled)
            {
                // in swing area, button pressed
                PlayerState = PlayerStateEnum.Swing;
                _audioSource.clip = swingAttach;
                _audioSource.Play();
                ropeRenderer.enabled = true;
                HangChanged?.Invoke(true, _velocity.x < 0);
            }
            else if (PlayerState is PlayerStateEnum.Swing && _isButtonHeld)
            {
                // in swing and holding down button
                // if not at max radius yet, fall normally
                if (!_swingStarted &&
                    Vector2.Distance(_swingArea.transform.position, transform.position) >= _swingRadius)
                {
                    // reached max radius, start swing
                    _swingStarted = true;
                    OnSwingStart?.Invoke(_swingArea.transform.position);
                    Vector2 relPos = transform.position - _swingArea.transform.position;
                    _wasSwingClockwise = Vector3.Cross(relPos, _velocity).z > 0f;
                }

                if (_swingStarted)
                {
                    // swinging at max radius
                    Vector2 relPos = transform.position - _swingArea.transform.position;
                    // if going down, accelerate to target swing speed
                    if (_velocity.y <= 0f && _velocity.magnitude <= maxSwingSpeed)
                    {
                        _velocity = _velocity.normalized * Mathf.MoveTowards(_velocity.magnitude, maxSwingSpeed,
                            swingAcceleration * Time.fixedDeltaTime);
                    }

                    // for some reason we wanna arbitrarily give the player max velocity
                    // however we can't use an entirely energy based system to determine their velocity
                    // since... well our system doesn't really obey physics
                    // (gravity literally has different accelerations depending on direction)
                    // so... we just arbitrarily accelerate to some ratio of the max swing speed
                    // why a ratio? because 0.5v^2 > gh at max swing speed but it works since
                    // energy gets sent to the shadow realm in the final adjustment step (testPos/newPos bs)
                    // a ratio of 0.8 seems to work decently well though
                    if (_velocity.magnitude <= maxSwingSpeed && enableSwingHacks)
                    {
                        Vector2 playerDir = relPos.normalized;
                        float angleToVertical = Mathf.Acos(Vector2.Dot(playerDir, Vector2.down));
                        if (angleToVertical < hackRange)
                        {
                            float currentAngularVelocity = _velocity.magnitude / (2 * Mathf.PI * Mathf.Max(playerDir.magnitude, _swingRadius));
                            Vector2 targetDir = new((_wasSwingClockwise ? -1 : 1) * Mathf.Sin(hackRange),
                                -Mathf.Cos(hackRange));
                            float angleToTarget = Mathf.Acos(Vector2.Dot(playerDir, targetDir));
                            float targetSwingSpeed = maxSwingSpeed * hackVelocityRatio;
                            // past the bottom, but we still may want to apply acceleration
                            if (angleToTarget < hackRange)
                            {
                                // prevent reaching higher than it was with max swing speed
                                float yDistFromBottom = relPos.y + relPos.magnitude;
                                float maxKinEnergy = 0.5f * maxSwingSpeed * maxSwingSpeed;
                                float curGravEnergy = fallAccelerationUp * yDistFromBottom;
                                targetSwingSpeed = Mathf.Sqrt(2 * (maxKinEnergy - curGravEnergy));
                            }

                            float timeToVertical = angleToTarget / currentAngularVelocity;
                            float newAccel = Mathf.Min(targetSwingSpeed / timeToVertical, maxHackAcceleration);
                            float newVelocityMag = Mathf.MoveTowards(_velocity.magnitude, targetSwingSpeed,
                                newAccel * Time.fixedDeltaTime);
                            Vector2 desiredDirection = -Vector2.Perpendicular(playerDir);
                            if (_wasSwingClockwise) desiredDirection *= -1;
                            _velocity = desiredDirection * newVelocityMag;
                        }
                    }

                    Vector2 testPos = relPos + _velocity * Time.fixedDeltaTime;
                    Vector2 newPos = testPos.normalized * _swingRadius;
                    _velocity = Vector2.ClampMagnitude((newPos - relPos) / Time.fixedDeltaTime, maxSwingSpeed);
                    if (_wasSwingClockwise ^ Vector3.Cross(relPos, _velocity).z > 0f)
                    {
                        SwingDifferentDirection?.Invoke(_wasSwingClockwise);
                        _wasSwingClockwise = !_wasSwingClockwise;
                        _audioSource.clip = RandomUtil.SelectRandom(swingChangeDirection);
                        _audioSource.Play();
                    }
                }
            }
            else if (PlayerState is PlayerStateEnum.Swing && !_isButtonHeld)
            {
                // swinging but button is released
                // stop swinging, disallow swing until reentering area
                PlayerState = PlayerStateEnum.Air;
                ropeRenderer.enabled = false;
                _canSwing = false;
                _audioSource.clip = swingDetach;
                _audioSource.Play();
                HangChanged?.Invoke(false, _velocity.x < 0);

                if (_swingStarted)
                {
                    // give x velocity boost on release
                    float boostDirection = transform.position.x >= _swingArea.transform.position.x ? 1f : -1f;
                    if (Mathf.Abs(_velocity.x) <= minSwingReleaseX)
                        _velocity.x = minSwingReleaseX * boostDirection;
                    _buttonNotPressedPreviousFrame = true;
                    _swingStarted = false;
                }
            }
        }

        private void RedrawRope()
        {
            if (PlayerState == PlayerStateEnum.Swing)
            {
                ropeRenderer.positionCount = 2;
                ropeRenderer.SetPosition(0, transform.position);
                ropeRenderer.SetPosition(1, _swingArea.transform.position);
            }
        }

        private void ApplyMovement()
        {
            _rb.velocity = _velocity;
            if (_velocity.x != 0f) Direction = Mathf.Sign(_velocity.x);
            //Debug.DrawRay(transform.position, _velocity, Color.magenta);
        }

        private bool IsButtonUsed()
        {
            return !_buttonNotPressedPreviousFrame && _time <= _timeButtonPressed + buttonBufferTime;
        }

        #endregion

        /// <summary>
        /// On reset, respawn at checkpoint with the direction we faced at that checkpoint
        /// </summary>
        private void OnReset()
        {
            Vector2 checkpointPos = GameManager.Instance.CheckPointPos;
            Vector3 spawnPos = new(checkpointPos.x, checkpointPos.y, transform.position.z);
            transform.position = spawnPos;
            Direction = GameManager.Instance.RespawnFacingLeft ? -1.0f : 1.0f;
            if (CurrentInteractableArea)
            {
                CurrentInteractableArea.EndInteract(this);
                CurrentInteractableArea = null;
            }

            if (PlayerState == PlayerStateEnum.OnObject)
            {
                PlayerState = PlayerStateEnum.Air;
            }

            _playerVelocityEffectors.Clear();
            _impulseVelocityEffectors.Clear();
            ropeRenderer.enabled = false;
            PlayerState = PlayerStateEnum.Run;
            _velocity = Vector2.zero;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("LetterBlock"), false);
        }
    }
}
