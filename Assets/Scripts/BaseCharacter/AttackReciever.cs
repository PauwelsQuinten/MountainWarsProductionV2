using UnityEngine;

public class AttackReciever : MonoBehaviour
{
    [SerializeField] private GameEvent _recieveAttackEvent;

    public void SwordHit()
    {
        _recieveAttackEvent.Raise(this.transform.parent, null);
    }
}
