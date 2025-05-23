using UnityEngine;

public class ResetAttackState : StateMachineBehaviour
{
    private const string P_ATTACK_STATE = "AttackState";
    private const string P_FULL_BODY = "FullBodyAnimation";
    private const string P_FEINT = "Feint";


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //When attack is a Stab, make sure to reset the Feint signal which is on by default
        var state = animator.GetFloat(P_ATTACK_STATE);
        if (state == 2)
            animator.ResetTrigger(P_FEINT);

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Reset the attackstate so it wont keep looping this attack
        animator.SetFloat(P_ATTACK_STATE, 3f);
        animator.SetBool(P_FULL_BODY, false);

    }

}
