using System.Collections;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.Rendering.GPUSort;

public class AnimationManager : MonoBehaviour
{
    private Animator _animator;
    private AnimationState _currentState;
    [Header("Events")]
    [SerializeField] private GameEvent _endAnimation;
    [SerializeField] private GameEvent _startAnimation;

    private bool _canResetIdle = true;
    private Coroutine _animCoroutine;

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
            Debug.Log("Disrupt");
            return;
        }

        if(args.AnimState != AnimationState.Idle)
        {
                BoredBehaviour bored = _animator.GetBehaviour<BoredBehaviour>();
                if (bored != null) bored.IdleExit();
                //else Debug.Log("Bored is null");
        }

        //Debug.Log($"anim call: {args.AnimState.ToString()}, speed: {args.Speed}, layer: {args.AnimLayer}");
        // Crossfade with normalized transition offset
        _animator.speed = args.Speed;
        _animator.CrossFade(args.AnimState.ToString(), 0.2f, args.AnimLayer, 0f);

        _currentState = args.AnimState;

        if (args.DoResetIdle)
        {
            _startAnimation.Raise(this, null);
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
        _animator.SetTrigger("Feint");
        
    }

    //Deprectated probably
    public void LastAnimFrameCalled(Component Sender, object obj)
    {
        if (Sender.gameObject != gameObject) return;

        _currentState = AnimationState.Idle;
    }

    public void GetHit(Component Sender, object obj)
    {
        if (Sender.gameObject != gameObject) return;

        _animator.speed = 1;
        _animator.SetTrigger("GetHit");
    }
    
    public void GetStunned(Component sender, object obj)
    {
        var args = obj as StunEventArgs;
        if (args == null)return;
        if (args.ComesFromEnemy && sender.gameObject == gameObject)return;
        else if (!args.ComesFromEnemy && sender.gameObject != gameObject) return;
        
        _animator.speed = 1;
        _animator.SetBool("IsStunned", true);
    }

     public void RecoverStunned(Component Sender, object obj)
    {
        if (Sender.gameObject != gameObject) return;

        _animator.SetBool("IsStunned", false);
    }

}
