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
        Debug.Log("Swordhit signal");
    }
    public void EndAnimation()
    {
        if ( _endAnimation ) 
            _endAnimation.Raise(this.transform.parent, null);
    }
    public void SetInParryZone(int InZone)
    {
        bool zone = InZone == 0? true : false;
        _inParryZone.Raise(this.transform.parent, zone);
    }

}
