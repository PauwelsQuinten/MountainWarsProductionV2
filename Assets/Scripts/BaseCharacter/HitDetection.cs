using System.Collections.Generic;
using UnityEngine;

public class HitDetection : MonoBehaviour
{
    [SerializeField] GameEvent _DealDamage;
    [SerializeField] GameEvent _vfx;

    private List<BodyParts> _hitParts = new List<BodyParts>();

    public void DetectHit(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        AttackEventArgs args = obj as AttackEventArgs;
        if (args == null) return;
        _hitParts = GetDamagedParts(args);
        _DealDamage.Raise(this, new DamageEventArgs { AttackPower = args.AttackPower, HitParts = _hitParts});
        _vfx?.Raise(this, new VfxEventArgs { Type = VfxType.Hit});
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
                        //TODO shield gets hit animation
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
                        parts.Add(BodyParts.RightArm);
                        break;
                }
                break;
        }
        return parts;
    }
}
