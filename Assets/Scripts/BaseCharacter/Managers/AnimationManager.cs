using System;
using System.Collections;
using Unity.VisualScripting;
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

    WalkBehaviour _walkBehaviour;
    //private bool _canResetIdle = true;
    //private Coroutine _animCoroutine;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _walkBehaviour = _animator.GetBehaviour<WalkBehaviour>();
        _animator.SetFloat("AttackState", 3f);

    }

    public void ChangeAnimationState(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        WalkingEventArgs walkArgs = obj as WalkingEventArgs;
        if (walkArgs != null)
        {
            SetWalkingState(walkArgs);
            return;
        }
        AnimationEventArgs args = obj as AnimationEventArgs;
        if (args == null) return;

        //Reset signal to make sure the swing will not be interupted, this is only for reseting the trigger and nothing else
        if (ResetFeintSignal(args))
            return;

        //For fluid block direction switches, else he will always first equip. looks really buggy
        if (SetBlockDirection(args))
            return;

        //Reset bored timer when doing an action
        if (args.AnimState != AnimationState.Idle)
        {
            ResetBoredTime();
        }

        //Lowerbody(2) should always transition even when stunnend
        if ((!_animator.GetBool("IsStunned") && !_animator.GetBool("GetHit")) || (args.AnimLayer.Count == 1 && args.AnimLayer.Contains(2)))
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
                    foreach (int layer in args.AnimLayer)
                        _animator.CrossFade(args.AnimState.ToString(), 0.2f, layer, 0f);
                    break;
            }
        }

        _currentState = args.AnimState;

        //Set a bool to prevent the actionqueue from executing another action before current is finished 
        if (args.DoResetIdle)
        {
            _startAnimation.Raise(this, null);
        }
    }

    private void ResetBoredTime()
    {
        BoredBehaviour bored = _animator.GetBehaviour<BoredBehaviour>();
        if (bored != null) bored.IdleExit();
    }

    private void SetWalkingState(WalkingEventArgs walkArgs)
    {
        float XVelocity = 0f;
        float YVelocity = 0f;
        float GotTarget = 0f;
        if (walkArgs.IsLockon)
        {
            float angleDiff = Geometry.Geometry.CalculateAngleRadOfInput(walkArgs.WalkDirection) - walkArgs.Orientation;
            float speed = walkArgs.WalkDirection.magnitude;
            Vector2 animInput = Geometry.Geometry.CalculateVectorFromfOrientation(angleDiff) * speed;
            XVelocity = animInput.x;
            YVelocity = animInput.y;
            GotTarget = 1f;
        }
        else
        {
            XVelocity = walkArgs.WalkDirection.magnitude;
        }
        _animator.SetFloat("OnTarget", GotTarget);
        _animator.SetFloat("Xmovement", XVelocity);
        _animator.SetFloat("Ymovement", YVelocity);

        ResetBoredTime();
    }

    private bool SetBlockDirection(AnimationEventArgs args)
    {
        if (_currentState == args.AnimState && args.AnimState != AnimationState.Idle && args.AnimState != AnimationState.Empty)
        {
            if (args.BlockDirection != Direction.Default)
            {
                _animator.SetFloat("fBlockDirection", (float)(int)args.BlockDirection);
                _animator.SetInteger("BlockMedium", (int)args.BlockMedium);
            }

            return true;
        }
        else if (args.BlockDirection != Direction.Default)
        {
            _animator.SetFloat("fBlockDirection", (float)(int)args.BlockDirection);
            _animator.SetInteger("BlockMedium", (int)args.BlockMedium);
        }
        return false;
    }

    private bool ResetFeintSignal(AnimationEventArgs args)
    {
        if (!args.IsFeint)
        {
            InteruptAnimation(false);
            return true;
        }
        else if (args.AnimState == AnimationState.SlashLeft || args.AnimState == AnimationState.SlashRight || args.AnimState == AnimationState.Stab)
            InteruptAnimation(true);
        return false;
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

    public void UpdateWalingSpeed(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        WalkingEventArgs args = obj as WalkingEventArgs;
        if (args != null) return;

        if (_walkBehaviour != null)
        {
            _walkBehaviour.Velocity = args.WalkDirection;
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
