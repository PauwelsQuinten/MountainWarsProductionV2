using System.Collections;
using UnityEngine;

public class BoredBehaviour : StateMachineBehaviour
{
    [SerializeField]
    private float _timeUntilBored;

    [SerializeField]
    private int _numberOfBoredAnimations;

    private bool _isBored;
    private float _idleTime;
    private int _boredAnimation;

    private bool _canRun;
    private Animator _animator;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _animator = animator;
        _canRun = true;
        ResetIdle();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_canRun) return;
        if (_isBored == false)
        {
            _idleTime += Time.deltaTime;

            if (_idleTime > _timeUntilBored && stateInfo.normalizedTime % 1 < 0.02f)
            {
                _isBored = true;

                int tempInt = Random.Range(1, 4);

                switch (tempInt)
                {
                    case 1:
                        _boredAnimation = 2;
                        break;
                    case 2:
                        _boredAnimation = 4;
                        break;
                    case 3:
                        _boredAnimation = 6;
                        break;
                }


                _animator.SetFloat("BoredAnimation", _boredAnimation - 1);
            }
        }
        else if (stateInfo.normalizedTime % 1 > 0.98)
        {
            ResetIdle();
        }

        animator.SetFloat("BoredAnimation", _boredAnimation, 0.2f, Time.deltaTime);
    }

    public void IdleExit()
    {
        _canRun = false;
        ResetIdle();
        if (_animator != null) 
            _animator.SetFloat("BoredAnimation", _boredAnimation);
    }

    private void ResetIdle()
    {
        if (_isBored)
        {
            _boredAnimation--;
        }

        _isBored = false;
        _idleTime = 0;
    }
}
