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
        if ((!_animator.GetBool(P_Stun) && !_animator.GetBool(P_GET_HIT)) || (args.AnimLayer == 2 ))
        {
            _animator.SetFloat(P_ACTION_SPEED, args.Speed);

            switch (args.AnimState)
            {
                case AnimationState.Stab:
                    SetAttackAnim(args, 2f);
                    break;
                case AnimationState.SlashLeft:                   
                    if (args.AttackWithLeftHand)
                        SetAttackAnim(args, 1f);
                    else
                        SetAttackAnim(args, 0f);
                    break;
                case AnimationState.SlashRight:
                    if (args.AttackWithLeftHand)
                        SetAttackAnim(args, 0f);
                    else
                        SetAttackAnim(args, 1f);
                    break;
                default:
                    if (!_animator.IsInTransition(args.AnimLayer))
                        _animator.CrossFade(args.AnimState.ToString(), 0.2f, args.AnimLayer, 0f);
                    else
                        _animator.Play(args.AnimState.ToString(), args.AnimLayer);
                    break;
            }
        }

        _currentState = args.AnimState;

        
    }

    private void SetAttackAnim(AnimationEventArgs args, float attNum)
    {
        _animator.SetBool(P_ATTACK_HEIGHT, args.IsAttackHigh);
        if (args.IsAttackHigh)
            _animator.CrossFade("AttackHigh", 0.1f, 1, 0f);
        else
            _animator.CrossFade("AttackLow", 0.1f, 1, 0f);

        _animator.SetBool(P_ATTACK_MEDIUM, args.AttackWithLeftHand);
        _animator.SetFloat(P_ATTACK_STATE, attNum);

        _attBlend = 1f;
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
            Vector3 animInput = Geometry.Geometry.CalculateVectorFromfOrientation(angleDiff) * _movementSpeed;
            _XVelocity = animInput.x;
            _YVelocity =  animInput.z;
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
        if (args.BlockDirection == Direction.Default)
            return false;

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
        if (Mathf.Abs(_animator.GetFloat(P_X_MOVEMENT)) < 0.05f)
            _animator.SetFloat(P_X_MOVEMENT, XVelocity);

        _animator.SetFloat(P_y_MOVEMENT, YVelocity, 0.1f, Time.deltaTime);
        if (Mathf.Abs(_animator.GetFloat(P_y_MOVEMENT)) < 0.05f)
            _animator.SetFloat(P_y_MOVEMENT, XVelocity);
        //_animator.SetFloat("AttackBlend", attBlend, 0.1f, Time.deltaTime);
    }

    private void InteruptAnimation(bool isFeint)
    {
        //if(isFeint) 
        //    _animator.SetTrigger(P_FEINT);
        //else
        //    _animator.ResetTrigger(P_FEINT);

        _animator.SetBool(P_FEINT, isFeint);
        
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

        _animator.SetFloat(P_ACTION_SPEED, 1f);
        _animator.SetFloat(P_HIT_HEIGHT, (float)(int)args.AttackHeight);

        //Transition block to hit when his block was badly aimed
        float dir = _animator.GetFloat(P_BLOCK_DIR);
        if (dir > 0.9f && dir < 3.1f)
            _animator.SetTrigger(P_GET_HIT);

        _animator.SetBool(P_Stun, true);

        //Set all states besides base to empty 
        for (int i = 1; i < 4; i++)
        {
            if (i == 1)
                _animator.CrossFade(AnimationState.Hit.ToString(), 0.2f, i, 0f);
            else
                _animator.CrossFade(AnimationState.Empty.ToString(), 0.2f, i, 0f);
        }
        
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
        if (_animator.GetBool(P_Stun))
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
        if (args.Attacker == gameObject && args.BlockPower > 0f)
        {
            //Use play for an direct transition
            //_animator.Play(AnimationState.BlockedHit.ToString());
            _animator.SetFloat(P_BLOCKED_ATT, _animator.GetFloat(P_ATTACK_STATE));
            if (_animator.GetBool(P_ATTACK_HEIGHT))
                _animator.CrossFade(AnimationState.AttackFeedFackHigh.ToString(), 0.1f);
            else
                _animator.CrossFade(AnimationState.AttackFeedFackLow.ToString(), 0.1f);
        }            
    }

    public void RecoverStunned(Component Sender, object obj)
    {
        if (Sender.gameObject != gameObject) return;

        _animator.SetBool(P_FULL_BODY, false);  
        _animator.CrossFade(AnimationState.Idle.ToString(), 0.2f, 1, 0f);
        _animator.SetBool(P_Stun, false);

        _currentState = AnimationState.Idle;
    }

    public void StopFullBodyAnim(Component Sender, object obj)
    {
        if (Sender.gameObject != gameObject) return;

        _animator.SetBool(P_FULL_BODY, false);
        _animator.CrossFade(AnimationState.Idle.ToString(), 0.2f, 1, 0f);
    }

    public void SetFullBodyAnim(Component Sender, object obj)
    {
        AttackMoveEventArgs args = obj as AttackMoveEventArgs;
        if (args == null || args.Attacker != gameObject) return; 

        _animator.SetBool(P_FULL_BODY, args.AttackType != AttackType.None);
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
        _animator.SetFloat(P_ACTION_SPEED, 1f);
    }

}
