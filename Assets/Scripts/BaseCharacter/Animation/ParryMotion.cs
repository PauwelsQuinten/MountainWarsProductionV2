using UnityEngine;

public class ParryMotion : StateMachineBehaviour
{
    //This script is for putting the Parry/Blocking script in state of executing a parry
    //This is so he can be hit when the parry is baddly timed

    [SerializeField] private GameEvent _setInParryAnimation;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _setInParryAnimation.Raise(animator.transform.parent, true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _setInParryAnimation.Raise(animator.transform.parent, false);
        //Debug.Log("Exited state: " + stateInfo.ToString());
    }
}
