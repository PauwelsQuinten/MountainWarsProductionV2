using UnityEngine;

public class FullBodyOnTransition : StateMachineBehaviour
{
    private const string P_FULL_BODY = "FullBodyAnimation";
    private const string P_ATTACK_STATE = "AttackState";
    private const string P_BLOCKED_ATT = "BlockedAtt";


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(P_FULL_BODY, true);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Reset the attackstate so it wont keep looping this attack
        animator.SetFloat(P_BLOCKED_ATT, 3);
        animator.SetBool(P_FULL_BODY, false);

    }
}
