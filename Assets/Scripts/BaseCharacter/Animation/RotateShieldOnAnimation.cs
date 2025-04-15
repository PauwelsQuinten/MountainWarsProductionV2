using UnityEngine;

public class RotateShieldOnAnimation : StateMachineBehaviour
{
    [SerializeField] private GameEvent _rotateShield;


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int direction = animator.GetInteger("BlockDirection");

        _rotateShield.Raise(animator.transform.parent, direction);
        //Debug.Log("Exited state: " + stateInfo.ToString());
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        //_rotateShield.Raise(animator.transform.parent, 0);
    }

}
