using UnityEngine;

public class AIPerception : MonoBehaviour
{
    [Header("events")]
    [SerializeField] private GameEvent _foundTargetEvent;
    [Header("Senses")]
    private Hearing _hearing;
    private Seeing _seeing;
    private GameObject _target;

    private void Start()
    {
        var state = GetComponent<StateManager>();
   
        _hearing = GetComponent<Hearing>();
        if (_hearing && state)
            _hearing.CharacterMask = state.TargetLayers;

        _seeing = GetComponent<Seeing>();
        if (_seeing && state)
            _seeing.CharacterMask = state.TargetLayers;

        if (_seeing == null && _hearing == null)
            Debug.LogError("no senses inserted in AIPerception");
    }

    public void CheckSurrounding(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        var args = obj as OrientationEventArgs;
        if (args == null) return;

        GameObject target = null;

        //if (sender.gameObject.name == "Player")
        //{
        //    Debug.Log("player is looking for target");
        //}

        target = _hearing.HearSurrounding();

        if (target == null)
            target = _seeing.SeeSurrounding(args.NewOrientation);

        if (target != _target )
        {
            _target = target;
            _foundTargetEvent.Raise(this, new NewTargetEventArgs { NewTarget = target });
        }
    }
}
