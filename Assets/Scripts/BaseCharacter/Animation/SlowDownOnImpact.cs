using System;
using System.Threading.Tasks;
using UnityEngine;

public class SlowDownOnImpact : MonoBehaviour
{
    [SerializeField, Tooltip("The time duration the animation will be slowed down")] private float _slowedDuration = 0.1f;
    [Range(0f, 0.5f)]
    [SerializeField, Tooltip("The set animationspeed for during slow down")] private float _slowSpeed = 0.1f;
    [Range(0f, 0.5f)]
    [SerializeField, Tooltip("The time percentage duration the animation will ease in or out")] private float _easInPercentage = 0.2f; 
    [SerializeField, Tooltip("The animator parameter name")] private string _animParameter = "ActionSpeed";
    [SerializeField, Tooltip("Slow down both the attacker and the defender")] private bool _slowBothDown = false;

    private Animator _animator;
    private float _easInDuration = 0.2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        if (_animator == null)
            Debug.LogError("no animator found in Slowdown component");
        _easInDuration = _slowedDuration * _easInPercentage;
    }

    public async void StartSlowDown(Component sender, object obj)
    {
        AttackEventArgs args = obj as AttackEventArgs;
        if (args == null) return;
        if (args.Attacker != gameObject && (args.Defender != gameObject && _slowBothDown)) return;

        Debug.Log("start slow down hit");
        float start = _animator.GetFloat(_animParameter);
        float miliseconds = (_slowedDuration - 2f * _easInDuration) * 1000f;

        await EaseAnimSpeed(start, _slowSpeed, _easInDuration);
        //Debug.Log("Set speed slow");

        await Task.Delay(Mathf.RoundToInt(miliseconds));
        await EaseAnimSpeed(_slowSpeed, start, _easInDuration);
        Debug.Log("end slow down hit");
    }

    private async Task EaseAnimSpeed(float start, float end, float duration)
    {
        float elapsedTime = 0f;

        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime/duration);

            float easedValue = t < 0.5f ?
                2 * t * t :
                1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;

            float value = Mathf.Lerp(start, end, easedValue);
            if (_animator)
                _animator.SetFloat(_animParameter, value);
            await Task.Yield(); 
        }
        if (_animator)
            _animator.SetFloat(_animParameter, end);
    }


}
