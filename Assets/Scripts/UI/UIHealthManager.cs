using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.GPUSort;

public class UIHealthManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private Image _healthBar;
    [SerializeField]
    private FloatReference _patchupDuration;
    [SerializeField] 
    private Color _fullHealthColor;
    [SerializeField]
    private Color _noHealthColor;

    [Header("Blood")]
    [SerializeField]
    private Image  _bloodBar;
    [SerializeField]
    private Image _patchUpBar;
 
    [Header("BodyParts")]
    [SerializeField]
    private List<Image> _bodyParts = new List<Image>();

    [Header("Stamina")]
    [SerializeField]
    private Image _staminaBar;

    private Coroutine _patchUp;
    private bool _completedPatchUp;

    [Header("Equipment")]
    [SerializeField]
    private Image _shield;
    [SerializeField]
    private Image _weapon;

    [SerializeField]
    private GameObject _canvas;
    [SerializeField]
    private GameEvent _characterDeath;

    [Header("Dialogue")]
    [SerializeField]
    GameEvent _dialogueTrigger;
    [SerializeField]
    private bool _triggerDialogueOnDeath;
    [SerializeField]
    private int _dialogueToTrigger;

    private Coroutine _dissableHealth;

    private string _name;

    private StateManager _playerStateManager;

    public void Update()
    {
        if(_playerStateManager == null)
        {
            _playerStateManager = GameObject.Find("Player").GetComponent<StateManager>();
        }
        if (gameObject.GetComponent<AIController>() != null)
        {
            if (_playerStateManager.CurrentCamera == null) return;
            _canvas.transform.LookAt(_playerStateManager.CurrentCamera.transform);
        }
    }
    public void UpdateHealth(Component sender, object obj)
    {
        HealthEventArgs args = obj as HealthEventArgs;
        if (args == null) return;

        if(sender.gameObject.GetComponent<PlayerController>() == null)
        {
            if (sender.gameObject != gameObject) return;
            _name = gameObject.name;

            _bloodBar.transform.parent.gameObject.SetActive(true);
            _healthBar.transform.parent.gameObject.SetActive(true);

            if (_dissableHealth != null) StopCoroutine(_dissableHealth);
            _dissableHealth = StartCoroutine(DissableHealthUI());

            //if (args.CurrentHealth < 0)
            //    _characterDeath.Raise(this, new CharacterDeathEventArgs{ CharacterName = _name});
        }
        else
        {
            if (gameObject.GetComponent<AIController>() != null) return;
            _name = "Player";
        }

        float fillAmount = args.CurrentHealth / args.MaxHealth;
        _healthBar.fillAmount = fillAmount;

        if (sender.gameObject.GetComponent<PlayerController>() != null)
            UpdateBodyPartColor(sender, args);

        if (args.CurrentHealth > 0) return;
        if (_triggerDialogueOnDeath)
            _dialogueTrigger.Raise(this, new DialogueTriggerEventArgs { NextDialogueIndex = _dialogueToTrigger });
        _characterDeath.Raise(this, new CharacterDeathEventArgs { CharacterName = _name });
    }

    private void UpdateBodyPartColor(Component sender, HealthEventArgs args)
    {
        BodyParts? partToRemove = null;

        foreach (Image part in _bodyParts)
        {
            foreach (var damagedPart in args.DamagedBodyParts)
            {
                if (part.name == damagedPart.ToString())
                {
                    float progress = args.BodyPartsHealth[damagedPart] / args.MaxBodyPartsHealth[damagedPart];
                    Color newColor = Color.Lerp(_noHealthColor, _fullHealthColor, progress);

                    if (args.BodyPartsHealth[damagedPart] >= args.MaxBodyPartsHealth[damagedPart])
                    {
                        partToRemove = damagedPart;
                    }

                    part.color = newColor;
                }
            }
        }

        if(partToRemove != null) 
            args.DamagedBodyParts.Remove(partToRemove.GetValueOrDefault());
    }

    public void UpdateBlood(Component sender, object obj)
    {
        BloodEventArgs args = obj as BloodEventArgs;
        if (args == null) return;

        if (sender.gameObject.GetComponent<PlayerController>() == null)
        {
            if (sender.gameObject != gameObject) return;
            _name = gameObject.name;

            _bloodBar.transform.parent.gameObject.SetActive(true);
            _healthBar.transform.parent.gameObject.SetActive(true);

            if (_dissableHealth != null) StopCoroutine(_dissableHealth);

            //if (args.CurrentBlood < 0)
            //    _characterDeath.Raise(this, new CharacterDeathEventArgs { CharacterName = _name });
        }
        else
        {
            if (gameObject.GetComponent<AIController>() != null) return;
            _name = "Player";
        }
        float barFill = args.CurrentBlood / args.MaxBlood;
        _bloodBar.fillAmount = barFill;

        if (args.CurrentBlood > 0) return;

        if (_triggerDialogueOnDeath)
            _dialogueTrigger.Raise(this, new DialogueTriggerEventArgs { NextDialogueIndex = _dialogueToTrigger });
        _characterDeath.Raise(this, new CharacterDeathEventArgs { CharacterName = _name });
    }

    public void UpdatePatchUp(Component sender, object obj)
    {
        if (sender.gameObject.GetComponent<PlayerController>() == null) return;

        if (_completedPatchUp)
        {
            _completedPatchUp = false;
            return;
        }
        if(_patchUp == null) 
            _patchUp = StartCoroutine(PathUpBar());

        bool? canReset = obj as bool?;
        if((bool)canReset)
        {
            StopCoroutine(_patchUp);

            _patchUpBar.transform.parent.gameObject.SetActive(false);
            float barFill = 0;
            _patchUpBar.fillAmount = barFill;

            _patchUp = null;
        }
    } 

    public void UpdateStamina(Component sender, object obj)
    {
        StaminaEventArgs args = obj as StaminaEventArgs;
        if (args == null) return;

        if (sender.gameObject.GetComponent<PlayerController>() == null) return;

        float barFill = args.CurrentStamina / args.MaxStamina;
        _staminaBar.fillAmount = barFill;
    }

    public void UpdateEquipment(Component sender, object obj)
    {
        EquipmentEventArgs args = obj as EquipmentEventArgs;
        if (args == null) return;

        if (sender.gameObject.GetComponent<PlayerController>() == null) return;


        float progress = args.ShieldDurability;
        Color newColor = Color.Lerp(_noHealthColor, _fullHealthColor, progress);
        if (progress <= 0f)
        {
            _shield.enabled = false;
        }
        else
        {
            if (!_shield.enabled)
                _shield.enabled = true;
            _shield.color = newColor;
        }

        progress = args.WeaponDurability;
        newColor = Color.Lerp(_noHealthColor, _fullHealthColor, progress);
        if (progress <= 0f)
        {
            _weapon.enabled = false;
        }
        else
        {
            if (!_weapon.enabled)
                _weapon.enabled = true;
            _weapon.color = newColor;
        }
    }

    private IEnumerator PathUpBar()
    {
        float time = 0;
        float size = 0;
        _patchUpBar.transform.parent.gameObject.SetActive(true);
        while(_patchUpBar != null && _patchUpBar.fillAmount < 1)
        {
            if (this == null) yield break;
            time += Time.deltaTime / _patchupDuration.value;
            size = time;
            _patchUpBar.fillAmount = size;
            yield return null;
        }

        _patchUpBar.transform.parent.gameObject.SetActive(false);
        size = 0;
        _patchUpBar.fillAmount = size;
        _completedPatchUp = true;
        _patchUp = null;
    }

    private IEnumerator DissableHealthUI()
    {
         yield return new WaitForSeconds(10);
        if (this == null || _bloodBar == null || _healthBar) yield break;
        _bloodBar.transform.parent.gameObject.SetActive(false);
        _healthBar.transform.parent.gameObject.SetActive(false);
    }
}