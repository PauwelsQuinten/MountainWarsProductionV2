using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [Header("Events")]
    [SerializeField]
    private GameEvent _footstep;
    [SerializeField]
    private GameEvent _whoosh;
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
}
