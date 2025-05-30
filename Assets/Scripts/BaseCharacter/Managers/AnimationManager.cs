using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
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

    private float _XVelocity = 0f;
    private float _YVelocity = 0f;
    private float _GotTarget = 0f;
    private float _movementSpeed = 1f;
    private float _attBlend = 0f;
    private float _newBlockDirection = 0f;

    private const string P_FULL_BODY = "FullBodyAnimation";
    private const string P_BLOCK_DIR = "fBlockDirection";
    private const string P_BLOCKED_HIT = "BlockedHit";
    private const string P_BLOCK_MEDIUM = "BlockMedium";
    private const string P_Stun = "IsStunned";
    private const string P_ATTACK_STATE = "AttackState";
    private const string P_GET_HIT = "GetHit";
    private const string P_HIT_HEIGHT = "HitHeight";
    private const string P_ON_TARGET = "OnTarget";
    private const string P_X_MOVEMENT = "Xmovement";
    private const string P_y_MOVEMENT = "Ymovement";
    private const string P_ACTION_SPEED = "ActionSpeed";
    private const string P_ATTACK_MEDIUM = "LeftHandAttack";
    private const string P_ATTACK_HEIGHT = "IsAttackHigh";
    private const string P_FEINT = "Feint";
    private const string P_BLOCKED_ATT = "BlockedAtt";

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _animator.SetFloat(P_ATTACK_STATE, 3f);

    }

    private void Update()
    {
        UpdateAnimatorValues(_XVelocity, _YVelocity, _GotTarget, _movementSpeed, _attBlend);

        float current = _animator.GetFloat(P_BLOCK_DIR);
        if (!Mathf.Approximately(current, _newBlockDirection))
        {
            BockDirectionUpdate(true);
        }
        else
            BockDirectionUpdate(false);

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

        if (args.IsFullBodyAnim)
            _animator.SetBool(P_FULL_BODY, args.IsFullBodyAnim);

        //Reset bored timer when doing an action
        if (args.AnimState != AnimationState.Idle)
        {
            ResetBoredTime();
        }

        //Lowerbody(2) should always transition even when stunnend
        if ((!_animator.GetBool(P_Stun) && !_animator.GetBool(P_GET_HIT)) || (args.AnimLayer.Count == 1 && args.AnimLayer.Contains(2)))
        {
            _animator.SetFloat(P_ACTION_SPEED, args.Speed);

            switch (args.AnimState)
            {
                case AnimationState.Stab:
                    _animator.SetBool(P_ATTACK_HEIGHT, args.IsAttackHigh);
                    _animator.SetBool(P_ATTACK_MEDIUM, args.AttackWithLeftHand);
                    _animator.SetFloat(P_ATTACK_STATE, 2f);
                    _attBlend = 1f;
                    break;
                case AnimationState.SlashLeft:
                    _animator.SetBool(P_ATTACK_HEIGHT, args.IsAttackHigh);
                    _animator.SetBool(P_ATTACK_MEDIUM, args.AttackWithLeftHand);
                    if (args.AttackWithLeftHand)
                        _animator.SetFloat(P_ATTACK_STATE, 1f);
                    else
                        _animator.SetFloat(P_ATTACK_STATE, 0f);
                    _attBlend = 1f;
                    break;
                case AnimationState.SlashRight:
                    _animator.SetBool(P_ATTACK_HEIGHT, args.IsAttackHigh);
                    _animator.SetBool(P_ATTACK_MEDIUM, args.AttackWithLeftHand);
                    if (args.AttackWithLeftHand)
                        _animator.SetFloat(P_ATTACK_STATE, 0f);
                    else
                        _animator.SetFloat(P_ATTACK_STATE, 1f);
                    _attBlend = 1f;
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
        _YVelocity = 0f;
        _GotTarget = 0f;
        _movementSpeed = walkArgs.WalkDirection.magnitude * walkArgs.Speed;

        if (walkArgs.IsLockon)
        {
            float input = Geometry.Geometry.CalculateAngleRadOfInput(walkArgs.WalkDirection);
            float angleDiff = input - walkArgs.Orientation * Mathf.Deg2Rad;
            Vector2 animInput = Geometry.Geometry.CalculateVectorFromfOrientation(angleDiff) * _movementSpeed;
            _XVelocity = animInput.x;
            _YVelocity = animInput.y;
            _GotTarget = 1f;
        }
        else
        {
            _XVelocity = _movementSpeed;
        }
        ResetBoredTime();
    }

    private bool SetBlockDirection(AnimationEventArgs args)
    {
        //When already holding a block, only change the direction. nothing else needs to be updated
        if (_currentState == args.AnimState && args.AnimState != AnimationState.Idle && args.AnimState != AnimationState.Empty)
        {
            //Imediatly set block value without lerping when it was in a wrong direction or down
            if ((_newBlockDirection == (float)(int)Direction.Wrong || _newBlockDirection == (float)(int)Direction.Idle) 
                && _newBlockDirection != (float)(int)args.BlockDirection)
            {
                _newBlockDirection = (float)(int)args.BlockDirection;
                BockDirectionUpdate(false);
                return true;
            }
            //Switch between block directions, make sure if from wrong or down it is instantly
            if (args.BlockDirection != Direction.Default)
            {
                _newBlockDirection = (float)(int)args.BlockDirection;
                if (_newBlockDirection == (float)(int)Direction.Wrong || _newBlockDirection == (float)(int)Direction.Idle)
                    BockDirectionUpdate(false);
                _animator.SetInteger(P_BLOCK_MEDIUM, (int)args.BlockMedium);
            }
            
            return true;
        }
        //When start holding block
        else if (args.BlockDirection != Direction.Default)
        {
            if ((_newBlockDirection == (float)(int)Direction.Wrong || _newBlockDirection == (float)(int)Direction.Idle)
               && _newBlockDirection != (float)(int)args.BlockDirection)
            {
                _newBlockDirection = (float)(int)args.BlockDirection;
                BockDirectionUpdate(false);
            }
            _newBlockDirection = (float)(int)args.BlockDirection;
            _animator.SetInteger(P_BLOCK_MEDIUM, (int)args.BlockMedium);
        }

        if (_newBlockDirection == (float)(int)Direction.Wrong || _newBlockDirection == (float)(int)Direction.Idle)
            BockDirectionUpdate(false);
        return false;
    }

    private void BockDirectionUpdate(bool smoothly)
    {
        if (smoothly) 
            _animator.SetFloat(P_BLOCK_DIR, _newBlockDirection, 0.1f, Time.deltaTime);
        else
            _animator.SetFloat(P_BLOCK_DIR, _newBlockDirection);
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

    private void UpdateAnimatorValues(float XVelocity, float YVelocity, float GotTarget, float speed, float attBlend)
    {
        _animator.SetFloat(P_ON_TARGET, GotTarget, 0.1f, Time.deltaTime);
        _animator.SetFloat(P_X_MOVEMENT, XVelocity, 0.1f, Time.deltaTime);
        _animator.SetFloat(P_y_MOVEMENT, YVelocity, 0.1f, Time.deltaTime);
        //_animator.SetFloat("AttackBlend", attBlend, 0.1f, Time.deltaTime);
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
        _attBlend = 0f;
    }

    public void GetHit(Component Sender, object obj)
    {
        if (Sender.gameObject != gameObject) return;
        AttackEventArgs args = obj as AttackEventArgs;
        if (args == null) 
            return;

        _animator.speed = 1;
        _animator.SetFloat(P_HIT_HEIGHT, (float)(int)args.AttackHeight);

        //Set all states besides base  to empty 
        _animator.SetTrigger(P_GET_HIT);
        _animator.SetBool(P_Stun, true);

        for (int i = 1; i < 4; i++)
        {
            if (i == 1)
                _animator.CrossFade(AnimationState.Hit.ToString(), 0.2f, i, 0f);
            else
                _animator.CrossFade(AnimationState.Empty.ToString(), 0.2f, i, 0f);
        }
        //update this in animator imediatly or the stun will overwrite this state
        _animator.Update(0f);
    }

    public void GetStunned(Component sender, object obj)
    {
        LoseEquipmentEventArgs loseEquipmentEventArgs = obj as LoseEquipmentEventArgs;
        if (loseEquipmentEventArgs != null && sender.gameObject == gameObject)
        {
            SetStunValues();
        }

        var args = obj as StunEventArgs;
        if (args == null) return;
        if (args.StunTarget != gameObject) return;

        //When the character gets hit, he first get the animation call for hit.
        //Then the stun is set in StateManager which automaticly gets this stun call,
        //so we check first if he got hit before we set this state to stun
        if (IsCurrentState(1, "FullBody.Hit"))
            return;

        //Set all states besides base  to empty 
        SetStunValues();
    }

    public void BlockHit(Component sender, object obj)
    {
        var args = obj as AttackEventArgs;
        if (args == null) return;
        if (args.Defender == gameObject)
        {
            _animator.SetTrigger(P_BLOCKED_HIT);
        }
        if (args.Attacker == gameObject )
        {
            //Use play for an direct transition
            //_animator.Play(AnimationState.BlockedHit.ToString());
            _animator.CrossFade(AnimationState.BlockedHit.ToString(), 0f);
            _animator.SetFloat(P_BLOCKED_ATT, _animator.GetFloat(P_ATTACK_STATE));
        }            
    }

    public void RecoverStunned(Component Sender, object obj)
    {
        if (Sender.gameObject != gameObject) return;

        _animator.CrossFade(AnimationState.Idle.ToString(), 0.2f, 1, 0f);
        _animator.SetBool(P_Stun, false);

    }

    public void StopFullBodyAnim(Component Sender, object obj)
    {
        if (Sender.gameObject != gameObject) return;

        _animator.SetBool(P_FULL_BODY, false);
        _animator.CrossFade(AnimationState.Idle.ToString(), 0.2f, 1, 0f);
    }

    private bool IsCurrentState(int layer, string state)
    {
        if (_animator.IsInTransition(layer))
        {
            AnimatorStateInfo info = _animator.GetNextAnimatorStateInfo(layer);
            if (info.IsName(state))
                return true;
        }
        else
        {
            AnimatorClipInfo[] clipInfos = _animator.GetCurrentAnimatorClipInfo(layer);
            var currentState = clipInfos[0].clip.name;
            if (currentState == state)
                return true;
        }
        return false;
    }

    private void SetStunValues()
    {
        for (int i = 1; i < 4; i++)
        {
            if (i == 1)
                _animator.CrossFade(AnimationState.Stun.ToString(), 0.2f, i, 0f);
            else
                _animator.CrossFade(AnimationState.Empty.ToString(), 0.2f, i, 0f);
        }
        _animator.SetBool(P_Stun, true);
    }

}
