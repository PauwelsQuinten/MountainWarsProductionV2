using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class Attacker : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private GameEvent _attackEvent;
    [SerializeField] float _timeInterval = 1f;
    [Header("State")]
    [SerializeField] AttackType _attackType = AttackType.Stab;
    [SerializeField] AttackHeight _attackHeight = AttackHeight.Torso;
    [SerializeField] float _attackPower = 1f;
    
    private Coroutine _AttackCoroutine;


    void Start()
    {
        _AttackCoroutine = StartCoroutine(Attack(_timeInterval));
    }

    private IEnumerator Attack(float time)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            Attack();
        }
    }

    private void Attack()
    {
        var attackPackage = new AttackEventArgs
        {
            AttackType = _attackType
                   ,
            AttackHeight = _attackHeight
                   ,
            AttackPower = _attackPower
        };
        _attackEvent.Raise(this, attackPackage);
            Debug.Log("Attack send");
    }

}
