using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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
    private NavMeshAgent _navMeshAgent= null;

    Vector3 _targetDir = Vector3.zero;
    Vector3 _targetPos = Vector3.zero;


    public override void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        base.StartAction(currentWorldState, blackboard);

        if (!_navMeshAgent)
            _navMeshAgent = npc.GetComponent<NavMeshAgent>();
        if (!_navMeshAgent)
            Debug.LogError("No navMeshagent found on the npc");

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

        Vector3 npcPos = currentWorldState.transform.position;
        float angleRad = (float)blackboard.variable.Orientation * Mathf.Deg2Rad;

        FindTargetPosAndDirection(blackboard, ref _targetDir, npcPos, ref _targetPos, ref angleRad);

        _navMeshAgent.SetDestination(_targetPos);
        _moveInput.Raise(this, new DirectionEventArgs { MoveDirection = _targetDir, SpeedMultiplier = 1f, Sender = npc });

        if (_navMeshAgent.isStopped)
            _navMeshAgent.isStopped = false;

    }

    public override void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (Time.timeScale == 0)
        {
            _navMeshAgent.isStopped = true;
            return;
        }

        Vector3 targetDir = Vector3.zero;
        Vector3 npcPos = currentWorldState.transform.position;
        Vector3 targetPos = Vector3.zero;
        float angleRad = (float)blackboard.variable.Orientation * Mathf.Deg2Rad;

        FindTargetPosAndDirection(blackboard, ref targetDir, npcPos, ref targetPos, ref angleRad);

        if (Vector3.Distance(targetPos, _targetPos) > 1f )
            _navMeshAgent.SetDestination(targetPos);
        if (Vector3.Distance(targetDir, _targetDir) > 1f )
            _moveInput.Raise(this, new DirectionEventArgs { MoveDirection = targetDir, SpeedMultiplier = 1f, Sender = npc });

        if (_navMeshAgent.isStopped)
            _navMeshAgent.isStopped = false;
    }

    public override bool IsCompleted(WorldState currentWorldState)
    {
        if (base.IsCompleted(currentWorldState))
        {
            _moveInput.Raise(this, new DirectionEventArgs { MoveDirection = Vector2.zero, SpeedMultiplier = 1f, Sender = npc });
            _navMeshAgent.isStopped = true;

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
        _navMeshAgent.isStopped = true;

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

    private void FindTargetPosAndDirection(BlackboardReference blackboard, ref Vector3 targetDir, Vector3 npcPos, ref Vector3 targetPos, ref float angleRad)
    {
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
                targetPos = targetDir + npcPos;
                break;

            case ObjectTarget.Backward:
                targetDir = -new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                targetPos = targetDir + npcPos;
                break;

            case ObjectTarget.Side:
                //angleRad *= _direction * 0.5f;
                angleRad += _direction * Mathf.PI * 0.5f;
                targetDir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                targetPos = targetDir + npcPos;
                break;

        }
    }

}