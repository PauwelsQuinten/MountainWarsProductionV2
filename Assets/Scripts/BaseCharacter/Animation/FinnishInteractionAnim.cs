using UnityEngine;

public class FinnishInteractionAnim : StateMachineBehaviour
{
    [SerializeField] private GameEvent _stopFullBodyAnim;
    private bool _firedEvent = false;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > 0.9f && !_firedEvent)
        {
            _firedEvent = true;
            _stopFullBodyAnim.Raise(animator.transform.parent, null);
        }
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
            _firedEvent = false;
        
    }

}
