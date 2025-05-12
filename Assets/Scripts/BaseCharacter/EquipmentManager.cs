using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    private const string PLAYER = "Player";

    [Header("EquipmentPrefabs")]
    [SerializeField] private Equipment _leftHand;
    [SerializeField] private Equipment _rightHand;
    [SerializeField] private Equipment _fists;
    [Header("Events")]
    [SerializeField] private GameEvent _onEquipmentBreak;
    [SerializeField] private GameEvent _onEquipmentDamage;
    [SerializeField] private GameEvent _changeAnimation;
    [SerializeField] private GameEvent _changeIKStance;
    [Header("Sockets"), Tooltip("These are the sockets that will hold the equipment")]
    [SerializeField] private Transform _leftHandSocket;
    [SerializeField] private Transform _rightHandSocket;
    [SerializeField] private Transform _sheathSocket;
    [Header("Item")]
    [SerializeField] private LayerMask _itemMask;
    [SerializeField] private float _itemPickupRadius = 1f;
    [Header("ShieldPosition")]
    [SerializeField] private float _startAngle = 195f;
    [SerializeField] private float _sideAngleToStart = 60f;
    
    [Header("EquipmentPosition")]
    //[SerializeField] private Quaternion _swordStartRotation = Quaternion.Euler(-32f, -116f, -195f);
    [SerializeField] private Quaternion _spearStartRotation = Quaternion.Euler(15f, -160f, -45);
    [Header("Blackboard")]
    [SerializeField]
    private List<BlackboardReference> _blackboards;

    private List<Equipment> HeldEquipment = new List<Equipment> {null, null, null };

    private const int LEFT_HAND = 0;
    private const int RIGHT_HAND = 1;
    private const int FISTS = 2;

    private StateManager _stateManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_fists && _fists.Type == EquipmentType.Fist)
        {
            var equipment = Instantiate(_fists);
            EquipmentHelper.CreateAndEquip(HeldEquipment, equipment, FISTS, null, transform);
        }

        if (_rightHand && _rightHand.EquipmentHand == EquipmentHand.RightHand)
        {
            var equipment = Instantiate(_rightHand);
            EquipmentHelper.CreateAndEquip(HeldEquipment, equipment, RIGHT_HAND, _rightHandSocket, transform);

            if (_rightHand.EquipmentHand == EquipmentHand.TwoHanded)
            {
                HeldEquipment[RIGHT_HAND].transform.localRotation = _spearStartRotation;
                DisableAimingScript(_rightHand.EquipmentHand);
                _changeAnimation.Raise(this, true);
                _changeIKStance?.Raise(this, new ChangeIKStanceEventArgs { UseSpear = true, LHSocket = equipment.WeaponSocket });

                _stateManager = GetComponent<StateManager>();
                UpdateBlackboard();
                return;
            }
            else
            {
                DisableAimingScript(_rightHand.EquipmentHand);
                HeldEquipment[RIGHT_HAND].transform.localRotation = Quaternion.identity;
                _changeAnimation?.Raise(this, false);
                _changeIKStance?.Raise(this, new ChangeIKStanceEventArgs { UseSpear = false });
            }
        }

        if (_leftHand && _leftHand.EquipmentHand == EquipmentHand.LeftHand )
        {
            var equipment = Instantiate(_leftHand);
            EquipmentHelper.CreateAndEquip(HeldEquipment, equipment, LEFT_HAND, _leftHandSocket, transform);
            HeldEquipment[LEFT_HAND].transform.localRotation = Quaternion.identity;

        }

        _stateManager = GetComponent<StateManager>();
        UpdateBlackboard();
    }

    public void LoseEquipment(Component sender, object obj)
    {
        var args = obj as LoseEquipmentEventArgs;
        if (args == null) return;

        if (args.ToSelf && sender.gameObject != gameObject) return;
        if (!args.ToSelf && sender.gameObject == gameObject) return;

        int hand = 0;
        switch (args.EquipmentType)
        {
            case EquipmentType.Ranged:
            case EquipmentType.Melee:
                hand = RIGHT_HAND;
                _changeAnimation.Raise(this, false);
                _changeIKStance?.Raise(this, new ChangeIKStanceEventArgs { UseSpear = false });
                break;
              
            case EquipmentType.Shield:
                hand = LEFT_HAND;
                break;

            case EquipmentType.Fist:
                return;
        }

        EquipmentHelper.DropEquipment(HeldEquipment, hand);
    }

    public void CheckDurability(Component sender, object obj)
    {
        //Check for vallid signal
        DefenceEventArgs args = obj as DefenceEventArgs;
        if (args == null) return;

        if (sender.gameObject == gameObject)
            BlockMediumReduction(args);
        else
            AttackMediumReduction(args);

        UpdateBlackboard();
    }

    private void AttackMediumReduction(DefenceEventArgs args)
    {
        //Attackmedium
        int attackIndex = 2;
        if (HeldEquipment[RIGHT_HAND])
            attackIndex = RIGHT_HAND;
        else if (!HeldEquipment[RIGHT_HAND] && HeldEquipment[LEFT_HAND])
            attackIndex = LEFT_HAND;
        else
            attackIndex = FISTS;

        //Reduce durability
        HeldEquipment[attackIndex].Damage(args.AttackPower, args.BlockResult, false);
        _onEquipmentDamage.Raise(this, new EquipmentEventArgs
        {
            ShieldDurability = GetDurabilityPercentage(LEFT_HAND),
            WeaponDurability = GetDurabilityPercentage(RIGHT_HAND)
        });

        LoseEquipmentEventArgs send = null;
        if (EquipmentHelper.CheckIfBroken(args, attackIndex, HeldEquipment, out send))
        {
            Destroy(HeldEquipment[attackIndex].gameObject);
            HeldEquipment[attackIndex] = null;
            _onEquipmentBreak.Raise(this, send);
        }
    }

    private void BlockMediumReduction(DefenceEventArgs args)
    {
        //Reduce durability
        int index = 2;
        switch (args.BlockMedium)
        {
            case BlockMedium.Shield:
                index = LEFT_HAND;
                break;
            case BlockMedium.Sword:
                index = RIGHT_HAND;
                break;
            case BlockMedium.Nothing:
                index = FISTS;
                break;
        }
        HeldEquipment[index].Damage(args.AttackPower, args.BlockResult, true);
        _onEquipmentDamage.Raise(this, new EquipmentEventArgs
        {
            ShieldDurability = GetDurabilityPercentage(LEFT_HAND),
            WeaponDurability = GetDurabilityPercentage(RIGHT_HAND)
        });

        LoseEquipmentEventArgs send = null;
        if(EquipmentHelper.CheckIfBroken(args, index, HeldEquipment, out send))
        {
            Destroy(HeldEquipment[index].gameObject);
            HeldEquipment[index] = null;
            _onEquipmentBreak.Raise(this, send);
        }        
    }


    private void UpdateBlackboard()
    {
        //Update blackboard
        if (gameObject.CompareTag(PLAYER))
        {
            foreach (var blackboard in _blackboards)
            {
                blackboard.variable.TargetLHEquipmentHealth = GetDurabilityPercentage(LEFT_HAND);
                blackboard.variable.TargetRHEquipmentHealth = GetDurabilityPercentage(RIGHT_HAND);
                blackboard.variable.TargetWeaponRange = GetAttackRange();
            }
        }

        else
        {
            _blackboards[0].variable.LHEquipmentHealth = GetDurabilityPercentage(LEFT_HAND);
            _blackboards[0].variable.RHEquipmentHealth = GetDurabilityPercentage(RIGHT_HAND);
            _blackboards[0].variable.WeaponRange = GetAttackRange();
            _blackboards[0].variable.HasRHEquipment = HeldEquipment[RIGHT_HAND] != null;
            _blackboards[0].variable.HasLHEquipment = HeldEquipment[LEFT_HAND] != null;
        }
    }

    public void PickupEquipment(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _itemPickupRadius, _itemMask);
        if (hitColliders.Length == 0) return; 

        foreach (var hitCollider in hitColliders)
        {
            var newEquip = hitCollider.gameObject.GetComponent<Equipment>();
            if (newEquip && newEquip.transform.parent == null)
            {
                Transform socket = newEquip.EquipmentHand == EquipmentHand.LeftHand? _leftHandSocket : _rightHandSocket ;
                EquipmentHelper.EquipEquipment(HeldEquipment, newEquip, newEquip.EquipmentHand, socket);
            }
        }

        //Update which aiming script to use on the new equipment and fighting stance
        var hand = HeldEquipment[RIGHT_HAND];
        if (hand)
        {
            if (hand.EquipmentHand == EquipmentHand.TwoHanded)
            {
                hand.transform.localRotation = _spearStartRotation;
                _changeAnimation.Raise(this, true);
                _changeIKStance?.Raise(this, new ChangeIKStanceEventArgs { UseSpear = true, LHSocket = hand.WeaponSocket });
            }

            else
            {
                hand.transform.localRotation = Quaternion.identity;
                _changeAnimation.Raise(this, false);
                _changeIKStance?.Raise(this, new ChangeIKStanceEventArgs { UseSpear = false });
            }
            DisableAimingScript(hand.EquipmentHand);
        }
        else
            DisableAimingScript(EquipmentHand.RightHand);

        _onEquipmentDamage.Raise(this, new EquipmentEventArgs
        {
            ShieldDurability = GetDurabilityPercentage(LEFT_HAND),
            WeaponDurability = GetDurabilityPercentage(RIGHT_HAND)
        });
    }

    public void RotateShield(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        if (HeldEquipment[LEFT_HAND] == null) return;

        Direction blockDirection = (Direction)obj;

        float diffInRealOrientation = (int)_stateManager.Orientation - _stateManager.fOrientation;
        switch (blockDirection)
        {
            case Direction.Idle:
            case Direction.Wrong:
            case Direction.ToCenter:
                HeldEquipment[LEFT_HAND].transform.localRotation = Quaternion.Euler(_startAngle + diffInRealOrientation, 0f, 0f);
                break;
            case Direction.ToRight:
                HeldEquipment[LEFT_HAND].transform.localRotation = Quaternion.Euler(_startAngle + _sideAngleToStart + diffInRealOrientation, 0f, 0f);
                break;
            case Direction.ToLeft:
                HeldEquipment[LEFT_HAND].transform.localRotation = Quaternion.Euler(_startAngle - _sideAngleToStart + diffInRealOrientation, 0f, 0f);
                break;
        }


    }

    public void RotateSword(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        if (HeldEquipment[RIGHT_HAND] == null) return;

        //float diffInRealOrientation = (int)_stateManager.Orientation - _stateManager.fOrientation;
        //bool rotate = (int)obj == 1? true : false;
        //
        //if (rotate)
        //{
        //    HeldEquipment[RIGHT_HAND].transform.localRotation = Quaternion.Euler(-26, -27, 50);
        //    //HeldEquipment[RIGHT_HAND].transform.Rotate(Vector3.right, diffInRealOrientation);
        //}
        //else
        //{
        //    HeldEquipment[RIGHT_HAND].transform.localRotation = Quaternion.identity;
        //}

    }


    public bool HasFullEquipment()
    {
        return HeldEquipment[RIGHT_HAND] && HeldEquipment[LEFT_HAND];
    }

    public Equipment GetEquipment(bool isRighthand)
    {
        int index = isRighthand ? 1 : 0;
        return HeldEquipment[index];
    }

    private void DisableAimingScript(EquipmentHand hand)
    {
        var aimingComp = GetComponent<Aiming>();
        var aimingSpearComp = GetComponent<SpearAiming>();

        if (hand == EquipmentHand.TwoHanded)
        {
            if (aimingComp)
                aimingComp.SetActive(false);
            if (aimingSpearComp)
                aimingSpearComp.SetActive(true, HeldEquipment[RIGHT_HAND]);
        }
        else
        {
            if (aimingSpearComp)
                aimingSpearComp.SetActive(false, null);
            if (aimingComp)
                aimingComp.SetActive(true);
        }
       
    }
    
    public bool HasEquipmentInHand(bool isRighthand)
    {
        int index = isRighthand ? 1 : 0;
        return HeldEquipment[index] != null;
    }
    
    public bool HasNoneInHand()
    {
        return HeldEquipment[RIGHT_HAND] == null && HeldEquipment[LEFT_HAND] == null;
    }
   

    public float GetEquipmentPower()
    {
        if (HeldEquipment[RIGHT_HAND])
            return HeldEquipment[RIGHT_HAND].Power;
        else if (!HeldEquipment[RIGHT_HAND] && HeldEquipment[LEFT_HAND])
            return HeldEquipment[LEFT_HAND].Power;
        return HeldEquipment[FISTS].Power;
    }

    public float GetAttackRange()
    {
        if (HeldEquipment[RIGHT_HAND])
            return HeldEquipment[RIGHT_HAND].Range;
        else if (!HeldEquipment[RIGHT_HAND] && HeldEquipment[LEFT_HAND])
            return HeldEquipment[LEFT_HAND].Range;
        return HeldEquipment[FISTS].Range;
    }

    private float GetDurabilityPercentage(int index)
    {
        if (HeldEquipment[index])
            return HeldEquipment[index].GetDurabilityPercentage();
        return 0f;
    }

    public void SheathWeapon(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        if (HeldEquipment[RIGHT_HAND] == null) return;
        if (_stateManager.WeaponIsSheathed)
        {
            HeldEquipment[RIGHT_HAND].gameObject.transform.parent = _rightHandSocket.transform;
            HeldEquipment[RIGHT_HAND].gameObject.transform.localPosition = Vector3.zero;
            HeldEquipment[RIGHT_HAND].gameObject.transform.localRotation = Quaternion.Euler(new Vector3(48, 108, 194));
            _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.DrawWeapon, AnimLayer = 3, DoResetIdle = true });
            _stateManager.WeaponIsSheathed = false;
        }
        else
        {
            StartCoroutine(SetWeaponActive(34f / 30f, _sheathSocket.gameObject));
            _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.SheathWeapon, AnimLayer = 3, DoResetIdle = true });
            _stateManager.WeaponIsSheathed = true;
        }
    }
   
    private IEnumerator SetWeaponActive(float duration, GameObject socket)
    {
        yield return new WaitForSeconds(duration);
        HeldEquipment[RIGHT_HAND].gameObject.transform.parent = socket.transform;
        HeldEquipment[RIGHT_HAND].gameObject.transform.localPosition = Vector3.zero;
        HeldEquipment[RIGHT_HAND].gameObject.transform.localRotation = Quaternion.identity;
    }
}
