using UnityEngine;
using Random = UnityEngine.Random;

namespace Util
{
    [RequireComponent(typeof(Animator))]
    public class AnimationCycleOffsetRandomizer : MonoBehaviour
    {
        private Animator anim;
        private static readonly int CycleOffsetKey = Animator.StringToHash("CycleOffset");
        
        private void Awake()
        {
            anim = GetComponent<Animator>();
            anim.SetFloat(CycleOffsetKey, Random.Range(0f, 1f));
        }
    }
}
