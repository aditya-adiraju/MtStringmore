using UnityEngine;

/// <summary>
/// Controls Jump; only allows a transition to Fall if all frames in Jump
/// have been played
/// </summary>
public class KnitbyAnimatorJump : StateMachineBehaviour
{
    private static readonly int JumpedKey = Animator.StringToHash("Jumped");

    /// <inheritdoc />
    /// <remarks>
    /// Called when a transition starts and the state machine starts to evaluate this state
    /// </remarks>
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(JumpedKey, false);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Called on each Update frame between OnStateEnter and OnStateExit callbacks
    /// </remarks>
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.GetBool(JumpedKey) && stateInfo.normalizedTime >= 0.99)
        {
            animator.SetBool(JumpedKey, true);
        }
    }
}
