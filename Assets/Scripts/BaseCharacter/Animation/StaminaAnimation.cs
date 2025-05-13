using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class StaminaAnimation : StateMachineBehaviour
{
    private StaminaManager _staminaComp = null;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var character = animator.transform.parent;
        if (character != null)
        {
            _staminaComp = character.GetComponent<StaminaManager>();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    
    }

}
