using UnityEngine;

public class Equipment : MonoBehaviour
{
    [SerializeField] private EquipmentType _type;
    public EquipmentType Type { get { return _type; } private set { _type = value; } }
    [SerializeField] private float _durability = 10f;
    public float Durability { get { return _durability; } set {_durability = value; } }
    private float _maxDurability = 0f;
    [SerializeField] private EquipmentHand _equipmentHand = EquipmentHand.RightHand;
    public EquipmentHand EquipmentHand { get { return _equipmentHand; } private set { _equipmentHand = value; } }
    [SerializeField] private float _power = 1f;
    public float Power { get { return _power; } private set { _power = value; } }
    [SerializeField] private float _range = 1f;
    public float Range { get { return _range; } private set { _range = value; } }

    private void Awake()
    {
        _maxDurability = _durability;
    }

    public void Damage(float damage, BlockResult blockResult, bool wasDefending)
    {
        float damageMultiplier = 1f;
        switch (blockResult)
        {
            case BlockResult.Hit:
                damageMultiplier = 0f;
                break;
            case BlockResult.SwordBlock:
                if (wasDefending)
                    damageMultiplier = 0.25f;
                else
                    damageMultiplier = 0.35f;
                break;
            case BlockResult.SwordHalfBlock:
                if (wasDefending)
                    damageMultiplier = 0.25f;
                else
                    damageMultiplier = 0.35f;
                break;
            case BlockResult.HalfBlocked:
                if (wasDefending)
                    damageMultiplier = 0.35f;
                else
                    damageMultiplier = 0.15f;
                break;
            case BlockResult.FullyBlocked:
                if (wasDefending)
                    damageMultiplier = 0.15f;
                else
                    damageMultiplier = 0.35f;
                break;
            case BlockResult.Parried:
                damageMultiplier = 0f;
                break;
        }

        _durability -= damage * damageMultiplier;
    }

    public float GetDurabilityPercentage()
    {
        return _durability / _maxDurability;
    }

}
