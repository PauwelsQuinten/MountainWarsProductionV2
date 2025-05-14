using UnityEngine;

public class SwordRotation : StateMachineBehaviour
{
    [SerializeField] private GameEvent _rotateSword;
    private float _storredAttackState = 3f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _storredAttackState = animator.GetFloat("AttackState");
        if (_storredAttackState == 2f)
            _rotateSword.Raise(animator.transform.parent, 1);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (_storredAttackState == 2f)
        {
            _rotateSword.Raise(animator.transform.parent, 0);
            _storredAttackState = 3f;
        }
    }

}
