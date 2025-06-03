using UnityEngine;

public class QueuedAnimation : MonoBehaviour
{
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
                _changeAnimation.Raise(this, new AnimationEventArgs 
                { 
                    AnimState = AnimationState.SheathWeapon, AnimLayer = 3, DoResetIdle = true 
                });
                break;
            case SpecialInput.UnSheatSword:
                _changeAnimation.Raise(this, new AnimationEventArgs
                { 
                    AnimState = AnimationState.DrawWeapon, AnimLayer = 3, DoResetIdle = true 
                });
                break;
            default:
                return;
        }

    }

    public void Pickup(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        var args = obj as AimingOutputArgs;
        if (args == null) return;
        if (args.Special != SpecialInput.PickUp) return;

        _changeAnimation.Raise(this, new AnimationEventArgs
        { 
            AnimState = AnimationState.PickUp, AnimLayer = 3, DoResetIdle = true 
        });       
    }

    public void PlayQueuedAnim(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        var args = obj as AimingOutputArgs;
        if (args == null) return;
        if ((int)args.Special < 100) return;

        AnimationState state = (AnimationState)args.Special;

        _changeAnimation.Raise(this, new AnimationEventArgs 
        { 
            AnimState = state, AnimLayer = 1, DoResetIdle = true, IsFullBodyAnim = true
        });       

    }

}
