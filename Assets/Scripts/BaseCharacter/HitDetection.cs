using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HitDetection : MonoBehaviour
{
    [SerializeField] GameEvent _DealDamage;

    private List<BodyParts> _hitParts = new List<BodyParts>();

    public void DetectHit(Component sender, object obj)
    {
        if (sender.gameObject == gameObject) return;

        AttackEventArgs args = obj as AttackEventArgs;
        if (args == null) return;
        _hitParts = GetDamagedParts(args);
        _DealDamage.Raise(this, new DamageEventArgs { AttackPower = args.AttackPower, HitParts = _hitParts});
    }

    private List<BodyParts> GetDamagedParts(AttackEventArgs args)
    {
        _hitParts.Clear();
        List<BodyParts> parts = new List<BodyParts>();
        switch (args.AttackType)
        {
            case AttackType.Stab:

                switch (args.AttackHeight)
                {
                    case AttackHeight.Head:
                        parts.Add(BodyParts.Head);
                        break;
                    case AttackHeight.Torso:
                        parts.Add(BodyParts.Torso);
                        break;
                }
                break;
            case AttackType.HorizontalSlashToLeft:

                switch (args.AttackHeight)
                {
                    case AttackHeight.Head:
                        parts.Add(BodyParts.Head);
                        break;
                    case AttackHeight.Torso:
                        parts.Add(BodyParts.LeftArm);
                        break;
                }
                break;
            case AttackType.HorizontalSlashToRight:

                switch (args.AttackHeight)
                {
                    case AttackHeight.Head:
                        parts.Add(BodyParts.Head);
                        break;
                    case AttackHeight.Torso:
                        //TODO shield gets hit animation
                        break;
                }
                break;
        }
        return parts;
    }
}
