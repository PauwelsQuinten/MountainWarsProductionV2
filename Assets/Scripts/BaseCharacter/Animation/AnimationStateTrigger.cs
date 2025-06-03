using UnityEngine;

public class AnimationStateTrigger : StateMachineBehaviour
{
    [SerializeField] private GameEvent _endAnimation;
    [SerializeField] private GameEvent _startAnimation;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _startAnimation.Raise(animator.transform.parent, null);
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _endAnimation.Raise(animator.transform.parent, null);
        //Debug.Log("Exited state: " + stateInfo.ToString());
    }

}
