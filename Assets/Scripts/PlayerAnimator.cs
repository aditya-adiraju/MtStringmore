using UnityEngine;

/// <summary>
/// Handles player animation
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    #region Serialized Private Fields

    // @formatter:off
    [Header("References")]
    [SerializeField] private Animator anim;

    [SerializeField] private GameObject deathSmoke;
    [SerializeField] private SpriteRenderer sprite;

    // [Header("Particles")] [SerializeField] private ParticleSystem _jumpParticles;
    // [SerializeField] private ParticleSystem _launchParticles;
    // [SerializeField] private ParticleSystem _moveParticles;
    // [SerializeField] private ParticleSystem _landParticles;

    [Header("Audio Clips")] [SerializeField]
    private AudioClip runSound;

    [SerializeField] private AudioClip[] jumpSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip wallSlideSound;
    [SerializeField] private AudioClip[] deathSounds;
    
    [Header("Visual")]
    [Tooltip("Player position offset when hanging onto object (small red wire sphere gizmo)")]
    [SerializeField] private Vector2 hangOffset;
    // @formatter:on

    #endregion

    private Vector3 _spriteOriginalPosition;
    
    /// <summary>
    /// Max number of roasted states.
    /// </summary>
    public const int NumRoastStates = 6;

    /// <summary>
    /// RoastState of the player (i.e. how cooked they are) on a scale of 0 (not cooked) to <see cref="NumRoastStates"/>
    /// (very cooked).
    /// </summary>
    public int RoastState
    {
        set => anim.SetInteger(RoastKey, value);
    }

    #region Private Fields

    private AudioSource _source;
    private PlayerController _player;

    private bool _grounded;
    // private ParticleSystem.MinMaxGradient _currentGradient;

    #endregion

    #region Animation Keys

    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int XSpeedKey = Animator.StringToHash("XSpeed");
    private static readonly int YVelocityKey = Animator.StringToHash("YVelocity");
    private static readonly int WallChangedKey = Animator.StringToHash("WallChanged");
    private static readonly int HangKey = Animator.StringToHash("Hang");
    private static readonly int JumpKey = Animator.StringToHash("Jump");
    private static readonly int DeathKey = Animator.StringToHash("Dead");
    private static readonly int RoastKey = Animator.StringToHash("Roast");

    #endregion

    #region Unity Event Handlers

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _player = GetComponentInParent<PlayerController>();
        _spriteOriginalPosition = transform.localPosition;
    }

    private void Start()
    {
        GameManager.Instance.Reset += OnReset;
    }

    private void OnEnable()
    {
        _player.Jumped += OnJumped;
        _player.DoubleJumped += OnJumped;
        _player.GroundedChanged += OnGroundedChanged;
        _player.WallChanged += OnWallChanged;
        _player.HangChanged += OnHangChanged;
        _player.Death += OnDeath;
        _player.Dashed += OnDash;
        _player.SwingDifferentDirection += OnSwingDifferentDirection;

        // _moveParticles.Play();
    }

    private void OnDisable()
    {
        _player.Jumped -= OnJumped;
        _player.DoubleJumped -= OnJumped;
        _player.GroundedChanged -= OnGroundedChanged;
        _player.WallChanged -= OnWallChanged;
        _player.HangChanged -= OnHangChanged;
        _player.Death -= OnDeath;
        _player.Dashed -= OnDash;
        
        GameManager.Instance.Reset -= OnReset;

        // _moveParticles.Stop();
    }

    private void Update()
    {
        if (_player is null) return;

        // DetectGroundColor();
        HandleSpriteFlip();
        HandleVerticalSpeed();
    }

    #endregion

    #region Event Handlers

    private void HandleSpriteFlip()
    {
        if (_player.PlayerState != PlayerController.PlayerStateEnum.Swing && _player.Velocity.x != 0)
        {
            sprite.flipX = _player.Velocity.x < 0;
        }
    }

    private void HandleVerticalSpeed()
    {
        anim.SetFloat(XSpeedKey, Mathf.Abs(_player.Velocity.x));
        anim.SetFloat(YVelocityKey, _player.Velocity.y);
    }

    /// <summary>
    ///     If hitting wall, set wall changed to true and reset jump trigger because not jumping when holding onto wall
    /// </summary>
    /// <param name="wallChanged">True if hitting wall, false otherwise</param>
    private void OnWallChanged(bool wallChanged)
    {
        anim.SetBool(WallChangedKey, wallChanged);
        if (wallChanged)
        {
            anim.ResetTrigger(JumpKey);
            if (!_grounded && (_source.clip != wallSlideSound || !_source.isPlaying))
            {
                _source.clip = wallSlideSound;
                _source.Play();
            }
        }
        else if (_source.clip == wallSlideSound)
        {
            _source.Stop();
        }
    }

    /// <summary>
    ///     Set jump trigger
    ///     If jumping, no longer hitting ground, so reset grounded
    /// </summary>
    private void OnJumped()
    {
        anim.SetTrigger(JumpKey);
        anim.SetBool(GroundedKey, false);

        int randomSound = Random.Range(0, jumpSound.Length);
        _source.clip = jumpSound[randomSound];
        _source.PlayOneShot(jumpSound[randomSound]);

        // if (_grounded) // Avoid coyote
        // {
        //     SetColor(_jumpParticles);
        //     SetColor(_launchParticles);
        //     _jumpParticles.Play();
        // }
    }

    /// <summary>
    ///     If hitting ground, no longer jumping, so reset jump trigger
    ///     Set grounded to true
    ///     Play footstep audio
    /// </summary>
    /// <param name="grounded">True if hitting ground, false otherwise</param>
    /// <param name="impact">Y velocity upon hitting ground</param>
    private void OnGroundedChanged(bool grounded, float impact)
    {
        _grounded = grounded;

        if (grounded)
        {
            // DetectGroundColor();
            // SetColor(_landParticles);

            anim.ResetTrigger(JumpKey);
            anim.SetBool(GroundedKey, true);

            if (_source.clip == wallSlideSound)
                _source.Stop();
            _source.PlayOneShot(landSound);

            // _moveParticles.Play();

            // _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
            // _landParticles.Play();
        }
        else
        {
            if (_source.loop)
                _source.loop = false;
            anim.SetBool(GroundedKey, false);
            // _moveParticles.Stop();
        }
    }

    private void OnHangChanged(bool hanging, bool facingLeft)
    {
        anim.SetBool(HangKey, hanging);
        if (hanging)
        {
            if (facingLeft)
                transform.localPosition = new Vector3(
                    -_spriteOriginalPosition.x + hangOffset.x,
                    transform.localPosition.y,
                    transform.localPosition.z);
            else
                transform.localPosition = new Vector3(
                    _spriteOriginalPosition.x - hangOffset.x,
                    transform.localPosition.y,
                    transform.localPosition.z);
        }
        else
            transform.localPosition = _spriteOriginalPosition;
    }

    /// <summary>
    ///     Instantiates a death smoke object and sets the animation state to dead
    /// </summary>
    private void OnDeath()
    {
        foreach (var sound in deathSounds)
            _source.PlayOneShot(sound);
        Instantiate(deathSmoke, transform);
        anim.SetBool(DeathKey, true);
    }

    private void OnDash()
    {
        _source.clip = dashSound;
        _source.PlayOneShot(dashSound);
    }
    
    private void OnSwingDifferentDirection(bool clockwise)
    {
        // TODO FILL IN BY AUDIO PERSON
        transform.localPosition = new Vector3(
            -transform.localPosition.x,
            transform.localPosition.y, 
            transform.localPosition.z);
        sprite.flipX = clockwise;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3(hangOffset.x, hangOffset.y, 0f), 0.2f);
    }

    // private void DetectGroundColor()
    // {
    //     var hit = Physics2D.Raycast(transform.position, Vector3.down, 2);
    //
    //     if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) return;
    //     var color = r.color;
    //     _currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
    //     SetColor(_moveParticles);
    // }

    // private void SetColor(ParticleSystem ps)
    // {
    //     var main = ps.main;
    //     main.startColor = _currentGradient;
    // }

    /// <summary>
    /// On reset, re-activate sprites and make them full opacity
    /// </summary>
    private void OnReset()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
            var material = child.GetComponent<Renderer>().material;
            Color color = material.color;
            color.a = 1;
            material.color = color;
        }
    }

    #endregion
}
