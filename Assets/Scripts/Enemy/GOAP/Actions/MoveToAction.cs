using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;


public class MoveToAction : GoapAction
{
    [SerializeField] private ObjectTarget _MoveTo = ObjectTarget.Player;
    [Header("Input")]
    [SerializeField] private GameEvent _moveInput;

    private List<Equipment> _foundEquipment = new List<Equipment>();
    private Equipment _foundSpecificEquipment;
    private int _direction = 0;

    public override void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        base.StartAction(currentWorldState, blackboard);

        
        switch (_MoveTo)
        {
            case ObjectTarget.Weapon:
                _foundSpecificEquipment = FindEquipmentOfType(EquipmentType.Melee);
                //currentWorldState.FoundEquipment(_foundSpecificEquipment);
                break;

            case ObjectTarget.Shield:
                _foundSpecificEquipment = FindEquipmentOfType(EquipmentType.Shield);
                //currentWorldState.FoundEquipment(_foundSpecificEquipment);
                break;

            case ObjectTarget.Player:
            case ObjectTarget.Forward:
                break;
            case ObjectTarget.Backward:
                break;
            case ObjectTarget.Side:
                _direction = (Random.Range(0, 2) == 0) ? 1 : -1;
                break;
        }
    }

    public override void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        Vector3 targetDir = Vector3.zero;
        Vector3 npcPos = currentWorldState.transform.position;
        Vector3 targetPos = Vector3.zero;
        float angleRad = (float)blackboard.variable.Orientation * Mathf.Deg2Rad;

        switch (_MoveTo)
        {
            case ObjectTarget.Player:
                var target = blackboard.variable.Target;
                if (target)
                    targetPos = target.transform.position;

                targetDir = targetPos - npcPos;
                targetDir.Normalize();
                targetDir = new Vector2(targetDir.x, targetDir.z);
                break;

            case ObjectTarget.Weapon:
                if (_foundSpecificEquipment)
                    targetPos = _foundSpecificEquipment.transform.position;

                targetDir = targetPos - npcPos;
                targetDir = new Vector2(targetDir.x, targetDir.z);
                targetDir.Normalize();
                break;

            case ObjectTarget.Shield:
                if (_foundSpecificEquipment)
                    targetPos = _foundSpecificEquipment.transform.position;

                targetDir = targetPos - npcPos;
                targetDir = new Vector2(targetDir.x, targetDir.z);
                targetDir.Normalize();
                break;

            case ObjectTarget.Forward:
                targetDir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                break;

            case ObjectTarget.Backward:
                targetDir = -new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                break;

            case ObjectTarget.Side:
                //angleRad *= _direction * 0.5f;
                angleRad += _direction * Mathf.PI * 0.5f;
                targetDir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                break;

        }
        _moveInput.Raise(this, new DirectionEventArgs{ MoveDirection = targetDir, SpeedMultiplier = 1f, Sender = npc });
        
    }

    public override bool IsCompleted(WorldState currentWorldState)
    {
        if (base.IsCompleted(currentWorldState))
        {
            _moveInput.Raise(this, new DirectionEventArgs { MoveDirection = Vector2.zero, SpeedMultiplier = 1f, Sender = npc });

            return true;
        }
        return false;
    }

    public override bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard)
    {
        /*return (blackboard.variable.TargetState == AttackState.Attack || blackboard.variable.TargetState == AttackState.BlockAttack)
            && currentWorldState.TargetAttackRange == EWorldStateRange.InRange;*/
        return false;
    }

    public override bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (_MoveTo == ObjectTarget.Side)
            Cost = Random.Range(0.5f, 1f);
        return true;
    }

    public override void CancelAction()
    {
        _moveInput.Raise(this, new DirectionEventArgs { MoveDirection = Vector2.zero, SpeedMultiplier = 1f, Sender = npc });
    }


    private Equipment FindEquipmentOfType(EquipmentType type)
    {
        Equipment[] foundStuff = GameObject.FindObjectsByType<Equipment>(FindObjectsSortMode.None);
        _foundEquipment = new List<Equipment>(foundStuff);

        foreach (Equipment equipment in foundStuff)
        {
            if (equipment.Type == type && equipment.GetComponent<SphereCollider>().enabled)
                return equipment;
        }
        return null;

    }

}