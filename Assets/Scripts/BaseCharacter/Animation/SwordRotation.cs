using UnityEngine;

public class SwordRotation : StateMachineBehaviour
{
    [SerializeField] private GameEvent _rotateSword;


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("Exited state: " + stateInfo.ToString());
        
        _rotateSword.Raise(animator.transform.parent, 1);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        _rotateSword.Raise(animator.transform.parent, 0);
    }

}
