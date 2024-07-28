using UnityEngine;

/// <summary>
/// Handles player animation
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject deathSmoke;
    [SerializeField] private SpriteRenderer sprite;

    [SerializeField] private float maxTilt = 5;
    [SerializeField] private float tiltSpeed = 20;

    // [Header("Particles")] [SerializeField] private ParticleSystem _jumpParticles;
    // [SerializeField] private ParticleSystem _launchParticles;
    // [SerializeField] private ParticleSystem _moveParticles;
    // [SerializeField] private ParticleSystem _landParticles;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] footsteps;
    [SerializeField] private AudioClip jump;

    private AudioSource _source;
    private PlayerController _player;
    private bool _grounded;
    // private ParticleSystem.MinMaxGradient _currentGradient;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _player = GetComponentInParent<PlayerController>();
    }

    private void OnEnable()
    {
        _player.Jumped += OnJumped;
        _player.GroundedChanged += OnGroundedChanged;
        _player.WallChanged += OnWallChanged;
        _player.Death += OnDeath;

        // _moveParticles.Play();
    }

    private void OnDisable()
    {
        _player.Jumped -= OnJumped;
        _player.GroundedChanged -= OnGroundedChanged;
        _player.WallChanged -= OnWallChanged;
        _player.Death -= OnDeath;

        // _moveParticles.Stop();
    }

    private void Update()
    {
        if (_player == null) return;

        DetectGroundColor();
        HandleSpriteFlip();
        // HandleYVelocity();
        HandleVerticalSpeed();
        HandleCharacterTilt();
    }

    private void HandleSpriteFlip()
    {
        if (_player.Velocity.x != 0) sprite.flipX = _player.Velocity.x < 0;
    }

    // private void HandleYVelocity()
    // {
    //     var inputStrength = Mathf.Abs(_player.FrameInput.x);
    //     _anim.SetFloat(YVelocityKey, Mathf.Lerp(1, _maxYVelocity, inputStrength));
        // _moveParticles.transform.localScale = Vector3.MoveTowards(_moveParticles.transform.localScale, Vector3.one * inputStrength, 2 * Time.deltaTime);
    // }

    private void HandleVerticalSpeed()
    {
        anim.SetFloat(XSpeedKey, Mathf.Abs(_player.Velocity.x));
        anim.SetFloat(YVelocityKey, _player.Velocity.y);
    }

    private void HandleCharacterTilt()
    {
        var runningTilt = _grounded ? Quaternion.Euler(0, 0, maxTilt * _player.Velocity.x) : Quaternion.identity;
        anim.transform.up = Vector3.RotateTowards(anim.transform.up, runningTilt * Vector2.up, tiltSpeed * Time.deltaTime, 0f);
    }

    private void OnWallChanged(bool wallHit)
    {
        anim.SetBool(WallHitKey, wallHit);
        if (wallHit) {
            anim.ResetTrigger(JumpKey);
        }
    }

    private void OnJumped()
    {
        anim.SetTrigger(JumpKey);
        anim.ResetTrigger(GroundedKey);


        // if (_grounded) // Avoid coyote
        // {
        //     SetColor(_jumpParticles);
        //     SetColor(_launchParticles);
        //     _jumpParticles.Play();
        // }
    }

    private void OnGroundedChanged(bool grounded, float impact)
    {
        _grounded = grounded;

        if (grounded)
        {
            // DetectGroundColor();
            // SetColor(_landParticles);

            anim.ResetTrigger(JumpKey);
            anim.SetBool(GroundedKey, true);
            _source.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)]);
            // _moveParticles.Play();

            // _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
            // _landParticles.Play();
        }
        else
        {
            anim.SetBool(GroundedKey, false);
            // _moveParticles.Stop();
        }
    }

    private void OnDeath()
    {
        Instantiate(deathSmoke, transform);
        anim.SetTrigger(DeathKey);
    }

    private void DetectGroundColor()
    {
        var hit = Physics2D.Raycast(transform.position, Vector3.down, 2);

        if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) return;
        var color = r.color;
        // _currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
        // SetColor(_moveParticles);
    }

    // private void SetColor(ParticleSystem ps)
    // {
    //     var main = ps.main;
    //     main.startColor = _currentGradient;
    // }

    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int XSpeedKey = Animator.StringToHash("XSpeed");
    private static readonly int YVelocityKey = Animator.StringToHash("YVelocity");
    private static readonly int WallHitKey = Animator.StringToHash("WallHit");
    private static readonly int JumpKey = Animator.StringToHash("Jump");
    private static readonly int DeathKey = Animator.StringToHash("Dead");
}