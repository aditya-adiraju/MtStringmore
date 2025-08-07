using System.Collections;
using JetBrains.Annotations;
using Player;
using UnityEngine;
using Util;
using Yarn.Unity;

namespace Yarn
{
    public class CutsceneCharacter : MonoBehaviour
    {
        private static readonly int YVelocityKey = Animator.StringToHash("YVelocity");
        private static readonly int AnimSpeed = Animator.StringToHash("AnimSpeed");
        private static readonly int WallChangedKey = Animator.StringToHash("WallChanged");
        private static readonly int JumpKey = Animator.StringToHash("Jump");
        private Animator _animator;
        private Vector3 _initialPos;
        private bool _isMoving;
        [CanBeNull] private PlayerAnimator _playerAnimator;
        [CanBeNull] private Coroutine _shiverCoroutine;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _playerAnimator = gameObject.GetComponentInChildren<PlayerAnimator>();
            _spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
            _animator = gameObject.GetComponentInChildren<Animator>();
        }

        // move character within cutscene
        // yarn syntax is <<move CharacterObjectName x y speed [flip] [keepAnim] [waitAnim] [z] [animSpeed]>>
        // e.g. <<move Knitby 3 1 20>>
        [YarnCommand("move")]
        public IEnumerator MoveCoroutine(float x, float y, float speed, bool flipSprite = false,
            bool keepAnimation = true, bool waitAnimation = false, float z = 0, float animSpeed = 1f)
        {
            _animator.enabled = true;
            _animator.SetFloat(AnimSpeed, animSpeed);
            Vector3 position = new(x, y, z == 0 ? transform.position.z : z);
            if (flipSprite) _spriteRenderer.flipX = !_spriteRenderer.flipX;
            _isMoving = true;
            while (transform.position != position)
            {
                transform.position = Vector3.MoveTowards(transform.position, position, Time.deltaTime * speed);
                yield return null;
            }

            _isMoving = false;
            yield return SetAnimation(keepAnimation, waitAnimation);
        }

        // leap character within cutscene
        // yarn syntax is
        // <<leap CharacterObjectName xVel yVelInitial duration [grounded] [gravity] [flip] [keepAnim] [waitAnim]>>
        // e.g. <<leap Knitby 3 1 20 true -66 true true true>>
        [YarnCommand("leap")]
        public IEnumerator LeapCoroutine(float xVel, float yVelInitial, float duration, bool grounded = false,
            float gravity = -66, bool flipSprite = false,
            bool keepAnimation = true, bool waitAnimation = false)
        {
            _animator.enabled = true;
            if (flipSprite) _spriteRenderer.flipX = !_spriteRenderer.flipX;
            _isMoving = true;

            float yVel = yVelInitial;

            // apply animation changes to either marshmallow or knitby
            if (_playerAnimator)
            {
                _playerAnimator.OnWallChanged(false);
                _playerAnimator.OnJumped();
                _playerAnimator.OnGroundedChanged(false, 0f);
            }
            else
            {
                // 1.67f for 0.5s = 0.83f as the numerator
                _animator.SetFloat(AnimSpeed, 0.83f / duration);
            }

            for (float elapsedTime = 0; elapsedTime <= duration; elapsedTime += Time.deltaTime)
            {
                // do scuffed kinematics
                transform.position = new Vector3(
                    transform.position.x + Time.deltaTime * xVel,
                    transform.position.y + yVel * Time.deltaTime + 0.5f * gravity * Mathf.Pow(Time.deltaTime, 2.0f),
                    transform.position.z);
                // update new yVel
                yVel += Time.deltaTime * gravity;
                _animator.SetFloat(YVelocityKey, yVel);
                yield return null;
            }

            yield return SetAnimation(keepAnimation, waitAnimation);
            _isMoving = false;
            if (_playerAnimator)
            {
                if (grounded)
                {
                    _playerAnimator?.OnGroundedChanged(true, 0f);
                }
                else
                {
                    _animator.SetBool(WallChangedKey, true);
                    _animator.ResetTrigger(JumpKey);
                }
            }
            else
            {
                _animator.SetFloat(AnimSpeed, 1f); // reset changes made to knitby jump speed
            }
        }

        // leap_nonblock is a non-blocking version of leap with identical syntax
        [YarnCommand("leap_nonblock")]
        public void Leap(float xVel, float yVelInitial, float duration, bool grounded = false, float gravity = -66,
            bool flipSprite = false, bool keepAnimation = true, bool waitAnimation = false)
        {
            StartCoroutine(LeapCoroutine(xVel, yVelInitial, duration, grounded, gravity, flipSprite, keepAnimation,
                waitAnimation));
        }

        [YarnCommand("flip")]
        public void Flip()
        {
            _spriteRenderer.flipX = !_spriteRenderer.flipX;
        }

        // run is a non-blocking version of move with identical syntax
        [YarnCommand("run")]
        public void Run(float x, float y, float speed, bool flipSprite = false, bool keepAnimation = true,
            bool waitAnimation = false, float z = 0, float animSpeed = 1f)
        {
            StartCoroutine(MoveCoroutine(x, y, speed, flipSprite, keepAnimation, waitAnimation, z, animSpeed));
        }

        [YarnCommand("hide")]
        public void Hide()
        {
            _spriteRenderer.enabled = false;
        }

        [YarnCommand("show")]
        public void Show()
        {
            _spriteRenderer.enabled = true;
        }

        [YarnCommand("set_animation")]
        public IEnumerator SetAnimation(bool state, bool wait)
        {
            // wait until current animation is finished
            if (wait && _animator.enabled)
            {
                int finishTime = Mathf.CeilToInt(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                if (finishTime == 0) finishTime = 1;
                yield return new WaitWhile(() =>
                    _animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= finishTime &&
                    _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Knitby_Idle");
            }

            _animator.enabled = state;
        }

        [YarnCommand("shiver")]
        public IEnumerator ShiverCoroutine(float jitter, float jitterMagnitude, float duration = float.MaxValue)
        {
            _initialPos = _spriteRenderer.gameObject.transform.position;
            yield return SetAnimation(false, true);
            _shiverCoroutine =
                StartCoroutine(RandomUtil.RandomJitterRoutine(_spriteRenderer.gameObject.transform, duration, jitter,
                    jitterMagnitude));
        }

        [YarnCommand("stop_shiver")]
        public IEnumerator StopShiver()
        {
            if (_shiverCoroutine is not null) StopCoroutine(_shiverCoroutine);
            _spriteRenderer.gameObject.transform.position = _initialPos;
            // wait a frame before restarting animation or,
            // for some reason, knitby will sometimes randomly jump
            yield return new WaitForFixedUpdate();
            yield return SetAnimation(true, true);
        }

        [YarnCommand("wait_move_finish")]
        public IEnumerator WaitMoveFinish()
        {
            yield return new WaitUntil(() => !_isMoving);
        }
    }
}
