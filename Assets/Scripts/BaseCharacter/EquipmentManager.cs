using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Rendering;

public class EquipmentManager : MonoBehaviour
{
    private const string PLAYER = "Player";

    [SerializeField] private Equipment _leftHand;
    [SerializeField] private Equipment _rightHand;
    [SerializeField] private Equipment _fists;
    [Header("Events")]
    [SerializeField] private GameEvent _onEquipmentBreak;
    [SerializeField] private GameEvent _onEquipmentDamage;
    [SerializeField] private GameEvent _changeAnimation;
    [Header("Sockets")]
    [SerializeField] private Transform _leftHandSocket;
    [SerializeField] private Transform _rightHandSocket;
    [SerializeField] private Transform _sheathSocket;
    [Header("Item")]
    [SerializeField] private LayerMask _itemMask;
    [Header("Blackboard")]
    [SerializeField]
    private BlackboardReference _blackboard;

    private List<Equipment> HeldEquipment = new List<Equipment> {null, null, null };

    private const int LEFT_HAND = 0;
    private const int RIGHT_HAND = 1;
    private const int FISTS = 2;

    private Equipment _discoverdEquipment;

    private StateManager _stateManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (_leftHand && !_leftHand.IsRightHandEquipment)
        {
            var leftEquipment = Instantiate(_leftHand);
            if (_leftHandSocket)
                leftEquipment.transform.parent = _leftHandSocket;
            else
                leftEquipment.transform.parent = transform;

            leftEquipment.transform.localPosition = Vector3.zero;
            HeldEquipment[LEFT_HAND] = leftEquipment;
        }


        if (_rightHand && _rightHand.IsRightHandEquipment)
        {
            var rightEquipment = Instantiate(_rightHand);
            if (_rightHandSocket)
                rightEquipment.transform.parent = _rightHandSocket;
            else
                rightEquipment.transform.parent = transform; 
            rightEquipment.transform.localPosition = Vector3.zero;
            HeldEquipment[RIGHT_HAND] = rightEquipment;
        }


        if (_fists && _fists.Type == EquipmentType.Fist)
        {
            var fist = Instantiate(_fists);
            fist.transform.parent = transform;
            fist.transform.localPosition = Vector3.zero;
            HeldEquipment[FISTS] = fist;
        }

        UpdateBlackboard();
    }

    public void LoseEquipment(Component sender, object obj)
    {
        var args = obj as LoseEquipmentEventArgs;
        if (args == null) return;

        if (args.ToSelf && sender.gameObject != gameObject) return;
        if (!args.ToSelf && sender.gameObject == gameObject) return;

        bool isRighthand = false;
        switch (args.EquipmentType)
        {
            case EquipmentType.Ranged:
            case EquipmentType.Melee:
                isRighthand = true;
                break;
              
            case EquipmentType.Shield:
                isRighthand = false;
                break;

            case EquipmentType.Fist:
                return;
        }

        DropEquipment(isRighthand);
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

        CheckIfBroken(args, attackIndex);
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

        CheckIfBroken(args, index);
    }

    private void CheckIfBroken(DefenceEventArgs args, int index)
    {
        //Check if broken
        if (HeldEquipment[index].Durability < 0f)
        {
            Debug.Log($"!!!!!!!!!!!!!!!!!!!!! breaks {HeldEquipment[index]} !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Destroy(HeldEquipment[index].gameObject);
            HeldEquipment[index] = null;

            var send = new LoseEquipmentEventArgs
            {
                EquipmentType = args.BlockMedium == BlockMedium.Shield ? EquipmentType.Shield : EquipmentType.Melee,
                ToSelf = true
            };
            _onEquipmentBreak.Raise(this, send);
        }
    }

    private void UpdateBlackboard()
    {
        //Update blackboard
        if (gameObject.CompareTag(PLAYER))
        {
            _blackboard.variable.TargetLHEquipmentHealth = GetDurabilityPercentage(LEFT_HAND);
            _blackboard.variable.TargetRHEquipmentHealth = GetDurabilityPercentage(RIGHT_HAND);
            _blackboard.variable.TargetWeaponRange = GetAttackRange();
        }

        else
        {
            _blackboard.variable.LHEquipmentHealth = GetDurabilityPercentage(LEFT_HAND);
            _blackboard.variable.RHEquipmentHealth = GetDurabilityPercentage(RIGHT_HAND);
            _blackboard.variable.WeaponRange = GetAttackRange();
            _blackboard.variable.HasRHEquipment = HeldEquipment[RIGHT_HAND] != null;
            _blackboard.variable.HasLHEquipment = HeldEquipment[LEFT_HAND] != null;
        }
    }

    public void PickupEquipment(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        float radius = 2f;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, _itemMask);
        foreach (var hitCollider in hitColliders)
        {
            var newEquip = hitCollider.gameObject.GetComponent<Equipment>();
            if (newEquip && newEquip.transform.parent == null)
            {
                if (newEquip.IsRightHandEquipment)
                {
                    DropEquipment(true);
                    HeldEquipment[RIGHT_HAND] = newEquip;
                    newEquip.transform.parent = _rightHandSocket;
                    newEquip.transform.localPosition = Vector3.zero;
                }

                else if (!newEquip.IsRightHandEquipment)
                {
                    DropEquipment(false);
                    HeldEquipment[LEFT_HAND] = newEquip;
                    newEquip.transform.parent = _leftHandSocket;
                    newEquip.transform.localPosition = Vector3.zero;
                }
            }
        }
    }
    public void RotateShield(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        if (HeldEquipment[LEFT_HAND] == null) return;

        Direction blockDirection = (Direction)obj;
        switch (blockDirection)
        {
            case Direction.Idle:
                HeldEquipment[LEFT_HAND].transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
                break;
            case Direction.ToRight:
                HeldEquipment[LEFT_HAND].transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
                break;
            case Direction.ToLeft:
                HeldEquipment[LEFT_HAND].transform.localRotation = Quaternion.Euler(-15f, 0f, 0f);
                break;
            case Direction.ToCenter:
                HeldEquipment[LEFT_HAND].transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
                break;
            case Direction.Wrong:
                HeldEquipment[LEFT_HAND].transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
                break;
        }


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
    
    public bool HasEquipmentInHand(bool isRighthand)
    {
        int index = isRighthand ? 1 : 0;
        return HeldEquipment[index] != null;
    }
    
    public bool HasNoneInHand()
    {
        return HeldEquipment[RIGHT_HAND] == null && HeldEquipment[LEFT_HAND] == null;
    }
    
    private void DropEquipment(bool isRightHand)
    {
        int index = isRightHand ? 1 : 0;
        if (HeldEquipment[index] == null)
            return;
        HeldEquipment[index].transform.parent = null; 
        HeldEquipment[index] = null; 
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
        if(_stateManager == null) _stateManager = GetComponent<StateManager>();
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

    public void ShieldGrabbed(Component sender, object obj)
    {
        if (sender.gameObject == this.gameObject) return;
        if (Vector3.Distance(gameObject.transform.position, sender.gameObject.transform.position) > 3) return;
        if (_stateManager == null) _stateManager = GetComponent<StateManager>();
        _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Empty, AnimLayer = 3, DoResetIdle = true });
        _stateManager.AttackState = AttackState.Idle;
    }

    private IEnumerator SetWeaponActive(float duration, GameObject socket)
    {
        yield return new WaitForSeconds(duration);
        HeldEquipment[RIGHT_HAND].gameObject.transform.parent = socket.transform;
        HeldEquipment[RIGHT_HAND].gameObject.transform.localPosition = Vector3.zero;
        HeldEquipment[RIGHT_HAND].gameObject.transform.localRotation = Quaternion.identity;
    }
}
