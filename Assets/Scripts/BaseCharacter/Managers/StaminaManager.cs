using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    private const string PLAYER = "Player";

    [HideInInspector]
    public float CurrentStamina;

    [Header("Stamina")]
    [SerializeField]
    private float _maxStamina;

    [Header("Regen")]
    [SerializeField]
    private float _regenSpeed;

    [Header("Events")]
    [SerializeField]
    private GameEvent _changedStamina;

    private BlackboardReference _blackboards;

    private bool _canRegen;
    private Coroutine _resetRegen;


    private void Awake()
    {
        _blackboards = GetComponent<StateManager>().BlackboardRef;
    }
    private void Start()
    {
        CurrentStamina = _maxStamina;
        UpdateBlackboard();
    }

    private void Update()
    {
        if(_canRegen) RegenStamina();
    }

    private void RegenStamina()
    {
        CurrentStamina += _regenSpeed * Time.deltaTime;
        _changedStamina.Raise(this, new StaminaEventArgs { CurrentStamina = CurrentStamina, MaxStamina = _maxStamina });
        if (CurrentStamina <= _maxStamina) return;
        _canRegen = false;

        UpdateBlackboard();
    }

    public void LoseStamina(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        StaminaEventArgs staminaLos = obj as StaminaEventArgs;
        if (staminaLos == null) return;

        if (CurrentStamina > staminaLos.StaminaCost)
            CurrentStamina -= staminaLos.StaminaCost;
        else
            CurrentStamina = 0f;
       
        _changedStamina.Raise(this, new StaminaEventArgs { CurrentStamina = CurrentStamina, MaxStamina = _maxStamina });

        UpdateBlackboard();
        _canRegen = false;
        

        if (_resetRegen != null) StopCoroutine(_resetRegen);
        _resetRegen = StartCoroutine(ResetCanRegen());
    }

    private void UpdateBlackboard()
    {
       _blackboards.variable.Stamina = CurrentStamina / _maxStamina;
    }

    private IEnumerator ResetCanRegen()
    {
        yield return new WaitForSeconds(2);

        _canRegen = true;
    }

    public float GetStaminaPercentage()
    {
        return CurrentStamina / _maxStamina;
    }
}
