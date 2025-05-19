using UnityEngine;

public class DragShieldDown : MonoBehaviour
{
    [Header("State")]
    [SerializeField, Tooltip("the max distance for having your opponent drag your shield down")]
    private float _shieldGrabDistance = 1f;
    [Header("Events")]
    [SerializeField] private GameEvent _changeAnimation;
    [SerializeField] private GameEvent _lowerBlock;
    [Header("Stamina")]
    [SerializeField]
    private FloatReference _staminaCost;
    [SerializeField]
    private GameEvent _loseStamina;

    //Start the animation that grabs your opponent shield
    public void GrabShield(Component sender, object obj)
    {
        if (sender.gameObject != this.gameObject) return;
        var args = obj as AimingOutputArgs;
        if (args.Special != SpecialInput.ShieldGrab) return;

        _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.DragShieldDown, AnimLayer = { 2,3 }, DoResetIdle = true});
        _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value });
    }

    // Event that lowers your shield, triggered from your opponent animation
    public void ShieldGrabbed(Component sender, object obj)
    {
        if (sender.gameObject == this.gameObject) return;
        if (Vector3.Distance(gameObject.transform.position, sender.gameObject.transform.position) > _shieldGrabDistance) return;
        _lowerBlock.Raise(this, new AimingOutputArgs { AttackSignal = AttackSignal.Idle, AttackState = AttackState.Idle});
        //_changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Empty, AnimLayer = 4, DoResetIdle = true, BlockDirection = 0 });
    }

}
