using System;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [Header("Events")]
    [SerializeField]
    private GameEvent _footstep;
    [SerializeField]
    private GameEvent _whoosh;
    [SerializeField] 
    private GameEvent _recieveAttackEvent;
    [SerializeField] 
    private GameEvent _endAnimation;
    [SerializeField] 
    private GameEvent _inParryZone;
    [SerializeField] 
    private GameEvent _rotateShield;
    [SerializeField] 
    private GameEvent _dragShieldDown;
    [SerializeField] 
    private GameEvent _sheatSword;
    [SerializeField] 
    private GameEvent _pickup;
    [SerializeField] 
    private GameEvent _stunned;
    [SerializeField] 
    private GameEvent _moveAttack;
    private int _storredDirection = 0;
    private void Start()
    {
        
    }

    public void Footstep()
    {
        if (_footstep && transform.parent) 
            _footstep.Raise(this.transform.parent, EventArgs.Empty);
    }

    public void Whoosh()
    {
        if (_whoosh && transform.parent) 
            _whoosh.Raise(this.transform.parent, EventArgs.Empty);;
    }

    public void SwordHit()
    {
        if (_recieveAttackEvent && transform.parent) 
            _recieveAttackEvent.Raise(this.transform.parent, null);
    }

    public void EndAnimation()
    {
        if (_endAnimation && transform.parent) 
            _endAnimation.Raise(this.transform.parent, null);
    }

    public void SetInParryZone(int InZone)
    {
        bool zone = InZone == 0? true : false;
        Debug.Log($"in parry movement signal = {zone}");

        if (_inParryZone && transform.parent)
            _inParryZone.Raise(this.transform.parent, zone);
    }

    public void RotateShield(int direction)
    {
        if (direction != _storredDirection)
        {
            _storredDirection = direction;
            _rotateShield.Raise(this.transform.parent, direction);
        }
        
    }
    
    public void DragShield()
    {

        if (_dragShieldDown && transform.parent)
            _dragShieldDown.Raise(this.transform.parent, null);
    }
     
    public void SheatSword(int zeroForIn)
    {
        bool isSheating = zeroForIn == 0? true : false;

        if (_sheatSword && transform.parent)
            _sheatSword.Raise(this.transform.parent, isSheating);
    }
     
    public void Pickup()
    {
        if (_pickup && transform.parent)
            _pickup.Raise(this.transform.parent, null);
    }

    public void Stunned()
    {
        if (_stunned && transform.parent)
            _stunned?.Raise(this.transform.parent, null);
    }
    
    public void MoveBack(int forward)
    {
        //To move Back
        if (forward == 0)
            _moveAttack.Raise(this, new AttackMoveEventArgs { Attacker = transform.parent.gameObject });

        //To move Forward
        else
            _moveAttack.Raise(this, new AttackMoveEventArgs { Attacker = transform.parent.gameObject, AttackType = AttackType.Stab });
        Debug.Log($"Move 1 = f, 0 = b. {forward}, {transform.parent}");
    }

    public void Confusion()
    {

    }
}
