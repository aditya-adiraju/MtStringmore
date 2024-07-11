using UnityEngine;

/// <summary>
/// VERY primitive animator example.
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _anim;
    [SerializeField] private GameObject _deathSmoke;
    [SerializeField] private SpriteRenderer _sprite;

    [SerializeField] private float _maxTilt = 5;
    [SerializeField] private float _tiltSpeed = 20;

    // [Header("Particles")] [SerializeField] private ParticleSystem _jumpParticles;
    // [SerializeField] private ParticleSystem _launchParticles;
    // [SerializeField] private ParticleSystem _moveParticles;
    // [SerializeField] private ParticleSystem _landParticles;

    [Header("Audio Clips")] [SerializeField]
    private AudioClip[] _footsteps;

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
        _player.WallHit += OnWallHit;
        _player.Death += OnDeath;

        // _moveParticles.Play();
    }

    private void OnDisable()
    {
        _player.Jumped -= OnJumped;
        _player.GroundedChanged -= OnGroundedChanged;
        _player.WallHit -= OnWallHit;
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
        if (_player.FrameInput.x != 0) _sprite.flipX = _player.FrameInput.x < 0;
    }

    // private void HandleYVelocity()
    // {
    //     var inputStrength = Mathf.Abs(_player.FrameInput.x);
    //     _anim.SetFloat(YVelocityKey, Mathf.Lerp(1, _maxYVelocity, inputStrength));
        // _moveParticles.transform.localScale = Vector3.MoveTowards(_moveParticles.transform.localScale, Vector3.one * inputStrength, 2 * Time.deltaTime);
    // }

    private void HandleVerticalSpeed()
    {
        _anim.SetFloat(XSpeedKey, Mathf.Abs(_player.FrameInput.x));
        _anim.SetFloat(YVelocityKey, _player.FrameInput.y);
    }

    private void HandleCharacterTilt()
    {
        var runningTilt = _grounded ? Quaternion.Euler(0, 0, _maxTilt * _player.FrameInput.x) : Quaternion.identity;
        _anim.transform.up = Vector3.RotateTowards(_anim.transform.up, runningTilt * Vector2.up, _tiltSpeed * Time.deltaTime, 0f);
    }

    private void OnWallHit(bool wallHit)
    {
        _anim.SetBool(WallHitKey, wallHit);
        if (wallHit) {
            _anim.ResetTrigger(JumpKey);
        }
    }

    private void OnJumped()
    {
        _anim.SetTrigger(JumpKey);
        _anim.ResetTrigger(GroundedKey);


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

            _anim.ResetTrigger(JumpKey);
            _anim.SetBool(GroundedKey, true);
            _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
            // _moveParticles.Play();

            // _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
            // _landParticles.Play();
        }
        else
        {
            _anim.SetBool(GroundedKey, false);
            // _moveParticles.Stop();
        }
    }

    private void OnDeath()
    {
        _deathSmoke.SetActive(true);
        _anim.SetTrigger(DeathKey);
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