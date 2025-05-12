using UnityEngine;

public class ResetAttackState : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //When attack is a Stab, make sure to reset the Feint signal which is on by default
        var state = animator.GetFloat("AttackState");
        if (state == 2)
            animator.ResetTrigger("Feint");

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Reset the attackstate so it wont keep looping this attack
        animator.SetFloat("AttackState", 3f);
        
    }

}
