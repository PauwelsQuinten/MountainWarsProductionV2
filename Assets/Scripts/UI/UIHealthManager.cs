using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.GPUSort;

public class UIHealthManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private Image _healthBar;
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
    private GameObject _cameraToLookAt;
    [SerializeField]
    private GameObject _canvas;
    [SerializeField]
    private GameEvent _gameLost;

    public void Update()
    {
        if (gameObject.GetComponent<AIController>() != null)
        {
            if (_cameraToLookAt == null) return;
            _canvas.transform.LookAt(_cameraToLookAt.transform);
        }
    }
    public void UpdateHealth(Component sender, object obj)
    {
        HealthEventArgs args = obj as HealthEventArgs;
        if (args == null) return;

        if(sender.gameObject.GetComponent<PlayerController>() == null)
        {
            if (sender.gameObject != gameObject) return;
        }
        else
        {
            if (gameObject.GetComponent<AIController>() != null) return;
        }

        float fillAmount = args.CurrentHealth / args.MaxHealth;
        _healthBar.fillAmount = fillAmount;

        if (sender.gameObject.GetComponent<PlayerController>() != null)
            UpdateBodyPartColor(sender, args);

        if (args.CurrentHealth > 0) return;

        if (sender.gameObject.GetComponent<PlayerController>() == null)
        {
            Destroy(this.gameObject);
            return;
        }
        _gameLost.Raise(this, EventArgs.Empty);
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
        }
        else
        {
            if (gameObject.GetComponent<AIController>() != null) return;
        }
        float barFill = args.CurrentBlood / args.MaxBlood;
        _bloodBar.fillAmount = barFill;

        if (args.CurrentBlood <= 0 && gameObject.CompareTag("Player"))
            _gameLost.Raise(this, EventArgs.Empty);

        //if (sender.gameObject.GetComponent<PlayerController>() == null)
        //{
        //    Destroy(this.gameObject);
        //    return;
        //}
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
        while(_patchUpBar.fillAmount < 1)
        {
            time += Time.deltaTime;
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
}