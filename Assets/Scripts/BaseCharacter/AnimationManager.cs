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

    //private bool _canResetIdle = true;
    //private Coroutine _animCoroutine;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void ChangeAnimationState(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        AnimationEventArgs args = obj as AnimationEventArgs;
        if (args == null) return;

        //For fluid block direction switches, else he will always first equip. looks really buggy
        if (_currentState == args.AnimState && args.AnimState != AnimationState.Idle && args.AnimState != AnimationState.Empty)
        {
            if (args.BlockDirection != Direction.Default)
            {
                _animator.SetFloat("fBlockDirection", (float)(int)args.BlockDirection);
                _animator.SetInteger("BlockState", (int)args.BlockMedium);
            }

            return;
        }
        else if (args.BlockDirection != Direction.Default)
        {
            _animator.SetFloat("fBlockDirection", (float)(int)args.BlockDirection);
            _animator.SetInteger("BlockState", (int)args.BlockMedium);
        }
        if (args.Interupt)
        {
            InteruptAnimation(args);
        }

        if(args.AnimState != AnimationState.Idle)
        {
                BoredBehaviour bored = _animator.GetBehaviour<BoredBehaviour>();
                if (bored != null) bored.IdleExit();
                //else Debug.Log("Bored is null");
        }
        // Crossfade with normalized transition offset

        if ((!_animator.GetBool("IsStunned") && !_animator.GetBool("GetHit")) || args.AnimLayer == 2)
            _animator.CrossFade(args.AnimState.ToString(), 0.2f, args.AnimLayer, 0f);

        _currentState = args.AnimState;

        if (args.DoResetIdle)
        {
            _animator.SetFloat("ActionSpeed", args.Speed);
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
        LoseEquipmentEventArgs loseEquipmentEventArgs = obj as LoseEquipmentEventArgs;
        if (loseEquipmentEventArgs != null && sender.gameObject == gameObject)
        {
            _animator.speed = 1;
            _animator.SetBool("IsStunned", true);
        }

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
