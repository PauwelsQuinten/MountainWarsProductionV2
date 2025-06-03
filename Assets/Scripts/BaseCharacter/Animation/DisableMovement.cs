using UnityEngine;

public class DisableMovement : StateMachineBehaviour
{
    CharacterMovement _moveComp = null;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GetComponent(animator);

        _moveComp.GetComponent<CharacterMovement>().enabled = false;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        _moveComp.GetComponent<CharacterMovement>().enabled = true;
    }


    private void GetComponent(Animator animator)
    {
        if (_moveComp == null)
        {
            var parent = animator.transform.parent.gameObject;
            _moveComp = parent.GetComponent<CharacterMovement>();
            if (_moveComp == null)
                Debug.LogError("No movementComponent found for DisableMovement comp in animator state");
        }
    }
}
