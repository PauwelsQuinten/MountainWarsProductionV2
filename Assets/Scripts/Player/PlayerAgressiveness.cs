using UnityEngine;

public class PlayerAgressiveness : MonoBehaviour
{
    private float _currentTime = 0f;
    private bool _isAgressive = false;
    [SerializeField]
    private BlackboardReference _blackboard;
    [SerializeField]
    private int _maxAttackTime = 4;

    void Update()
    {
        if (_currentTime < _maxAttackTime)
        {
            _currentTime += Time.deltaTime;
        }

        if (_isAgressive && _currentTime >= _maxAttackTime)
        {
            _isAgressive = false;
            _blackboard.variable.IsPlayerAgressive = _isAgressive;
        }
    }

    public void UpdateAttackCounter(Component sender, object obj)
    {
        _currentTime -= 1f;
        if (_currentTime < 0f)
        {
            _currentTime = 0f;
            _isAgressive = true;
            _blackboard.variable.IsPlayerAgressive = _isAgressive;
        }
    }

}
