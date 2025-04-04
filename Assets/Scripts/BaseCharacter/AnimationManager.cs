using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class AnimationManager : MonoBehaviour
{
    private Animator _animator;
    private AnimationState _currentState;
    [Header("Events")]
    [SerializeField] private GameEvent _endAnimation;
    [SerializeField] private GameEvent _startAnimation;

    private bool _canResetIdle = true;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void ChangeAnimationState(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        AnimationEventArgs args = obj as AnimationEventArgs;
        if (args == null) return;
        if (_currentState == args.AnimState && args.AnimState != AnimationState.Idle) return;

        if (args.Interupt)
        {
            InteruptAnimation(args);
            return;
        }

        if(args.AnimState != AnimationState.Idle)
        {
            if (_canResetIdle)
            {
                _animator.GetBehaviour<BoredBehaviour>().IdleExit();
                _canResetIdle = false;
            }
        }
        else
        {
            if (!_canResetIdle)
            {
                _canResetIdle = true;
            }
        }
        //switch (args.AnimLayer)
        //{
        //    case BASE_LAYER:
        //        if (args.AnimState == AnimationState.Empty) break;
        //        _animator.SetLayerWeight(BASE_LAYER, 1);
        //        _animator.SetLayerWeight(LOWER_BODY_LAYER, 1);
        //        _animator.SetLayerWeight(UPPER_BODY_LAYER, 1);
        //        Debug.Log("Enable full body");
        //        break;

        //    case UPPER_BODY_LAYER:
        //        if (args.AnimState == AnimationState.Empty) break;
        //        _animator.SetLayerWeight(BASE_LAYER, 1);
        //        _animator.SetLayerWeight(LOWER_BODY_LAYER, 1);
        //        _animator.SetLayerWeight(UPPER_BODY_LAYER, 1);
        //        Debug.Log("Enable upper body");
        //        break;

        //    case LOWER_BODY_LAYER:
        //        if (args.AnimState == AnimationState.Empty) break;
        //        _animator.SetLayerWeight(BASE_LAYER, 1);
        //        _animator.SetLayerWeight(LOWER_BODY_LAYER, 1);
        //        _animator.SetLayerWeight(UPPER_BODY_LAYER, 1);
        //        Debug.Log("Enable lower body");
        //        break;
        //}

        // Crossfade with normalized transition offset
        _animator.speed = args.Speed;
        _animator.CrossFade(args.AnimState.ToString(), 0.2f, args.AnimLayer, 0f);

        _currentState = args.AnimState;

        if ((int)_currentState > 3)
            _startAnimation.Raise(this, null);


        if (args.DoResetIdle)
        {
            StartCoroutine(ResetToIdle(_animator.GetCurrentAnimatorStateInfo(args.AnimLayer).length, args.AnimLayer));
        }
    }

    private void ResetAllLayers()
    {
        // Reset all layers to weight 0 first
        //_animator.SetLayerWeight(BASE_LAYER, 0);
        //_animator.SetLayerWeight(UPPER_BODY_LAYER, 0);
        //_animator.SetLayerWeight(LOWER_BODY_LAYER, 0);

        // Optional: Force all layers to their default state
        //if (_currentAnimLayer != 1) return;
        //_animator.Play("Empty", BASE_LAYER);
        //_animator.Play("Empty", UPPER_BODY_LAYER);
        //_animator.Play("Empty", LOWER_BODY_LAYER);
    }

    private void InteruptAnimation(AnimationEventArgs args)
    {
        _animator.CrossFade(AnimationState.Empty.ToString(), 0.3f, 3, 0.3f);
        _animator.CrossFade(args.AnimState.ToString(), 0.3f, args.AnimLayer, 0.2f);
        _currentState = AnimationState.Idle;
        _animator.GetBehaviour<BoredBehaviour>().IdleExit();


        _endAnimation.Raise(this, null);
    }

    private IEnumerator ResetToIdle(float time, int layer)
    {
        yield return new WaitForSeconds(time);

        // Reset only the active layer to idle
        _animator.CrossFade(AnimationState.Idle.ToString(), 0.2f, 1);
        ChangeAnimationState(this, new AnimationEventArgs { AnimState = AnimationState.Empty, AnimLayer = layer, DoResetIdle = false });

        _endAnimation.Raise(this, null);

        _currentState = AnimationState.Idle;
    }
}