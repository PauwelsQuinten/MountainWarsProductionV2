using UnityEngine;

public class SwordRotation : StateMachineBehaviour
{
    [SerializeField] private GameEvent _rotateSword;


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        _rotateSword.Raise(animator.transform.parent, 1);
        //Debug.Log("Exited state: " + stateInfo.ToString());
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        _rotateSword.Raise(animator.transform.parent, 0);
    }

}
