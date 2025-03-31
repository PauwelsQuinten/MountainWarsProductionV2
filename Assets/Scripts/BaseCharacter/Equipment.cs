using UnityEngine;

public class Equipment : MonoBehaviour
{
    [SerializeField] private EquipmentType _type;
    public EquipmentType Type { get { return _type; } private set { _type = value; } }
    [SerializeField] private float _durability = 10f;
    public float Durability { get { return _durability; } set {_durability = value; } }
    private float _maxDurability = 0f;
    [SerializeField] private bool _isRightHandEquipment = false;
    public bool IsRightHandEquipment { get { return _isRightHandEquipment; } private set { _isRightHandEquipment = value; } }
    [SerializeField] private float _power = 1f;
    public float Power { get { return _power; } private set { _power = value; } }
    [SerializeField] private float _range = 1f;
    public float Range { get { return _range; } private set { _range = value; } }

    private void Awake()
    {
        _maxDurability = _durability;
    }

    public void Damage(float damage, BlockResult blockResult)
    {
        float damageMultiplier = 1f;
        switch (blockResult)
        {
            case BlockResult.Hit:
                damageMultiplier = 0f;
                break;
            case BlockResult.SwordBlock:
                damageMultiplier = 0.35f;
                break;
            case BlockResult.SwordHalfBlock:
                damageMultiplier = 0.25f;
                break;
            case BlockResult.HalfBlocked:
                damageMultiplier = 0.25f;
                break;
            case BlockResult.FullyBlocked:
                damageMultiplier = 0.5f;
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
