using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PatchUp : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GameEvent _patchUpEvent;
    [SerializeField] private GameEvent _changeAnimation;

    [Header("variable")]
    [SerializeField, Tooltip("How long it takes to patch up your bleeding")]
    private FloatReference _patchUpDuration;

    private float _patchStartTime = 0f;
    private Coroutine _patchupRoutine;

    public void StartPatchUp(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        var args = obj as AimingOutputArgs;
        if (args == null || args.Special != SpecialInput.PatchUp) return;


        if (args.AnimationStart)
        {
            StartPatchingUp();
        }
        else
        {
            StopPatchingUp();
            if (_patchupRoutine != null) 
                StopCoroutine(_patchupRoutine);
        }
    }

    private void StartPatchingUp()
    {
        _patchStartTime = Time.time;
        _patchUpEvent.Raise(this, false);
        _changeAnimation.Raise(this, new AnimationEventArgs { AnimLayer = { 3 }, AnimState = AnimationState.PatchUp, DoResetIdle = true });

        _patchupRoutine = StartCoroutine(StartPatchingUp(_patchUpDuration.value));
    }

    private void StopPatchingUp()
    {
        if (_patchStartTime == 0f) return;

        float _patchEndTime = Time.time;
        float _patchTimer = _patchEndTime - _patchStartTime;

        if (_patchUpDuration.value <= _patchTimer)
            _patchUpEvent.Raise(this, true);
        else
            _patchUpEvent.Raise(this, false);

        _changeAnimation.Raise(this, new AnimationEventArgs { AnimLayer = { 3 }, AnimState = AnimationState.Empty, DoResetIdle = false });
        _patchStartTime = 0;
    }

    private IEnumerator StartPatchingUp(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopPatchingUp();
    }

}
