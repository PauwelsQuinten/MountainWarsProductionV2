using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class MoveToAction : GoapAction
{
    [SerializeField] private ObjectTarget _MoveTo = ObjectTarget.Player;
    [Header("Input")]
    [SerializeField] private MovingInputReference _moveInput;

    GameObject npc;
    private List<Equipment> _foundEquipment = new List<Equipment>();
    private Equipment _foundSpecificEquipment;
    private int _direction = 0;

    public override void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        base.StartAction(currentWorldState, blackboard);

        npc = blackboard.variable.Self;
        
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
        float angleRad = (float)blackboard.variable.Orientation;

        switch (_MoveTo)
        {
            case ObjectTarget.Player:
                var target = blackboard.variable.Target;
                if (target)
                    targetPos = target.transform.position;

                targetDir = targetPos - npcPos;
                targetDir.Normalize();
                break;

            case ObjectTarget.Weapon:
                if (_foundSpecificEquipment)
                    targetPos = _foundSpecificEquipment.transform.position;

                targetDir = targetPos - npcPos;
                targetDir.Normalize();
                break;

            case ObjectTarget.Shield:
                if (_foundSpecificEquipment)
                    targetPos = _foundSpecificEquipment.transform.position;

                targetDir = targetPos - npcPos;
                targetDir.Normalize();
                break;

            case ObjectTarget.Forward:
                targetDir = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);
                break;

            case ObjectTarget.Backward:
                targetDir = -new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);
                break;

            case ObjectTarget.Side:
                //angleRad *= _direction * 0.5f;
                angleRad += _direction * Mathf.PI * 0.5f;
                targetDir = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);
                break;

        }
        _moveInput.variable.value = targetDir;
        
    }

    public override bool IsCompleted(WorldState currentWorldState)
    {
        if (base.IsCompleted(currentWorldState))
        {
            _moveInput.variable.value = Vector2.zero;

            return true;
        }
        return false;
    }

    public override bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard)
    {
        //return AboutToBeHit(currentWorldState) || blackboard.variable.ObservedAttack == ;
        return (blackboard.variable.TargetState == AttackState.Attack || blackboard.variable.TargetState == AttackState.BlockAttack)
            && currentWorldState.TargetAttackRange == EWorldStateRange.InRange;
    }

    public override bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (_MoveTo == ObjectTarget.Side)
            Cost = Random.Range(0.5f, 1f);
        return true;
    }

    public override void CancelAction()
    {
        _moveInput.variable.value = Vector2.zero;
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