using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private Animator _animator;

    private AnimationState _currentState;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void ChangeAnimationState(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        AnimationState? animState = obj as AnimationState?;
        if (animState == null) return;
        if (_currentState == animState) return;

        _animator.CrossFade(animState.ToString(), 0.2f, -1, 0f);

        _currentState = animState.Value;
    }
}
