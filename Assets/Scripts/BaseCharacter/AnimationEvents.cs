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
    private void Start()
    {
        FindObjectOfType<FMODAudioHandler>();
    }

    public void Footstep()
    {
      _footstep.Raise();
    }

    public void Whoosh()
    {
        _whoosh.Raise();
    }

    public void SwordHit()
    {
        _recieveAttackEvent.Raise(this.transform.parent, null);
    }
    public void EndAnimation()
    {
        _endAnimation.Raise(this.transform.parent, null);
    }

}
