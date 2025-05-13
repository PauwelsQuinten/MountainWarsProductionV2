using UnityEngine;

public class SheatingSword : MonoBehaviour
{
    [SerializeField] private GameEvent _sheathWeapon;
    [SerializeField] private GameEvent _changeAnimation;

    private EquipmentManager _equipmentManager;

    private void Start()
    {
        _equipmentManager = GetComponent<EquipmentManager>();
        if (_equipmentManager == null)
            Debug.LogError("Missing equipmentManager in sheatSword component");
    }

    public void SheatSword(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        var args = obj as AimingOutputArgs;
        if (args == null) return;
        if (!_equipmentManager.HasEquipmentInHand(true)) return;

        switch(args.Special)
        {
            case SpecialInput.SheatSword:
                //_sheathWeapon.Raise(this, true);
                _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.SheathWeapon, AnimLayer = 3, DoResetIdle = true });
                break;
            case SpecialInput.UnSheatSword:
                //_sheathWeapon.Raise(this, false);
                _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.DrawWeapon, AnimLayer = 3, DoResetIdle = true });
                break;
            default:
                return;
        }

    }
}
