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

    private int _storredDirection = 0;
    private void Start()
    {
        
    }

    public void Footstep()
    {
      _footstep.Raise(this.transform.parent, EventArgs.Empty);
      
    }

    public void Whoosh()
    {
        _whoosh.Raise(this.transform.parent, EventArgs.Empty);;
    }

    public void SwordHit()
    {
        _recieveAttackEvent.Raise(this.transform.parent, null);
    }

    public void EndAnimation()
    {
        if ( _endAnimation ) 
            _endAnimation.Raise(this.transform.parent, null);
    }

    public void SetInParryZone(int InZone)
    {
        bool zone = InZone == 0? true : false;
        Debug.Log($"in parry zone signal = {zone}");

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
        _dragShieldDown.Raise(this.transform.parent, null);
    }
     
    public void SheatSword(int zeroForIn)
    {
        bool isSheating = zeroForIn == 0? true : false;
        _sheatSword.Raise(this.transform.parent, isSheating);
    }
     
    public void Pickup()
    {
        _pickup.Raise(this.transform.parent, null);
    }

}
