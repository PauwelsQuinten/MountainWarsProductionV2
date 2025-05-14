using System.Collections;
using Unity.VisualScripting;
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
        _animator.SetFloat("AttackState", 3f);
    }

    public void ChangeAnimationState(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        AnimationEventArgs args = obj as AnimationEventArgs;
        if (args == null) return;

        //Reset signal to make sure the swing will not be interupted, this is only for reseting the trigger and nothing else
        if (!args.IsFeint)
        {
            InteruptAnimation(false);
            return;
        }
        else if (args.AnimState == AnimationState.SlashLeft || args.AnimState == AnimationState.SlashRight || args.AnimState == AnimationState.Stab)
            InteruptAnimation(true);

        //For fluid block direction switches, else he will always first equip. looks really buggy
        if (_currentState == args.AnimState && args.AnimState != AnimationState.Idle && args.AnimState != AnimationState.Empty)
        {
            if (args.BlockDirection != Direction.Default)
            {
                _animator.SetFloat("fBlockDirection", (float)(int)args.BlockDirection);
                _animator.SetInteger("BlockMedium", (int)args.BlockMedium);
            }

            return;
        }
        else if (args.BlockDirection != Direction.Default)
        {
            _animator.SetFloat("fBlockDirection", (float)(int)args.BlockDirection);
            _animator.SetInteger("BlockMedium", (int)args.BlockMedium);
        }
        

        if(args.AnimState != AnimationState.Idle)
        {
                BoredBehaviour bored = _animator.GetBehaviour<BoredBehaviour>();
                if (bored != null) bored.IdleExit();
                //else Debug.Log("Bored is null");
        }
        // Crossfade with normalized transition offset

        //Lowerbody(2) should always transition even when stunnend
        if ((!_animator.GetBool("IsStunned") && !_animator.GetBool("GetHit")) || args.AnimLayer == 2)
        {
            _animator.SetFloat("ActionSpeed", args.Speed);

            switch (args.AnimState)
            {
                case AnimationState.Stab:
                    _animator.SetBool("IsAttackHigh", args.IsAttackHigh);
                    _animator.SetInteger("AttackMedium", (int)args.AttackMedium);
                    _animator.SetFloat("AttackState", 2f);
                    break;
                case AnimationState.SlashLeft:
                    _animator.SetBool("IsAttackHigh", args.IsAttackHigh);
                    _animator.SetInteger("AttackMedium", (int)args.AttackMedium);
                    _animator.SetFloat("AttackState", 0f);
                    break;
                case AnimationState.SlashRight:
                    _animator.SetBool("IsAttackHigh", args.IsAttackHigh);
                    _animator.SetInteger("AttackMedium", (int)args.AttackMedium);
                    _animator.SetFloat("AttackState", 1f);
                    break;
               default:
                    //_animator.SetFloat("AttackState", 3f);
                    _animator.CrossFade(args.AnimState.ToString(), 0.2f, args.AnimLayer, 0f);
                    break;
            }
        }

        _currentState = args.AnimState;

        if (args.DoResetIdle)
        {
            _startAnimation.Raise(this, null);
        }
    }

    public void SwitchWeaponStance(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        if (obj is bool)
        {
            bool useSpear = (bool)obj;
            if (_animator ==null)
                _animator = GetComponentInChildren<Animator>();
            _animator.SetBool("IsHoldingSpear", useSpear);
        }
       
    }

    private void InteruptAnimation(bool isFeint)
    {
        if(isFeint) 
            _animator.SetTrigger("Feint");
        else
            _animator.ResetTrigger("Feint");
        
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
        AttackEventArgs args = obj as AttackEventArgs;
        if (args == null) 
            return;

        _animator.speed = 1;
        _animator.SetFloat("HitHeight", (float)(int)args.AttackHeight);
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

    public void BlockHit(Component sender, object obj)
    {
        var args = obj as AttackEventArgs;
        if (args == null) return;
        if (args.Attacker == gameObject || args.Defender == gameObject)
            _animator.SetTrigger("BlockedHit");
    }


    public void RecoverStunned(Component Sender, object obj)
    {
        if (Sender.gameObject != gameObject) return;

        _animator.SetBool("IsStunned", false);
    }
}
