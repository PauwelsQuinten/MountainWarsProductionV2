using NUnit.Framework;
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
    [SerializeField, Tooltip("when MoveTo is set to PatrolPoints, he will take from this starting from 0")]
    private string _patrolName;
    private List<Transform> _patrolPoints;

    private List<Equipment> _foundEquipment = new List<Equipment>();
    private Equipment _foundSpecificEquipment;
    private int _direction = 0;
    private NavMeshAgent _navMeshAgent= null;
    private int _patrolIndex = 0;

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
            case ObjectTarget.PatrolPoint:
                if (_patrolPoints != null && _patrolPoints.Count > 0) break;
                _patrolPoints = new List<Transform>();
                var obj = GameObject.Find(_patrolName);
                foreach (Transform child in obj.transform)
                {
                    _patrolPoints.Add(child);
                }


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

        if (!_navMeshAgent.isActiveAndEnabled)return;

        _moveInput.Raise(this, new DirectionEventArgs { MoveDirection = _targetDir, SpeedMultiplier = 1f, Sender = npc });
        _navMeshAgent.SetDestination(_targetPos);

        ReactivateAgent();

    }

    public override void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (!_navMeshAgent.isActiveAndEnabled) return;

        if (Time.timeScale == 0)
        {
            _navMeshAgent.isStopped = true;
            return;
        }

        Vector3 targetDir = Vector3.zero;
        Vector3 npcPos = currentWorldState.transform.position;
        Vector3 targetPos = Vector3.zero;
        float angleRad = (float)blackboard.variable.Orientation * Mathf.Deg2Rad;

        if (Vector2.Distance(new Vector2 ( npcPos.x, npcPos.z) , new Vector2(_targetPos.x, _targetPos.z)) < 0.5f 
            && _patrolPoints.Count > 0)
        {
            _patrolIndex++;
            _patrolIndex %= _patrolPoints.Count;
        }

        FindTargetPosAndDirection(blackboard, ref targetDir, npcPos, ref targetPos, ref angleRad);

        //Update target position when he moves around
        if (Vector2.Distance(new Vector2(npcPos.x, npcPos.z), new Vector2(targetPos.x, targetPos.z)) > 1f
            || Vector3.Angle(targetDir, _targetDir) > 25f)
        {
            _navMeshAgent.SetDestination(targetPos);
            _targetPos = targetPos;
            _targetDir = targetDir;
            _moveInput.Raise(this, new DirectionEventArgs { MoveDirection = targetDir, SpeedMultiplier = 1f, Sender = npc });
            ReactivateAgent();
        }

        
    }

    public override bool IsCompleted(WorldState currentWorldState)
    {
        if (!_navMeshAgent.isActiveAndEnabled)
            return false;

        if (base.IsCompleted(currentWorldState))
        {
            StopMovement();

            return true;
        }
        return false;
    }

    public override bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard)
    {
        /*return (blackboard.variable.TargetState == AttackState.Attack || blackboard.variable.TargetState == AttackState.BlockAttack)
            && currentWorldState.TargetAttackRange == EWorldStateRange.InRange;*/
        bool interupt = false;
        switch (_MoveTo)
        {
            case ObjectTarget.Player:
                interupt = currentWorldState.HasTarget != EWorldStatePossesion.InPossesion;
                break;
            
            case ObjectTarget.PatrolPoint:
                interupt = currentWorldState.HasTarget == EWorldStatePossesion.InPossesion;
                break;
        }

        return interupt;
    }

    public override bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (_MoveTo == ObjectTarget.Side)
            Cost = Random.Range(0.5f, 1f);

        bool vallid = true;
        switch (_MoveTo)
        {
            case ObjectTarget.Player:
                if (currentWorldState.HasTarget != EWorldStatePossesion.InPossesion)
                    vallid = false;
                break;
        }

        return vallid;
    }

    public override void CancelAction()
    {
        StopMovement();

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

            case ObjectTarget.PatrolPoint:
                if (_patrolPoints == null || _patrolPoints.Count < 0) return;
                targetPos = _patrolPoints[_patrolIndex].transform.position;
               

                targetDir = targetPos - npcPos;
                targetDir = new Vector2(targetDir.x, targetDir.z);
                targetDir.Normalize(); 
                break;
        }
    }
    private void StopMovement()
    {
        _moveInput.Raise(this, new DirectionEventArgs { MoveDirection = Vector2.zero, SpeedMultiplier = 1f, Sender = npc });
        if (_navMeshAgent.isActiveAndEnabled)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.velocity = Vector3.zero;
            _navMeshAgent.updatePosition = false;
            _navMeshAgent.updateRotation = false;
        }       
        npc.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }

    private void ReactivateAgent()
    {
        if (_navMeshAgent.isStopped)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.updatePosition = true;
            _navMeshAgent.updateRotation = true;
        }
    }


}