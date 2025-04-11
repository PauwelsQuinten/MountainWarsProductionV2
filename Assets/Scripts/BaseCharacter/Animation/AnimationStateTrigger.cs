using Unity.VisualScripting;
using UnityEngine;

public class AnimationStateTrigger : StateMachineBehaviour
{
    [SerializeField] private GameEvent _endAnimation;


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _endAnimation.Raise(animator.transform.parent, null);
        Debug.Log("Exited state: " + stateInfo.shortNameHash);
    }

}
