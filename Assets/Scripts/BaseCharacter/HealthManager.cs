using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    private const string PLAYER = "Player";

    [Header("health")]
    [SerializeField, Tooltip("Used to set the base limb health")]
    private float _maxBaseLimbHealth;
    [SerializeField, Tooltip("The rate at which health regens")]
    private float _regenRate;
    [SerializeField]
    private GameEvent _changedHealth;

    [Header("Blood")]
    private float _currentBlood;
    [SerializeField, Tooltip("The max amount of blood the character starts with")]
    [Range(1f, 100f)]
    private float _maxBlood;
    [SerializeField, Tooltip("The rate at which you will lose blood")]
    private float _bleedOutSpeed;
    [SerializeField]
    private GameEvent _changedBlood;

    [Header("Damage")]
    [SerializeField, Tooltip("How much the damage drops when hitting a second limb")]
    private float _damageDropOff;
    [SerializeField, Tooltip("How much the damage increases when you already were stunned")]
    private float _damageIncreaseWhenStunned = 1.25f;

    [Header("Healing")]
    [SerializeField, Tooltip("How long it takes to patch up your bleeding")]
    private FloatReference _patchUpSpeed;
    [SerializeField]
    private float _timeBeforeRegerating = 2f;
    [SerializeField]
    private GameEvent _patchUp;

    [Header("Managers")]
    [SerializeField]
    private StateManager _stateManager;

    [Header("Blackboard")]
    [SerializeField]
    private BlackboardReference _blackboard;


    private float _currentHealth;
    private float _maxHealth;

    private Dictionary<BodyParts, float> _bodyPartHealth = new Dictionary<BodyParts, float>();
    private Dictionary<BodyParts, float> _maxBodyPartHealth = new Dictionary<BodyParts, float>();
    private float _bleedOutRate;

    private bool _canRegenBlood = false;
    private bool _canRegenHealth = false;
    private Coroutine _canRegenCoroutine;
    private Coroutine _patchUpRoutine;

    private List<BodyParts> _damagedBodyParts = new List<BodyParts>();

    private bool _isBleeding;

    private void Start()
    {
        SetHealth();

        UpdateBlackboard();
    }

    private void Update()
    {
        if(_bleedOutRate > 0 && _isBleeding) 
            LoseBlood();

        if(_canRegenHealth) 
            RegenHealth();
        if(_canRegenBlood && !_isBleeding) 
            RegenBlood();
    }

    public void TakeDamage(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        DamageEventArgs args = obj as DamageEventArgs;
        if (args == null) return;

        LoseHealth(args.AttackPower, args);

        if (_patchUpRoutine != null)
        {
            StopCoroutine(_patchUpRoutine);
            _patchUpRoutine = null;
        }

        UpdateBlackboard();
    }

    private void UpdateBlackboard()
    {
        //Update blackboard
        if (gameObject.CompareTag(PLAYER))
        {
            _blackboard.variable.TargetHealth = _currentHealth / _maxHealth;
            _blackboard.variable.TargetIsBleeding = _isBleeding;
        }
        else
        {
            _blackboard.variable.Health = _currentHealth / _maxHealth;
            _blackboard.variable.IsBleeding = _isBleeding;
        }
    }

    private void SetHealth()
    {
        _bodyPartHealth.Add(BodyParts.Head, _maxBaseLimbHealth * 0.75f);
        _bodyPartHealth.Add(BodyParts.Torso, _maxBaseLimbHealth * 1.5f);
        _bodyPartHealth.Add(BodyParts.LeftArm, _maxBaseLimbHealth);
        _bodyPartHealth.Add(BodyParts.RightArm, _maxBaseLimbHealth);
        _bodyPartHealth.Add(BodyParts.LeftLeg, _maxBaseLimbHealth);
        _bodyPartHealth.Add(BodyParts.RightLeg, _maxBaseLimbHealth);

        foreach(var part in _bodyPartHealth)
        {
            _maxHealth += part.Value;
        }

        _currentHealth = _maxHealth;
        _maxBodyPartHealth = _bodyPartHealth.ToDictionary(entry => entry.Key, entry => entry.Value);
        _currentBlood = _maxBlood;
    }

    private void LoseHealth(float damage, DamageEventArgs args)
    {
        List<BodyParts> parts = args.HitParts;
        int index = 0;
        if (_stateManager.AttackState == AttackState.Stun)
            damage *= _damageIncreaseWhenStunned;
        int damageTaken = (int)damage;

        foreach (BodyParts part in parts)
        {
            if (_bodyPartHealth[part] > 0)
            {
                damageTaken -= (int)(index * _damageDropOff);
                _bodyPartHealth[part] -= damageTaken;
                _currentHealth -= damage;

                if (!_damagedBodyParts.Contains(part)) _damagedBodyParts.Add(part);

                index++;
                if(_changedHealth != null)_changedHealth.Raise
                    (this, new HealthEventArgs 
                    {   BodyPartsHealth = _bodyPartHealth, 
                        MaxBodyPartsHealth = _maxBodyPartHealth, 
                        CurrentHealth = _currentHealth, MaxHealth = _maxHealth,
                        DamagedBodyParts = _damagedBodyParts,
                    });

                if (_canRegenCoroutine != null) StopCoroutine(_canRegenCoroutine);
                _canRegenCoroutine = StartCoroutine(ResetCanRegen());
                _canRegenHealth = false;

                if (_bodyPartHealth[part] <= 0)
                {
                    if (part == BodyParts.Head)
                    {
                        _currentHealth = 0;
                        if (_changedHealth != null) _changedHealth.Raise
                            (this, new HealthEventArgs
                            {
                                BodyPartsHealth = _bodyPartHealth,
                                MaxBodyPartsHealth = _maxBodyPartHealth,
                                CurrentHealth = _currentHealth,
                                MaxHealth = _maxHealth,
                                DamagedBodyParts = _damagedBodyParts,
                            }); 
                    }
                    else if (part == BodyParts.Torso)
                        _bleedOutRate += _bleedOutSpeed * 1.5f;
                    else
                        _bleedOutRate += _bleedOutSpeed;

                    _canRegenBlood = false;
                    _isBleeding = true;
                    _stateManager.IsBleeding = _isBleeding;

                    if (!gameObject.CompareTag(PLAYER) && _currentHealth <= 0)
                        Destroy(gameObject);
                }
            }
            else
            {
                Debug.Log($"{part} has taken too much damage");
            }
        }
    }

    private void LoseBlood()
    {
        if (!_isBleeding)
        {
            _isBleeding = true;
            _stateManager.IsBleeding = _isBleeding;
        }
        if (_currentBlood <= 0)
        {
            _currentBlood = 0;
            return;
        }

        _currentBlood -= _bleedOutRate * Time.deltaTime;

        _changedBlood.Raise
                    (this, new BloodEventArgs
                    {
                        CurrentBlood = _currentBlood,
                        MaxBlood = _maxBlood,
                    });

        if(_canRegenCoroutine !=  null) StopCoroutine( _canRegenCoroutine );
        _canRegenCoroutine = StartCoroutine(ResetCanRegen());

        if (!gameObject.CompareTag(PLAYER) && _currentBlood <= 0)
            Destroy(gameObject);
    }

    private void RegenBlood()
    {
        float healing = _regenRate * 0.75f;
        _currentBlood += healing * Time.deltaTime;

        if (_currentBlood >= _maxBlood)
        {
            _currentBlood = _maxBlood;
        }

        _changedBlood.Raise
            (this, new BloodEventArgs
            {
                CurrentBlood = _currentBlood,
                MaxBlood = _maxBlood,
            });  
    }

    private void RegenHealth()
    {
        float healing = _regenRate;
        _currentHealth += healing * Time.deltaTime;

        foreach(BodyParts part in _damagedBodyParts)
        {
            _bodyPartHealth[part] += healing / _damagedBodyParts.Count * Time.deltaTime;
            if (_bodyPartHealth[part] >= _maxBodyPartHealth[part]) _bodyPartHealth[part] = _maxBodyPartHealth[part];
        }

        if(_currentHealth >= _maxHealth)
        {
            _currentHealth = _maxHealth;
            _canRegenHealth = false;
        }

        _changedHealth.Raise
            (this, new HealthEventArgs
            {
                BodyPartsHealth = _bodyPartHealth,
                MaxBodyPartsHealth = _maxBodyPartHealth,
                CurrentHealth = _currentHealth,
                MaxHealth = _maxHealth,
                DamagedBodyParts = _damagedBodyParts
            });
    }

    public void PatchUpBleeding(Component sender, object obj)
    {
        if (!_isBleeding) return;
        if (sender.gameObject != gameObject) return;

        bool? canRegen = obj as bool?;

        if (!(bool)canRegen)
        {
            if (_patchUpRoutine == null)
                _patchUpRoutine = StartCoroutine(PatchUp());
            return;
        }
        _bleedOutRate = 0;
        _isBleeding = false;
        _stateManager.IsBleeding = _isBleeding;
        if (_canRegenCoroutine != null) StopCoroutine(_canRegenCoroutine);
        _canRegenCoroutine = StartCoroutine(ResetCanRegen());

        UpdateBlackboard();
    }

    private IEnumerator ResetCanRegen()
    {
        yield return new WaitForSeconds(_timeBeforeRegerating);
        if (_canRegenBlood && _currentBlood == _maxBlood) 
            _canRegenHealth = true;
        else 
        {
            if(_currentBlood != _maxBlood) 
                _canRegenBlood = true;
            StartCoroutine(ResetCanRegen());
        }
    }

    private IEnumerator PatchUp()
    {
        yield return new WaitForSeconds(_patchUpSpeed.value);
        PatchUpBleeding(this, true);
    }
}
