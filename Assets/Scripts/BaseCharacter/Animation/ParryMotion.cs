using UnityEngine;

public class ParryMotion : StateMachineBehaviour
{
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
