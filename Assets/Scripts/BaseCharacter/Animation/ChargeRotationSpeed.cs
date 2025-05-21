using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class ChargeRotationSpeed : StateMachineBehaviour
{
    [SerializeField] private float _powerToSpeedRatio = 0.7f;
    [SerializeField] private float _animationMaxSpeedAfterCharge = 2f;
    [SerializeField] private float _animationMinSpeedAfterCharge = 1f;
    private float _speed = 1f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat("ActionSpeed", 0.1f);

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)    
    {
        var character = animator.transform.parent;
        if (character != null )
        {
            var comp = character.GetComponent<Attacking>();
            if (comp != null && !comp.ChargePowerUsed)
            {
                animator.SetFloat("ActionSpeed", comp.ChargedPower * _powerToSpeedRatio);
                _speed = animator.GetFloat("ActionSpeed");

            }

        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_speed < 1f)
            animator.SetFloat("ActionSpeed", _animationMinSpeedAfterCharge);
        else if (_speed < _animationMaxSpeedAfterCharge)
            animator.SetFloat("ActionSpeed", _speed);
        else
            animator.SetFloat("ActionSpeed", _animationMaxSpeedAfterCharge);

        var character = animator.transform.parent;
        if (character != null)
        {
            var comp = character.GetComponent<AnimationManager>();
            if (comp != null)
                comp.LastAnimFrameCalled(animator.transform.parent, null);

        }
    }

}
