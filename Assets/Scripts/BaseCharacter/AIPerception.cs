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
        _hearing = GetComponent<Hearing>();
        _seeing = GetComponent<Seeing>();

        if (_seeing == null || _hearing == null)
            Debug.LogError("no senses inserted in AIPerception");
    }

    public void CheckSurrounding(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        var args = obj as OrientationEventArgs;
        if (args == null) return;

        GameObject target = null;

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
