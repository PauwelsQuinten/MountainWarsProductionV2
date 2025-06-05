using System.Collections.Generic;
using UnityEngine;

public static class EquipmentHelper 
{
    private static Quaternion _swordStartRotation = Quaternion.Euler(-32f, -116f, -195f);

    private const int LEFT_HAND = 0;
    private const int RIGHT_HAND = 1;
    private const int FISTS = 2;

    public static void EquipEquipment(List<Equipment> HeldEquipment, Equipment newEquip, EquipmentHand hand, Transform socket )
    {
        int index = 0;
        switch (hand)
        {
            case EquipmentHand.LeftHand:
                DropEquipment(HeldEquipment, LEFT_HAND);
                index = LEFT_HAND;
                break;
            case EquipmentHand.RightHand:
                DropEquipment(HeldEquipment, RIGHT_HAND);
                index = RIGHT_HAND;
                break;
            case EquipmentHand.TwoHanded:
                DropEquipment(HeldEquipment, LEFT_HAND);
                DropEquipment(HeldEquipment, RIGHT_HAND);
                index = RIGHT_HAND;
                break;
        }

        HeldEquipment[index] = newEquip;
        newEquip.transform.parent = socket;
        newEquip.transform.localPosition = Vector3.zero;
        newEquip.transform.localRotation = Quaternion.identity;

        SetPhysics(newEquip, true);

    }


    public static void DropEquipment(List<Equipment> HeldEquipment, int hand)
    {

        if (HeldEquipment[hand] == null)
            return;
        HeldEquipment[hand].transform.parent = null;

        SetPhysics(HeldEquipment[hand], false);

        HeldEquipment[hand] = null;
    }

    public static void CreateAndEquip(List<Equipment> HeldEquipment, Equipment Equipment, int hand, Transform socket, Transform parent)
    {
        //var leftEquipment = Instantiate(EquipmentPrefab);
        if (socket)
            Equipment.transform.parent = socket;
        else
            Equipment.transform.parent = parent;

        Equipment.transform.localPosition = Vector3.zero;
        HeldEquipment[hand] = Equipment;
        SetPhysics(HeldEquipment[hand], true);
    }

    public static bool CheckIfBroken(DefenceEventArgs args, int index, List<Equipment> HeldEquipment, out LoseEquipmentEventArgs package)
    {
        //Check if broken
        if (HeldEquipment[index].Durability < 0f)
        {
            Debug.Log($"!!!!!!!!!!!!!!!!!!!!! breaks {HeldEquipment[index]} !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

            package = new LoseEquipmentEventArgs
            {
                EquipmentType = args.BlockMedium == BlockMedium.Shield ? EquipmentType.Shield : EquipmentType.Melee,
                ToSelf = true
            };
            return true;
        }
        package = null;
        return false;
    }

    private static void SetPhysics(Equipment Equipment, bool setForHolding)
    {
        var collider = Equipment.GetComponent<CapsuleCollider>();
        if (collider)
        {
            collider.enabled = !setForHolding;
            //collider.isTrigger = true;
        }
        var rb = Equipment.GetComponent<Rigidbody>();
        if (rb)
            rb.isKinematic = setForHolding;
    }

}
