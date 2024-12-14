using UnityEngine;

/// <summary>
/// Controls Jump; only allows a transition to Fall if all frames in Jump
/// have been played
/// </summary>
public class KnitbyAnimatorJump : StateMachineBehaviour
{
    private static readonly int JumpedKey = Animator.StringToHash("Jumped");
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(JumpedKey, false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetBool(JumpedKey) == false && stateInfo.normalizedTime >= 0.99)
        {
            animator.SetBool(JumpedKey, true);
        }
    }
}
